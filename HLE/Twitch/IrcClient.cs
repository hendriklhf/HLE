using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// The base class for IRC clients.
/// This class does not validate input, the overlying <see cref="TwitchClient"/> does that.
/// </summary>
public abstract class IrcClient : IDisposable, IEquatable<IrcClient>
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

    internal event EventHandler? OnConnectionException;

    private protected CancellationTokenSource _cancellationTokenSource = new();

    private protected readonly string? _oAuthToken;
    private protected readonly (string Url, int Port) _url;

    // ReSharper disable once InconsistentNaming
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

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    public async ValueTask ConnectAsync(string[] channels)
    {
        await ConnectAsync(channels.AsMemory());
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    public async ValueTask ConnectAsync(List<string> channels)
    {
        await ConnectAsync(CollectionsMarshal.AsSpan(channels).AsMemoryDangerous());
    }

    /// <summary>
    /// Asynchronously connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public async ValueTask ConnectAsync(ReadOnlyMemory<string> channels)
    {
        await ConnectClientAsync();
        StartListening();
        OnConnected?.Invoke(this, EventArgs.Empty);

        using PoolBufferStringBuilder messageBuilder = new(_capReqMessage.Length);
        if (_oAuthToken is not null)
        {
            messageBuilder.Append(_passPrefix, _oAuthToken);
            await SendAsync(messageBuilder.WrittenMemory);
        }

        messageBuilder.Clear();
        messageBuilder.Append(_nickPrefix, Username);
        await SendAsync(messageBuilder.WrittenMemory);

        messageBuilder.Clear();
        messageBuilder.Append(_capReqMessage);
        await SendAsync(messageBuilder.WrittenMemory);

        await JoinChannelsThrottledAsync(channels);
    }

    /// <summary>
    /// Asynchronously disconnects the client.
    /// </summary>
    /// <param name="closeMessage">A close message or reason.</param>
    public async ValueTask DisconnectAsync(string closeMessage = "Manually closed")
    {
        await DisconnectClientAsync(closeMessage);
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }

    internal async ValueTask ReconnectAsync(ReadOnlyMemory<string> channels)
    {
        await DisconnectAsync();
        await ConnectAsync(channels);
    }

    /// <inheritdoc cref="SendRawAsync(ReadOnlyMemory{char})"/>
    public async ValueTask SendRawAsync(string rawMessage)
    {
        await SendAsync(rawMessage.AsMemory());
    }

    /// <summary>
    /// Asynchronously sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="rawMessage">The IRC message.</param>
    public async ValueTask SendRawAsync(ReadOnlyMemory<char> rawMessage)
    {
        await SendAsync(rawMessage);
    }

    /// <inheritdoc cref="SendMessageAsync(System.ReadOnlyMemory{char},System.ReadOnlyMemory{char})"/>
    public async ValueTask SendMessageAsync(string channel, string message)
    {
        await SendMessageAsync(channel.AsMemory(), message.AsMemory());
    }

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="builder">The builder that contains the message that will be sent.</param>
    public async ValueTask SendMessageAsync(ReadOnlyMemory<char> channel, PoolBufferStringBuilder builder)
    {
        await SendMessageAsync(channel, builder.WrittenMemory);
    }

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public async ValueTask SendMessageAsync(ReadOnlyMemory<char> channel, ReadOnlyMemory<char> message)
    {
        using PoolBufferStringBuilder messageBuilder = new(_privMsgPrefix.Length + _maxChannelNameLength + 2 + _maxMessageLength);
        messageBuilder.Append(_privMsgPrefix, channel.Span, " :", message.Span);
        await SendAsync(messageBuilder.WrittenMemory);
    }

    /// <inheritdoc cref="JoinChannelAsync(ReadOnlyMemory{char})"/>
    public async ValueTask JoinChannelAsync(string channel)
    {
        await JoinChannelAsync(channel.AsMemory());
    }

    /// <summary>
    /// Asynchronously joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public async ValueTask JoinChannelAsync(ReadOnlyMemory<char> channel)
    {
        using PoolBufferStringBuilder messageBuilder = new(_joinPrefix.Length + _maxChannelNameLength);
        messageBuilder.Append(_joinPrefix, channel.Span);
        await SendAsync(messageBuilder.WrittenMemory);
    }

    /// <inheritdoc cref="LeaveChannelAsync(ReadOnlyMemory{char})"/>
    public async ValueTask LeaveChannelAsync(string channel)
    {
        await LeaveChannelAsync(channel.AsMemory());
    }

    /// <summary>
    /// Asynchronously leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public async ValueTask LeaveChannelAsync(ReadOnlyMemory<char> channel)
    {
        using PoolBufferStringBuilder messageBuilder = new(_partPrefix.Length + _maxChannelNameLength);
        messageBuilder.Append(_partPrefix, channel.Span);
        await SendAsync(messageBuilder.WrittenMemory);
    }

    private async ValueTask JoinChannelsThrottledAsync(ReadOnlyMemory<string> channels)
    {
        if (channels.Length == 0)
        {
            return;
        }

        bool isVerifiedBot = _isVerifiedBot;
        int maxJoinsInPeriod = 20 + 180 * Unsafe.As<bool, byte>(ref isVerifiedBot);
        TimeSpan period = TimeSpan.FromSeconds(10);

        using PoolBufferStringBuilder messageBuilder = new(_joinPrefix.Length + _maxChannelNameLength);
        DateTimeOffset start = DateTimeOffset.UtcNow;
        for (int i = 0; i < channels.Length && !_cancellationTokenSource.IsCancellationRequested; i++)
        {
            if (i > 0 && i % maxJoinsInPeriod == 0)
            {
                TimeSpan waitTime = period - (DateTimeOffset.UtcNow - start);
                if (waitTime.TotalMilliseconds > 0)
                {
                    await Task.Delay(waitTime, _cancellationTokenSource.Token);
                }

                start = DateTimeOffset.UtcNow;
            }

            messageBuilder.Append(_joinPrefix, channels.Span[i]);
            await SendAsync(messageBuilder.WrittenMemory);
            messageBuilder.Clear();
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

    private protected virtual void InvokeOnConnectionException()
    {
        OnConnectionException?.Invoke(this, EventArgs.Empty);
    }

    internal void CancelTasks()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new();
    }

    private protected abstract ValueTask SendAsync(ReadOnlyMemory<char> message);

    private protected abstract void StartListening();

    private protected abstract ValueTask ConnectClientAsync();

    private protected abstract ValueTask DisconnectClientAsync(string closeMessage);

    private protected abstract (string Url, int Port) GetUrl();

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _cancellationTokenSource.Dispose();
    }

    public bool Equals(IrcClient? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is IrcClient other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Username, _oAuthToken, _url);
    }
}
