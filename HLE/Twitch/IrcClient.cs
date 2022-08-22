using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HLE.Time;

namespace HLE.Twitch;

/// <summary>
/// The base class for IRC clients.
/// </summary>
public abstract class IrcClient
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
    public event EventHandler<string>? OnDataReceived;

    /// <summary>
    /// Is invoked if the client sends data.
    /// </summary>
    public event EventHandler<string>? OnDataSent;

    private protected readonly CancellationTokenSource _tokenSource = new();
    private protected readonly CancellationToken _token;
    private protected readonly (string Url, int Port) _url;

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
        // ReSharper disable once VirtualMemberCallInConstructor
        _url = GetUrl();
    }

    /// <summary>
    /// Connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public void Connect(IEnumerable<string> channels)
    {
        async Task ConnectLocal()
        {
            await ConnectClient();
            OnConnected?.Invoke(this, EventArgs.Empty);
            StartListening();
            if (OAuthToken is not null)
            {
                await Send($"PASS {OAuthToken}");
            }

            await Send($"NICK {Username}");
            await Send("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            await JoinChannels(channels);
        }

        Task.Run(ConnectLocal, _token).Wait(_token);
    }

    /// <summary>
    /// Sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="message">The IRC message.</param>
    public void SendRaw(string message)
    {
        Send(message).Wait(_token);
    }

    /// <summary>
    /// Sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public void SendMessage(string channel, string message)
    {
        async Task SendLocal()
        {
            await Send($"PRIVMSG {channel} :{message}");
        }

        Task.Run(SendLocal, _token).Wait(_token);
    }

    /// <summary>
    /// Joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public void JoinChannel(string channel)
    {
        async Task JoinChannelLocal()
        {
            await Send($"JOIN {channel}");
        }

        Task.Run(JoinChannelLocal, _token).Wait(_token);
    }

    /// <summary>
    /// Leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public void LeaveChannel(string channel)
    {
        async Task LeaveChannelLocal()
        {
            await Send($"PART {channel}");
        }

        Task.Run(LeaveChannelLocal, _token).Wait(_token);
    }

    /// <summary>
    /// Disconnects the client.
    /// </summary>
    /// <param name="closeMessage">A close message or reason.</param>
    public void Disconnect(string closeMessage = "Manually closed")
    {
        async Task DisconnectLocal()
        {
            await DisconnectClient(closeMessage);
        }

        Task.Run(DisconnectLocal, _token).Wait(_token);
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private async Task JoinChannels(IEnumerable<string> channels)
    {
        string[] channelArr = channels.ToArray();
        if (channelArr.Length == 0)
        {
            return;
        }

        int maxChannels = IsVerifiedBot ? 200 : 20;
        // ReSharper disable once InconsistentNaming
        const short period = 10000;
        string[] joins = channelArr.Select(c => $"JOIN {c}").ToArray();
        long start = TimeHelper.Now();
        for (int i = 0; i < joins.Length; i++)
        {
            if (i > 0 && i % maxChannels == 0)
            {
                int waitTime = (int)(period - (TimeHelper.Now() - start));
                if (waitTime > 0)
                {
                    await Task.Delay(waitTime, _token);
                }

                start = TimeHelper.Now();
            }

            await Send(joins[i]);
        }
    }

    private protected void InvokeDataReceived(IrcClient sender, string message)
    {
        OnDataReceived?.Invoke(sender, message);
    }

    private protected void InvokeDataSent(IrcClient sender, string message)
    {
        OnDataSent?.Invoke(sender, message);
    }

    private protected abstract Task Send(string message);

    private protected abstract void StartListening();

    private protected abstract Task ConnectClient();

    private protected abstract Task DisconnectClient(string closeMessage);

    private protected abstract (string Url, int Port) GetUrl();
}
