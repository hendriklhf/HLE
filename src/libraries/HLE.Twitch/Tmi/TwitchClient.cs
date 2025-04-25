using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Threading;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Represents a Twitch chat client.
/// </summary>
[SuppressMessage("Design", "CA1030:Use events where appropriate")]
public sealed class TwitchClient : IDisposable, IEquatable<TwitchClient>
{
    /// <summary>
    /// The username of the client.
    /// </summary>
    public string Username => _client.Username;

    /// <summary>
    /// Indicates whether the client is connected anonymously or not.
    /// </summary>
    public bool IsAnonymousLogin { get; }

    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public bool IsConnected => _client.State is WebSocketState.Open;

    /// <summary>
    /// The list of channels the client is connected to. Channels can be retrieved by the owner's username or user id in order to read the room state, e.g. if slow-mode is on.
    /// </summary>
    public ChannelList Channels { get; } = new();

    /// <summary>
    /// Is invoked if the client connects.
    /// </summary>W
    public event AsyncEventHandler<TwitchClient>? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event AsyncEventHandler<TwitchClient>? OnDisconnected;

    /// <summary>
    /// Is invoked if a user joins a channel.
    /// </summary>
    public event AsyncEventHandler<TwitchClient, JoinChannelMessage>? OnJoinedChannel
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        add
        {
            if (!_ircHandler.IsOnJoinReceivedSubscribed)
            {
                _ircHandler.OnJoinReceived += IrcHandler_OnJoinReceivedAsync;
            }

            _onJoinedChannel += value;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        remove
        {
            if (_ircHandler.IsOnJoinReceivedSubscribed)
            {
                _ircHandler.OnJoinReceived -= IrcHandler_OnJoinReceivedAsync;
            }

            _onJoinedChannel -= value;
        }
    }

    /// <summary>
    /// Is invoked if a user leaves a channel.
    /// </summary>
    public event AsyncEventHandler<TwitchClient, LeftChannelMessage>? OnLeftChannel
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        add
        {
            if (!_ircHandler.IsOnPartReceivedSubscribed)
            {
                _ircHandler.OnPartReceived += IrcHandler_OnPartReceivedAsync;
            }

            _onLeftChannel += value;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        remove
        {
            if (_ircHandler.IsOnPartReceivedSubscribed)
            {
                _ircHandler.OnPartReceived -= IrcHandler_OnPartReceivedAsync;
            }

            _onLeftChannel -= value;
        }
    }

    /// <summary>
    /// Is invoked if a room state has been received.
    /// </summary>
    public event AsyncEventHandler<TwitchClient, Roomstate>? OnRoomstateReceived;

    /// <summary>
    /// Is invoked if a chat message has been received.
    /// </summary>
    public event AsyncEventHandler<TwitchClient, ChatMessage>? OnChatMessageReceived
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        add
        {
            if (!_ircHandler.IsOnChatMessageReceivedSubscribed)
            {
                _ircHandler.OnChatMessageReceived += IrcHandler_OnChatMessageReceivedAsync;
            }

            _onChatMessageReceived += value;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        remove
        {
            if (_ircHandler.IsOnChatMessageReceivedSubscribed)
            {
                _ircHandler.OnChatMessageReceived -= IrcHandler_OnChatMessageReceivedAsync;
            }

            _onChatMessageReceived -= value;
        }
    }

    /// <summary>
    /// Is invoked if a notice has been received.
    /// </summary>
    public event AsyncEventHandler<TwitchClient, Notice>? OnNoticeReceived
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        add
        {
            if (!_ircHandler.IsOnNoticeReceivedSubscribed)
            {
                _ircHandler.OnNoticeReceived += IrcHandler_OnNoticeReceivedAsync;
            }

            _onNoticeReceived += value;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        remove
        {
            if (_ircHandler.IsOnNoticeReceivedSubscribed)
            {
                _ircHandler.OnNoticeReceived -= IrcHandler_OnNoticeReceivedAsync;
            }

            _onNoticeReceived -= value;
        }
    }

    /// <summary>
    /// Is invoked if data is received from the chat server. If this event is subscribed to, the <see cref="Bytes"/> instance has to be manually disposed.
    /// </summary>
    public event AsyncEventHandler<TwitchClient, Bytes>? OnBytesReceived;

    private event AsyncEventHandler<TwitchClient, JoinChannelMessage>? _onJoinedChannel;

    private event AsyncEventHandler<TwitchClient, LeftChannelMessage>? _onLeftChannel;

    private event AsyncEventHandler<TwitchClient, ChatMessage>? _onChatMessageReceived;

    private event AsyncEventHandler<TwitchClient, Notice>? _onNoticeReceived;

    internal readonly WebSocketIrcClient _client;
    internal readonly IrcHandler _ircHandler;
    private readonly IrcChannelList _ircChannels = new();
    private readonly SemaphoreSlim _reconnectionLock = new(1);

    private static ReadOnlySpan<byte> PongPrefix => "PONG :"u8;

    private const string AnonymousUsername = "justinfan123";

    /// <summary>
    /// The constructor for an anonymous chat client. An anonymous chat client can only receive messages, but cannot send any messages.
    /// Connects with the username "justinfan123".
    /// </summary>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    public TwitchClient(ClientOptions options)
    {
        _client = new(AnonymousUsername, OAuthToken.Empty, options);
        _ircHandler = new();
        IsAnonymousLogin = true;
        SubscribeToEvents();
    }

    /// <summary>
    /// The constructor for a normal chat client.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if <paramref name="username"/> or <paramref name="oAuthToken"/> are in a wrong format.</exception>
    public TwitchClient(string username, OAuthToken oAuthToken, ClientOptions options)
    {
        username = ChannelFormatter.Format(username, false);
        _client = new(username, oAuthToken, options);
        _ircHandler = new();
        IsAnonymousLogin = false;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _client.OnConnected += (_, ct) => EventInvoker.InvokeAsync(OnConnected, this, ct);
        _client.OnDisconnected += (_, ct) => EventInvoker.InvokeAsync(OnDisconnected, this, ct);
        _client.OnBytesReceived += (_, data, ct) => WebSocketIrcClient_OnBytesReceivedAsync(data, ct);
        _client.OnConnectionException += (_, _, ct) => ReconnectAfterConnectionExceptionAsync(ct);

        _ircHandler.OnRoomstateReceived += IrcHandler_OnRoomstateReceivedAsync;
        _ircHandler.OnPingReceived += IrcHandler_OnPingReceivedAsync;
        _ircHandler.OnReconnectReceived += IrcHandler_OnReconnectReceivedAsync;
    }

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    public ValueTask SendAsync(string channel, string message) => SendAsync(channel.AsMemory(), message.AsMemory());

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    public ValueTask SendAsync(string channel, ReadOnlyMemory<char> message) => SendAsync(channel.AsMemory(), message);

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    public ValueTask SendAsync(ReadOnlyMemory<char> channel, string message) => SendAsync(channel, message.AsMemory());

    /// <summary>
    /// Asynchronously sends a chat message.
    /// </summary>
    /// <param name="channel">The username of the channel owner.</param>
    /// <param name="message">The message that will be sent.</param>
    public async ValueTask SendAsync(ReadOnlyMemory<char> channel, ReadOnlyMemory<char> message)
    {
        if (!IsConnected)
        {
            ThrowClientNotConnectedException();
        }

        if (IsAnonymousLogin)
        {
            ThrowAnonymousClientException();
        }

        if (!Channels.TryGet(channel.Span, out Channel? channelObject))
        {
            ThrowNotConnectedToChannelException(channel);
        }

        ImmutableArray<byte> prefixedChannel = channelObject.PrefixedNameUtf8;
        using Bytes messageUtf8 = Utf16ToUtf8(message.Span);
        await _client.SendMessageAsync(prefixedChannel.AsMemory(), messageUtf8.AsMemory());
    }

    /// <inheritdoc cref="SendAsync(long,ReadOnlyMemory{char})"/>
    public ValueTask SendAsync(long channelId, string message) => SendAsync(channelId, message.AsMemory());

    /// <summary>
    /// Asynchronously sends a chat message.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    public async ValueTask SendAsync(long channelId, ReadOnlyMemory<char> message)
    {
        if (!IsConnected)
        {
            ThrowClientNotConnectedException();
        }

        if (IsAnonymousLogin)
        {
            ThrowAnonymousClientException();
        }

        if (!Channels.TryGet(channelId, out Channel? channelObject))
        {
            ThrowNotConnectedToChannelException(channelId);
        }

        ImmutableArray<byte> prefixedChannel = channelObject.PrefixedNameUtf8;
        using Bytes messageUtf8 = Utf16ToUtf8(message.Span);
        await _client.SendMessageAsync(prefixedChannel.AsMemory(), messageUtf8.AsMemory());
    }

    [DoesNotReturn]
    private static void ThrowNotConnectedToChannelException(ReadOnlyMemory<char> channel)
        => throw new NotConnectedToChannelException(new string(channel.Span));

    [DoesNotReturn]
    private static void ThrowNotConnectedToChannelException(long channelId)
        => throw new NotConnectedToChannelException(channelId);

    [DoesNotReturn]
    private static void ThrowAnonymousClientException() => throw new AnonymousClientException();

    [DoesNotReturn]
    private static void ThrowClientNotConnectedException() => throw new ClientNotConnectedException();

    /// <inheritdoc cref="SendRawAsync(ReadOnlyMemory{char})"/>
    public ValueTask SendRawAsync(string rawMessage) => SendRawAsync(rawMessage.AsMemory());

    /// <summary>
    /// Asynchronously sends a raw message to the chat server.
    /// </summary>
    /// <param name="rawMessage">The raw message</param>
    public async ValueTask SendRawAsync(ReadOnlyMemory<char> rawMessage)
    {
        if (!IsConnected)
        {
            ThrowClientNotConnectedException();
        }

        using Bytes rawMessageUtf8 = Utf16ToUtf8(rawMessage.Span);
        await _client.SendRawAsync(rawMessageUtf8.AsMemory());
    }

    /// <summary>
    /// Asynchronously connects the client to the chat server. This method will be exited after the client has joined all channels.
    /// </summary>
    public Task ConnectAsync() => IsConnected ? Task.CompletedTask : ConnectAsync(_ircChannels.GetUtf8Names().AsMemory());

    private Task ConnectAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> ircChannels) => _client.ConnectAsync(ircChannels);

    /// <inheritdoc cref="JoinChannelsAsync(ReadOnlyMemory{string})"/>
    public async ValueTask JoinChannelsAsync(IEnumerable<string> channels)
    {
        if (channels.TryGetReadOnlyMemory(out ReadOnlyMemory<string> channelsMemory))
        {
            await JoinChannelsAsync(channelsMemory);
            return;
        }

        foreach (string channel in channels)
        {
            await JoinChannelAsync(channel);
        }
    }

    /// <inheritdoc cref="JoinChannelsAsync(ReadOnlyMemory{string})"/>
    public ValueTask JoinChannelsAsync(List<string> channels) => JoinChannelsAsync(ListMarshal.AsReadOnlyMemory(channels));

    /// <inheritdoc cref="JoinChannelsAsync(ReadOnlyMemory{string})"/>
    public ValueTask JoinChannelsAsync(params string[] channels) => JoinChannelsAsync(new ReadOnlyMemory<string>(channels, 0, channels.Length));

    /// <summary>
    /// If the client is not connected, adds the channels to the channel list, otherwise asynchronously connects the client to the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if any of <paramref name="channels"/> is in the wrong format.</exception>
    public async ValueTask JoinChannelsAsync(ReadOnlyMemory<string> channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            await JoinChannelAsync(channels.Span[i]);
        }
    }

    /// <inheritdoc cref="JoinChannelAsync(ReadOnlyMemory{char})"/>
    public ValueTask JoinChannelAsync(string channel) => JoinChannelAsync(channel.AsMemory());

    /// <summary>
    /// If the client is not connected, adds the channel to the channel list, otherwise asynchronously connects the client to the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if the <paramref name="channel"/> is in the wrong format.</exception>
    public ValueTask JoinChannelAsync(ReadOnlyMemory<char> channel)
    {
        IrcChannel ircChannel = _ircChannels.Add(channel.Span);
        return !IsConnected ? ValueTask.CompletedTask : _client.JoinChannelAsync(ircChannel.NameUtf8.AsMemory());
    }

    /// <inheritdoc cref="LeaveChannelAsync(ReadOnlyMemory{char})"/>
    public ValueTask LeaveChannelAsync(string channel) => LeaveChannelAsync(channel.AsMemory());

    /// <summary>
    /// If the client is not connected, removes the channel from the channel list, otherwise asynchronously leaves the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    public ValueTask LeaveChannelAsync(ReadOnlyMemory<char> channel)
    {
        IrcChannel? ircChannel = _ircChannels.Remove(channel.Span);
        if (ircChannel is null)
        {
            return ValueTask.CompletedTask;
        }

        Channels.Remove(ircChannel.Name);
        return !IsConnected ? ValueTask.CompletedTask : _client.LeaveChannelAsync(ircChannel.NameUtf8.AsMemory());
    }

    /// <inheritdoc cref="LeaveChannelsAsync(ReadOnlyMemory{string})"/>
    public async ValueTask LeaveChannelsAsync(IEnumerable<string> channels)
    {
        if (channels.TryGetReadOnlyMemory(out ReadOnlyMemory<string> channelsMemory))
        {
            await LeaveChannelsAsync(channelsMemory);
            return;
        }

        foreach (string channel in channels)
        {
            await LeaveChannelAsync(channel);
        }
    }

    /// <inheritdoc cref="LeaveChannelsAsync(ReadOnlyMemory{string})"/>
    public ValueTask LeaveChannelsAsync(List<string> channels) => LeaveChannelsAsync(ListMarshal.AsReadOnlyMemory(channels));

    /// <inheritdoc cref="LeaveChannelsAsync(ReadOnlyMemory{string})"/>
    public ValueTask LeaveChannelsAsync(params string[] channels) => LeaveChannelsAsync(new ReadOnlyMemory<string>(channels, 0, channels.Length));

    /// <summary>
    /// If the client is not connected, removes the channels from the channel list, otherwise leaves the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    public async ValueTask LeaveChannelsAsync(ReadOnlyMemory<string> channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            await LeaveChannelAsync(channels.Span[i]);
        }
    }

    /// <summary>
    /// If the client is not connected, clears the channel list, otherwise also asynchronously leaves all channels.
    /// </summary>
    public async ValueTask LeaveChannelsAsync()
    {
        if (IsConnected)
        {
            ReadOnlyMemory<ReadOnlyMemory<byte>> utf8Channels = _ircChannels.GetUtf8Names().AsMemory();
            for (int i = 0; i < utf8Channels.Length; i++)
            {
                await _client.LeaveChannelAsync(utf8Channels.Span[i]);
            }
        }

        Channels.Clear();
        _ircChannels.Clear();
    }

    /// <summary>
    /// Asynchronously disconnects the client from the chat server.
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        await _client.DisconnectAsync();
        Channels.Clear();
    }

    private Task WebSocketIrcClient_OnBytesReceivedAsync(Bytes data, CancellationToken cancellationToken)
    {
        _ircHandler.Handle(data.AsSpan());
        if (OnBytesReceived is null)
        {
            data.Dispose();
            return Task.CompletedTask;
        }

        return EventInvoker.InvokeAsync(OnBytesReceived, this, data, cancellationToken);
    }

    private Task IrcHandler_OnChatMessageReceivedAsync(IrcHandler _, ChatMessage message, CancellationToken cancellationToken)
        => EventInvoker.InvokeAsync(_onChatMessageReceived, this, message, cancellationToken);

    private Task IrcHandler_OnRoomstateReceivedAsync(object? sender, Roomstate roomstate, CancellationToken cancellationToken)
    {
        Channels.Update(ref roomstate);
        return EventInvoker.InvokeAsync(OnRoomstateReceived, this, roomstate, cancellationToken);
    }

    private Task IrcHandler_OnNoticeReceivedAsync(IrcHandler _, Notice notice, CancellationToken cancellationToken)
        => EventInvoker.InvokeAsync(_onNoticeReceived, this, notice, cancellationToken);

    private Task IrcHandler_OnReconnectReceivedAsync(IrcHandler _, CancellationToken cancellationToken)
        => _client.ReconnectAsync(_ircChannels.GetUtf8Names().AsMemory());

    private async Task IrcHandler_OnPingReceivedAsync(IrcHandler _, Bytes bytes, CancellationToken cancellationToken)
    {
        try
        {
            using PooledBufferWriter<byte> builder = new(PongPrefix.Length + bytes.Length);
            builder.Write(PongPrefix);
            builder.Write(bytes.AsSpan());

            await _client.SendRawAsync(builder.WrittenMemory);
        }
        finally
        {
            bytes.Dispose();
        }
    }

    private Task IrcHandler_OnJoinReceivedAsync(IrcHandler _, JoinChannelMessage message, CancellationToken cancellationToken)
        => EventInvoker.InvokeAsync(_onJoinedChannel, this, message, cancellationToken);

    private Task IrcHandler_OnPartReceivedAsync(IrcHandler _, LeftChannelMessage message, CancellationToken cancellationToken)
        => EventInvoker.InvokeAsync(_onLeftChannel, this, message, cancellationToken);

    private async Task ReconnectAfterConnectionExceptionAsync(CancellationToken cancellationToken)
    {
        await _reconnectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_client.State is WebSocketState.Open or WebSocketState.Connecting)
            {
                return;
            }

            _client.CancelTasks();
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await _client.ConnectAsync(_ircChannels.GetUtf8Names().AsMemory());
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
        finally
        {
            _reconnectionLock.Release();
        }
    }

    [Pure]
    private static Bytes Utf16ToUtf8(ReadOnlySpan<char> chars)
    {
        Encoding utf8 = Encoding.UTF8;
        int maxByteCount = utf8.GetMaxByteCount(chars.Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxByteCount);
        int byteCount = utf8.GetBytes(chars, buffer);
        return Bytes.AsBytes(buffer, byteCount);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        _client.Dispose();
        _reconnectionLock.Dispose();
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] TwitchClient? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(TwitchClient? left, TwitchClient? right) => Equals(left, right);

    public static bool operator !=(TwitchClient? left, TwitchClient? right) => !(left == right);
}
