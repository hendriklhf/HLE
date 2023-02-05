using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HLE.Twitch.Models;

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
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL { get; }

    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public abstract bool IsConnected { get; }

    /// <summary>
    /// Is invoked if the client connects to the server.
    /// </summary>
    public event EventHandler? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event EventHandler? OnDisconnected;

    /// <summary>
    /// Is invoked if the client receives data. If this event is subscribed to, the <see cref="ReceivedData"/> instance has to be manually disposed.
    /// Read more in the documentation of the <see cref="ReceivedData"/> class.
    /// </summary>
    public event EventHandler<ReceivedData>? OnDataReceived;

    private protected readonly string? _oAuthToken;
    private protected readonly (string Url, int Port) _url;

    private protected const string _newLine = "\r\n";

    private readonly bool _isVerifiedBot;

    private const string _passPrefix = "PASS ";
    private const string _nickPrefix = "NICK ";
    private const string _capReqMessage = "CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership";
    private const string _privMsgPrefix = "PRIVMSG ";
    private const string _joinPrefix = "JOIN ";
    private const string _partPrefix = "PART ";
    private const byte _maxChannelNameLength = 26; // 25 for the name + 1 for the '#'
    private const ushort _maxMessageLength = 500;

    /// <summary>
    /// The default constructor of the base <see cref="IrcClient"/>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    protected IrcClient(string username, string? oAuthToken = null, ClientOptions options = default)
    {
        Username = username;
        _oAuthToken = oAuthToken;
        UseSSL = options.UseSSL;
        _isVerifiedBot = options.IsVerifiedBot;
        _url = GetUrl();
    }

    /// <inheritdoc cref="Connect(ReadOnlyMemory{string})"/>
    public void Connect(IEnumerable<string> channels)
    {
        Connect(channels.ToArray().AsMemory());
    }

    /// <inheritdoc cref="Connect(ReadOnlyMemory{string})"/>
    public void Connect(string[] channels)
    {
        Connect(channels.AsMemory());
    }

    /// <inheritdoc cref="Connect(ReadOnlyMemory{string})"/>
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
        ConnectAsync(channels);
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    public async Task ConnectAsync(IEnumerable<string> channels)
    {
        await ConnectAsync(channels.ToArray().AsMemory());
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    public async Task ConnectAsync(string[] channels)
    {
        await ConnectAsync(channels.AsMemory());
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    public async Task ConnectAsync(List<string> channels)
    {
        await ConnectAsync(CollectionsMarshal.AsSpan(channels).AsMemory());
    }

    /// <summary>
    /// Asynchronously connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public async Task ConnectAsync(ReadOnlyMemory<string> channels)
    {
        char[] rentedArray = ArrayPool<char>.Shared.Rent(_capReqMessage.Length);
        try
        {
            Memory<char> buffer = rentedArray;
            int bufferLength;

            await ConnectClient();
            StartListening();
            OnConnected?.Invoke(this, EventArgs.Empty);

            if (_oAuthToken is not null)
            {
                _passPrefix.CopyTo(buffer.Span);
                bufferLength = _passPrefix.Length;
                _oAuthToken.CopyTo(buffer.Span[bufferLength..]);
                bufferLength += _oAuthToken.Length;
                await Send(buffer[..bufferLength]);
            }

            _nickPrefix.CopyTo(buffer.Span);
            bufferLength = _nickPrefix.Length;
            Username.CopyTo(buffer.Span[bufferLength..]);
            bufferLength += Username.Length;
            await Send(buffer[..bufferLength]);

            _capReqMessage.CopyTo(buffer.Span);
            bufferLength = _capReqMessage.Length;
            await Send(buffer[..bufferLength]);

            await JoinChannelsThrottled(channels);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <summary>
    /// Disconnects the client.
    /// </summary>
    /// <param name="closeMessage">A close message or reason.</param>
    public void Disconnect(string closeMessage = "Manually closed")
    {
        DisconnectAsync(closeMessage);
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Asynchronously disconnects the client.
    /// </summary>
    /// <param name="closeMessage">A close message or reason.</param>
    public async Task DisconnectAsync(string closeMessage = "Manually closed")
    {
        await DisconnectClient(closeMessage);
    }

    internal void Reconnect(ReadOnlyMemory<string> channels)
    {
        ReconnectAsync(channels);
    }

    internal async Task ReconnectAsync(ReadOnlyMemory<string> channels)
    {
        await DisconnectAsync();
        await ConnectAsync(channels);
    }

    /// <inheritdoc cref="SendRaw(ReadOnlyMemory{char})"/>
    public void SendRaw(string rawMessage)
    {
        SendRaw(rawMessage.AsMemory());
    }

    /// <summary>
    /// Sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="rawMessage">The IRC message.</param>
    public void SendRaw(ReadOnlyMemory<char> rawMessage)
    {
        SendRawAsync(rawMessage);
    }

    /// <inheritdoc cref="SendRawAsync(ReadOnlyMemory{char})"/>
    public async Task SendRawAsync(string rawMessage)
    {
        await Send(rawMessage.AsMemory());
    }

    /// <summary>
    /// Asynchronously sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="rawMessage">The IRC message.</param>
    public async Task SendRawAsync(ReadOnlyMemory<char> rawMessage)
    {
        await Send(rawMessage);
    }

    /// <inheritdoc cref="SendMessage(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    public void SendMessage(string channel, string message)
    {
        SendMessage(channel.AsMemory(), message.AsMemory());
    }

    /// <summary>
    /// Sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public void SendMessage(ReadOnlyMemory<char> channel, ReadOnlyMemory<char> message)
    {
        SendMessageAsync(channel, message);
    }

    /// <inheritdoc cref="SendMessageAsync(System.ReadOnlyMemory{char},System.ReadOnlyMemory{char})"/>
    public async Task SendMessageAsync(string channel, string message)
    {
        await SendMessageAsync(channel.AsMemory(), message.AsMemory());
    }

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="builder">The builder that contains the message that will be sent.</param>
    public async Task SendMessageAsync(ReadOnlyMemory<char> channel, MessageBuilder builder)
    {
        await SendMessageAsync(channel, builder.Message);
    }

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public async Task SendMessageAsync(ReadOnlyMemory<char> channel, ReadOnlyMemory<char> message)
    {
        char[] rentedArray = ArrayPool<char>.Shared.Rent(_privMsgPrefix.Length + _maxChannelNameLength + 2 + _maxMessageLength);
        try
        {
            Memory<char> buffer = rentedArray;
            _privMsgPrefix.CopyTo(buffer.Span);
            int bufferLength = _privMsgPrefix.Length;
            channel.Span.CopyTo(buffer.Span[bufferLength..]);
            bufferLength += channel.Length;
            buffer.Span[bufferLength++] = ' ';
            buffer.Span[bufferLength++] = ':';
            message.Span.CopyTo(buffer.Span[bufferLength..]);
            bufferLength += message.Length;
            await Send(buffer[..bufferLength]);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <inheritdoc cref="JoinChannel(ReadOnlyMemory{char})"/>
    public void JoinChannel(string channel)
    {
        JoinChannel(channel.AsMemory());
    }

    /// <summary>
    /// Joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public void JoinChannel(ReadOnlyMemory<char> channel)
    {
        JoinChannelAsync(channel);
    }

    /// <inheritdoc cref="JoinChannelAsync(ReadOnlyMemory{char})"/>
    public async Task JoinChannelAsync(string channel)
    {
        await JoinChannelAsync(channel.AsMemory());
    }

    /// <summary>
    /// Asynchronously joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public async Task JoinChannelAsync(ReadOnlyMemory<char> channel)
    {
        char[] rentedArray = ArrayPool<char>.Shared.Rent(_joinPrefix.Length + _maxChannelNameLength);
        try
        {
            Memory<char> buffer = rentedArray;
            _joinPrefix.CopyTo(buffer.Span);
            int bufferLength = _joinPrefix.Length;
            channel.Span.CopyTo(buffer.Span[bufferLength..]);
            bufferLength += channel.Length;
            await Send(buffer[..bufferLength]);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    /// <inheritdoc cref="LeaveChannel(ReadOnlyMemory{char})"/>
    public void LeaveChannel(string channel)
    {
        LeaveChannel(channel.AsMemory());
    }

    /// <summary>
    /// Leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public void LeaveChannel(ReadOnlyMemory<char> channel)
    {
        LeaveChannelAsync(channel);
    }

    /// <inheritdoc cref="LeaveChannelAsync(ReadOnlyMemory{char})"/>
    public async Task LeaveChannelAsync(string channel)
    {
        await LeaveChannelAsync(channel.AsMemory());
    }

    /// <summary>
    /// Asynchronously leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public async Task LeaveChannelAsync(ReadOnlyMemory<char> channel)
    {
        char[] rentedArray = ArrayPool<char>.Shared.Rent(_partPrefix.Length + _maxChannelNameLength);
        try
        {
            Memory<char> buffer = rentedArray;
            _partPrefix.CopyTo(buffer.Span);
            int bufferLength = _partPrefix.Length;
            channel.Span.CopyTo(buffer.Span[bufferLength..]);
            bufferLength += channel.Length;
            await Send(buffer[..bufferLength]);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    private async ValueTask JoinChannelsThrottled(ReadOnlyMemory<string> channels)
    {
        char[] rentedArray = ArrayPool<char>.Shared.Rent(_joinPrefix.Length + _maxChannelNameLength);
        try
        {
            if (channels.Length == 0)
            {
                return;
            }

            int maxJoinsInPeriod = _isVerifiedBot ? 200 : 20;
            TimeSpan period = TimeSpan.FromSeconds(10);

            Memory<char> buffer = rentedArray;
            DateTimeOffset start = DateTimeOffset.UtcNow;
            for (int i = 0; i < channels.Length; i++)
            {
                if (i > 0 && i % maxJoinsInPeriod == 0)
                {
                    TimeSpan waitTime = period - (DateTimeOffset.UtcNow - start);
                    if (waitTime.TotalMilliseconds > 0)
                    {
                        await Task.Delay(waitTime);
                    }

                    start = DateTimeOffset.UtcNow;
                }

                _joinPrefix.CopyTo(buffer.Span);
                int bufferLength = _joinPrefix.Length;
                channels.Span[i].CopyTo(buffer.Span[bufferLength..]);
                bufferLength += channels.Span[i].Length;
                await Send(buffer[..bufferLength]);
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
    }

    private protected void InvokeDataReceived(IrcClient sender, ReceivedData data)
    {
        if (OnDataReceived is null)
        {
            data.Dispose();
            return;
        }

        OnDataReceived.Invoke(sender, data);
    }

    private protected abstract ValueTask Send(ReadOnlyMemory<char> message);

    private protected abstract void StartListening();

    private protected abstract ValueTask ConnectClient();

    private protected abstract ValueTask DisconnectClient(string closeMessage);

    private protected abstract (string Url, int Port) GetUrl();

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public abstract void Dispose();
}
