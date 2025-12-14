using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Threading;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="ClientWebSocket"/>.
/// </summary>
public sealed class WebSocketIrcClient : IEquatable<WebSocketIrcClient>, IAsyncDisposable
{
    /// <summary>
    /// The username of the client.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Is invoked if the client connects to the server.
    /// </summary>
    public event AsyncEventHandler<WebSocketIrcClient>? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event AsyncEventHandler<WebSocketIrcClient>? OnDisconnected;

    /// <summary>
    /// Gets the state of the websocket connection.
    /// </summary>
    public WebSocketState State => _websocket.State;

    private static ReadOnlySpan<byte> PassPrefix => "PASS "u8;

    private static ReadOnlySpan<byte> NickPrefix => "NICK "u8;

    private static ReadOnlySpan<byte> CapReqMessage => "CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership"u8;

    private static ReadOnlySpan<byte> PrivMsgPrefix => "PRIVMSG "u8;

    private static ReadOnlySpan<byte> JoinPrefix => "JOIN "u8;

    private static ReadOnlySpan<byte> PartPrefix => "PART "u8;

    private readonly BackgroundWebSocketClient _websocket;
    private readonly bool _isVerifiedBot;
    private readonly OAuthToken _oAuthToken;
    private readonly ImmutableArray<byte> _usernameUtf8;

    private static readonly Uri s_sslConnectionUri = new("wss://irc-ws.chat.twitch.tv:443");
    private static readonly Uri s_nonSslConnectionUri = new("ws://irc-ws.chat.twitch.tv:80");

    private const int MaximumChannelNameLength = 26; // 25 for the name + 1 for the '#'
    private const int MaximumMessageLength = 500;

    /// <summary>
    /// The default constructor of <see cref="WebSocketIrcClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    [SuppressMessage("Style", "IDE0290:Use primary constructor")]
    public WebSocketIrcClient(string username, OAuthToken oAuthToken, ClientOptions options)
    {
        Username = username;

        _websocket = new(options.UseSsl ? s_sslConnectionUri : s_nonSslConnectionUri);
        _usernameUtf8 = ImmutableCollectionsMarshal.AsImmutableArray(Encoding.UTF8.GetBytes(username));
        _oAuthToken = oAuthToken;
        _isVerifiedBot = options.IsVerifiedBot;
    }

    public ValueTask<Bytes> ReceiveAsync(CancellationToken stoppingToken) => _websocket.ReceiveAsync(stoppingToken);

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{ReadOnlyMemory{byte}})"/>
    public async Task ConnectAsync(IEnumerable<ReadOnlyMemory<byte>> channels)
    {
        if (channels.TryGetReadOnlyMemory(out ReadOnlyMemory<ReadOnlyMemory<byte>> channelsAsMemory))
        {
            await ConnectAsync(channelsAsMemory).ConfigureAwait(false);
            return;
        }

        await ConnectAsync(channels.ToArray()).ConfigureAwait(false);
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{ReadOnlyMemory{byte}})"/>
    public Task ConnectAsync(ReadOnlyMemory<byte>[] channels) => ConnectAsync(channels.AsMemory());

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{ReadOnlyMemory{byte}})"/>
    public Task ConnectAsync(List<ReadOnlyMemory<byte>> channels) => ConnectAsync(ListMarshal.AsReadOnlyMemory(channels));

    /// <summary>
    /// Asynchronously connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public async Task ConnectAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> channels)
    {
        await _websocket.ConnectAsync().ConfigureAwait(false);
        EventInvoker.InvokeAsync(OnConnected, this).Ignore();
        await AuthenticateAndJoinChannelsAsync(channels).ConfigureAwait(false);
    }

    internal async Task AuthenticateAndJoinChannelsAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> channels)
    {
        using PooledBufferWriter<byte> messageBuilder = new(CapReqMessage.Length);
        if (_oAuthToken != OAuthToken.Empty)
        {
            messageBuilder.Write(PassPrefix);
            messageBuilder.WriteUtf8(_oAuthToken.AsSpan());
            await _websocket.SendEvenWhenInMaintenanceAsync(messageBuilder.WrittenMemory).ConfigureAwait(false);
        }

        messageBuilder.Clear();
        messageBuilder.Write(NickPrefix);
        messageBuilder.Write(_usernameUtf8.AsSpan());
        await _websocket.SendEvenWhenInMaintenanceAsync(messageBuilder.WrittenMemory).ConfigureAwait(false);

        messageBuilder.Clear();
        messageBuilder.Write(CapReqMessage);
        await _websocket.SendEvenWhenInMaintenanceAsync(messageBuilder.WrittenMemory).ConfigureAwait(false);

        await JoinChannelsThrottledAsync(channels).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously disconnects the client.
    /// </summary>
    public async Task DisconnectAsync()
    {
        await _websocket.DisconnectAsync().ConfigureAwait(false);
        EventInvoker.InvokeAsync(OnDisconnected, this).Ignore();
    }

    internal async Task ReconnectAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> channels)
    {
        await _websocket.ReconnectAsync().ConfigureAwait(false);
        await AuthenticateAndJoinChannelsAsync(channels).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="rawMessage">The IRC message.</param>
    public ValueTask SendRawAsync(ReadOnlyMemory<byte> rawMessage)
    {
        Bytes bytes = new(rawMessage.Span);
        return _websocket.SendAsync(bytes);
    }

    public ValueTask SendRawAsync(Bytes rawMessage) => _websocket.SendAsync(rawMessage);

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public ValueTask SendMessageAsync(ReadOnlyMemory<byte> channel, ReadOnlyMemory<byte> message)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(PrivMsgPrefix.Length + MaximumChannelNameLength + " :"u8.Length + MaximumMessageLength);

        UnsafeBufferWriter<byte> messageBuilder = new(buffer);
        messageBuilder.Write(PrivMsgPrefix);
        messageBuilder.Write(channel.Span);
        messageBuilder.Write(" :"u8);
        messageBuilder.Write(message.Span);

        Bytes bytes = Bytes.AsBytes(buffer, messageBuilder.Count);
        return _websocket.SendAsync(bytes);
    }

    /// <summary>
    /// Asynchronously joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public ValueTask JoinChannelAsync(ReadOnlyMemory<byte> channel)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(JoinPrefix.Length + MaximumChannelNameLength);
        UnsafeBufferWriter<byte> messageBuilder = new(buffer);
        messageBuilder.Write(JoinPrefix);
        messageBuilder.Write(channel.Span);

        Bytes bytes = Bytes.AsBytes(buffer, messageBuilder.Count);
        return _websocket.SendAsync(bytes);
    }

    /// <summary>
    /// Asynchronously leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public ValueTask LeaveChannelAsync(ReadOnlyMemory<byte> channel)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(PartPrefix.Length + MaximumChannelNameLength);
        UnsafeBufferWriter<byte> messageBuilder = new(buffer);
        messageBuilder.Write(PartPrefix);
        messageBuilder.Write(channel.Span);

        Bytes bytes = Bytes.AsBytes(buffer, messageBuilder.Count);
        return _websocket.SendAsync(bytes);
    }

    private async Task JoinChannelsThrottledAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> channels)
    {
        if (channels.Length == 0)
        {
            return;
        }

        int maximumJoinsInPeriod = _isVerifiedBot ? 200 : 20;
#if NET9_0_OR_GREATER
        long period = double.ConvertToIntegerNative<long>(TimeSpan.FromSeconds(10).TotalMilliseconds);
#else
        long period = (long)TimeSpan.FromSeconds(10).TotalMilliseconds;
#endif

        using PooledBufferWriter<byte> messageBuilder = new(JoinPrefix.Length + MaximumChannelNameLength);

        long start = Environment.TickCount64;
        for (int i = 0; i < channels.Length; i++)
        {
            if (i != 0 && i % maximumJoinsInPeriod == 0)
            {
                long now = Environment.TickCount64;
                long waitTime = period - (now - start);
                if (waitTime > 0)
                {
                    await Task.Delay((int)waitTime).ConfigureAwait(false);
                    await _websocket.WaitForPendingOutgoingMessagesAsync().ConfigureAwait(false);
                }

                start = now + waitTime;
            }

            messageBuilder.Write(JoinPrefix);
            messageBuilder.Write(channels.Span[i].Span);
            await _websocket.SendEvenWhenInMaintenanceAsync(messageBuilder.WrittenMemory).ConfigureAwait(false);
            messageBuilder.Clear();
        }
    }

    internal void RegisterAfterAutomaticReconnectionEvent<T>(Func<object, Task> action, T state) where T : class
        => _websocket.RegisterAfterAutomaticReconnectionEvent(action, state);

    public ValueTask DisposeAsync() => _websocket.DisposeAsync();

    [Pure]
    public bool Equals([NotNullWhen(true)] WebSocketIrcClient? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(WebSocketIrcClient? left, WebSocketIrcClient? right) => Equals(left, right);

    public static bool operator !=(WebSocketIrcClient? left, WebSocketIrcClient? right) => !(left == right);
}
