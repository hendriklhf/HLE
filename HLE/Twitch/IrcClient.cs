using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;

namespace HLE.Twitch;

/// <summary>
/// The base class for IRC clients.
/// </summary>
public abstract class IrcClient : IDisposable
{
    /// <summary>
    /// The username of the client.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The OAuth token of the user. If null, the client is connected anonymously.
    /// </summary>
    public string? OAuthToken { get; }

    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL { get; init; }

    /// <summary>
    /// Indicates whether the bot is verified or not. If your bot is verified you can set this to true. Verified bots have higher rate limits.
    /// </summary>
    public bool IsVerifiedBot { get; init; }

    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Is invoked if the client connects.
    /// </summary>
    public event EventHandler? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event EventHandler? OnDisconnected;

    /// <summary>
    /// Is invoked if the client receives data.
    /// </summary>
    public event EventHandler<ReadOnlyMemory<char>>? OnDataReceived;

    /// <summary>
    /// Is invoked if the client sends data.
    /// </summary>
    public event EventHandler<ReadOnlyMemory<char>>? OnDataSent;

    private protected CancellationTokenSource _tokenSource = new();
    private protected CancellationToken _token;
    private protected readonly (string Url, int Port) _url;
    private protected readonly ArrayPool<byte> _byteArrayPool = ArrayPool<byte>.Create();
    private protected readonly ArrayPool<char> _charArrayPool = ArrayPool<char>.Create();

    private protected const string _newLine = "\r\n";

    /// <summary>
    /// The default constructor of the base <see cref="IrcClient"/>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    protected IrcClient(string username, string? oAuthToken = null)
    {
        Username = username;
        OAuthToken = oAuthToken;
        _token = _tokenSource.Token;
        _url = GetUrl();
    }

    /// <summary>
    /// Connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public void Connect(IEnumerable<string> channels)
    {
        Connect(channels.ToArray().AsMemory());
    }

    /// <summary>
    /// Connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public void Connect(string[] channels)
    {
        Connect(channels.AsMemory());
    }

    /// <summary>
    /// Connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public void Connect(List<string> channels)
    {
        Connect(CollectionsMarshal.AsSpan(channels).AsMemory());
    }

    /// <summary>
    /// Connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public void Connect(ReadOnlyMemory<string> channels)
    {
        async ValueTask ConnectAsync()
        {
            await ConnectClient();
            if (OAuthToken is not null)
            {
                await Send($"PASS {OAuthToken}");
            }

            await Send($"NICK {Username}");
            await Send("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            OnConnected?.Invoke(this, EventArgs.Empty);
            StartListening();
            await JoinChannels(channels);
        }

        Task.Run(ConnectAsync, _token).Wait(_token);
    }

    /// <summary>
    /// Disconnects the client.
    /// </summary>
    /// <param name="closeMessage">A close message or reason.</param>
    public void Disconnect(string closeMessage = "Manually closed")
    {
        async ValueTask DisconnectAsync()
        {
            await DisconnectClient(closeMessage);
        }

        RequestCancellation();
        Task.Run(DisconnectAsync, _token).Wait(_token);
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }

    internal void Reconnect(ReadOnlyMemory<string> channels)
    {
        Disconnect();
        Connect(channels);
    }

    /// <summary>
    /// Sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="rawMessage">The IRC message.</param>
    public void SendRaw(string rawMessage)
    {
        async ValueTask SendAsync()
        {
            await Send(rawMessage);
        }

        Task.Run(SendAsync, _token).Wait(_token);
    }

    /// <summary>
    /// Sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public void SendMessage(string channel, string message)
    {
        async ValueTask SendAsync()
        {
            await Send($"PRIVMSG {channel} :{message}");
        }

        Task.Run(SendAsync, _token).Wait(_token);
    }

    /// <summary>
    /// Joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public void JoinChannel(string channel)
    {
        async ValueTask JoinChannelAsync()
        {
            await Send($"JOIN {channel}");
        }

        Task.Run(JoinChannelAsync, _token).Wait(_token);
    }

    /// <summary>
    /// Leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public void LeaveChannel(string channel)
    {
        async ValueTask LeaveChannelAsync()
        {
            await Send("PART " + channel);
        }

        Task.Run(LeaveChannelAsync, _token).Wait(_token);
    }

    private async ValueTask JoinChannels(ReadOnlyMemory<string> channels)
    {
        if (channels.Length == 0)
        {
            return;
        }

        int maxChannels = IsVerifiedBot ? 200 : 20;
        const short period = 10000;

        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        for (int i = 0; i < channels.Length; i++)
        {
            if (i > 0 && i % maxChannels == 0)
            {
                int waitTime = (int)(period - (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start));
                if (waitTime > 0)
                {
                    await Task.Delay(waitTime, _token);
                }

                start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            await Send("JOIN " + channels.Span[i]);
        }
    }

    private void RequestCancellation()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _tokenSource = new();
        _token = _tokenSource.Token;
    }

    private protected void InvokeDataReceived(IrcClient sender, ReadOnlyMemory<char> data)
    {
        OnDataReceived?.Invoke(sender, data);
    }

    private protected void InvokeDataSent(IrcClient sender, string data)
    {
        OnDataSent?.Invoke(sender, data.AsMemory());
    }

    private protected void InvokeDataSent(IrcClient sender, ReadOnlyMemory<char> data)
    {
        OnDataSent?.Invoke(sender, data);
    }

    private protected abstract ValueTask Send(string message);

    private protected abstract ValueTask Send(ReadOnlyMemory<char> message);

    private protected abstract void StartListening();

    private protected abstract ValueTask ConnectClient();

    private protected abstract ValueTask DisconnectClient(string closeMessage);

    private protected abstract (string Url, int Port) GetUrl();

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _tokenSource.Dispose();
    }
}
