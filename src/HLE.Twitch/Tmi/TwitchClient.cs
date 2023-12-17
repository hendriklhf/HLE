using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Strings;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Represents a Twitch chat client.
/// </summary>
public sealed partial class TwitchClient : IDisposable, IEquatable<TwitchClient>
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
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL => _client.UseSSL;

    /// <summary>
    /// The list of channels the client is connected to. Channels can be retrieved by the owner's username or user id in order to read the room state, e.g. if slow-mode is on.
    /// </summary>
    public ChannelList Channels { get; } = new();

    /// <summary>
    /// Is invoked if the client connects.
    /// </summary>W
    public event EventHandler? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event EventHandler? OnDisconnected;

    /// <summary>
    /// Is invoked if a user joins a channel.
    /// </summary>
    public event EventHandler<JoinChannelMessage>? OnJoinedChannel;

    /// <summary>
    /// Is invoked if a user leaves a channel.
    /// </summary>
    public event EventHandler<LeftChannelMessage>? OnLeftChannel;

    /// <summary>
    /// Is invoked if a room state has been received.
    /// </summary>
    public event EventHandler<Roomstate>? OnRoomstateReceived;

    /// <summary>
    /// Is invoked if a chat message has been received.
    /// </summary>
    public event EventHandler<IChatMessage>? OnChatMessageReceived;

    /// <summary>
    /// Is invoked if a notice has been received.
    /// </summary>
    public event EventHandler<Notice>? OnNoticeReceived;

    /// <summary>
    /// Is invoked if data is received from the chat server. If this event is subscribed to, the <see cref="ReceivedData"/> instance has to be manually disposed.
    /// Read more in the documentation of the <see cref="ReceivedData"/> class.
    /// </summary>
    public event EventHandler<ReceivedData>? OnDataReceived;

    internal readonly WebSocketIrcClient _client;
    internal readonly IrcHandler _ircHandler;
    internal readonly PooledList<string> _ircChannels = [];
    private readonly SemaphoreSlim _reconnectionLock = new(1);

    private const string AnonymousUsername = "justinfan123";
    private const char ChannelPrefix = '#';
    private const string PongPrefix = "PONG :";

    /// <summary>
    /// The constructor for an anonymous chat client. An anonymous chat client can only receive messages, but cannot send any messages.
    /// Connects with the username "justinfan123".
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    /// </summary>
    public TwitchClient(ClientOptions options)
    {
        _client = new(AnonymousUsername, OAuthToken.Empty, options);
        _ircHandler = new(options.ParsingMode);
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
        username = FormatChannel(username, false);
        _client = new(username, oAuthToken, options);
        _ircHandler = new(options.ParsingMode);
        IsAnonymousLogin = false;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _client.OnConnected += (_, e) => OnConnected?.Invoke(this, e);
        _client.OnDisconnected += (_, e) => OnDisconnected?.Invoke(this, e);
        _client.OnDataReceived += IrcClient_OnDataReceived;
        _client.OnConnectionException += async (_, _) => await ReconnectAfterConnectionExceptionAsync();

        _ircHandler.OnJoinReceived += (_, e) => OnJoinedChannel?.Invoke(this, e);
        _ircHandler.OnPartReceived += (_, e) => OnLeftChannel?.Invoke(this, e);
        _ircHandler.OnRoomstateReceived += IrcHandlerOnRoomstateReceived;
        _ircHandler.OnChatMessageReceived += IrcHandlerOnChatMessageReceived;
        _ircHandler.OnPingReceived += async (_, e) => await IrcHandler_OnPingReceivedAsync(e);
        _ircHandler.OnReconnectReceived += async (_, _) => await _client.ReconnectAsync(SpanMarshal.AsMemory(_ircChannels.AsSpan()));
        _ircHandler.OnNoticeReceived += (_, e) => OnNoticeReceived?.Invoke(this, e);
    }

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendAsync(string channel, string message) => SendAsync(channel.AsMemory(), message.AsMemory());

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendAsync(string channel, ReadOnlyMemory<char> message) => SendAsync(channel.AsMemory(), message);

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendAsync(ReadOnlyMemory<char> channel, string message) => SendAsync(channel, message.AsMemory());

    /// <summary>
    /// Asynchronously sends a chat message.
    /// </summary>
    /// <param name="channel">The username of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendAsync(ReadOnlyMemory<char> channel, ReadOnlyMemory<char> message)
    {
        if (!IsConnected)
        {
            ThrowClientNotConnectedException();
        }

        if (IsAnonymousLogin)
        {
            ThrowAnonymousClientException();
        }

        Channels.TryGet(channel.Span, out Channel? channelObject);
        if (channelObject is null)
        {
            ThrowNotConnectedToChannelException(channel);
        }

        string prefixedChannel = channelObject._prefixedName;
        return _client.SendMessageAsync(prefixedChannel.AsMemory(), message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNotConnectedToChannelException(ReadOnlyMemory<char> channel)
        => throw new NotConnectedToChannelException(new string(channel.Span));

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowAnonymousClientException() => throw new AnonymousClientException();

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowClientNotConnectedException() => throw new ClientNotConnectedException();

    /// <inheritdoc cref="SendAsync(long,ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendAsync(long channelId, string message) => SendAsync(channelId, message.AsMemory());

    /// <summary>
    /// Asynchronously sends a chat message.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendAsync(long channelId, ReadOnlyMemory<char> message)
    {
        if (!IsConnected)
        {
            ThrowClientNotConnectedException();
        }

        if (IsAnonymousLogin)
        {
            ThrowAnonymousClientException();
        }

        Channels.TryGet(channelId, out Channel? channelObject);
        string prefixedChannel = channelObject?._prefixedName ?? throw new NotConnectedToChannelException(channelId);
        return _client.SendMessageAsync(prefixedChannel.AsMemory(), message);
    }

    /// <inheritdoc cref="SendRawAsync(ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendRawAsync(string rawMessage) => SendRawAsync(rawMessage.AsMemory());

    /// <summary>
    /// Asynchronously sends a raw message to the chat server.
    /// </summary>
    /// <param name="rawMessage">The raw message</param>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendRawAsync(ReadOnlyMemory<char> rawMessage)
    {
        if (!IsConnected)
        {
            ThrowClientNotConnectedException();
        }

        return _client.SendRawAsync(rawMessage);
    }

    /// <summary>
    /// Asynchronously connects the client to the chat server. This method will be exited after the client has joined all channels.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public Task ConnectAsync() => IsConnected ? Task.CompletedTask : ConnectAsync(SpanMarshal.AsMemory(_ircChannels.AsSpan()));

    // ReSharper disable once InconsistentNaming
    private Task ConnectAsync(ReadOnlyMemory<string> ircChannels) => _client.ConnectAsync(ircChannels);

    /// <inheritdoc cref="JoinChannelsAsync(ReadOnlyMemory{string})"/>
    public async ValueTask JoinChannelsAsync(IEnumerable<string> channels)
    {
        if (channels.TryGetReadOnlyMemory<string>(out ReadOnlyMemory<string> channelsMemory))
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
    // ReSharper disable once InconsistentNaming
    public ValueTask JoinChannelsAsync(List<string> channels)
        => JoinChannelsAsync(SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(channels)));

    /// <inheritdoc cref="JoinChannelsAsync(ReadOnlyMemory{string})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask JoinChannelsAsync(params string[] channels) => JoinChannelsAsync(channels.AsMemory());

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
    // ReSharper disable once InconsistentNaming
    public ValueTask JoinChannelAsync(string channel) => JoinChannelAsync(channel.AsMemory());

    /// <summary>
    /// If the client is not connected, adds the channel to the channel list, otherwise asynchronously connects the client to the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if the <paramref name="channel"/> is in the wrong format.</exception>
    // ReSharper disable once InconsistentNaming
    public ValueTask JoinChannelAsync(ReadOnlyMemory<char> channel)
    {
        string channelString = FormatChannel(channel.Span);
        _ircChannels.Add(channelString);
        return IsConnected ? _client.JoinChannelAsync(channelString.AsMemory()) : ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="LeaveChannelAsync(ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask LeaveChannelAsync(string channel) => LeaveChannelAsync(channel.AsMemory());

    /// <summary>
    /// If the client is not connected, removes the channel from the channel list, otherwise asynchronously leaves the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    // ReSharper disable once InconsistentNaming
    public ValueTask LeaveChannelAsync(ReadOnlyMemory<char> channel)
    {
        if (_ircChannels.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        string channelString = FormatChannel(channel.Span);
        _ircChannels.Remove(channelString);
        Channels.Remove(channel.Span);
        return IsConnected ? _client.LeaveChannelAsync(channel) : ValueTask.CompletedTask;
    }

    /// <inheritdoc cref="LeaveChannelsAsync(ReadOnlyMemory{string})"/>
    public async ValueTask LeaveChannelsAsync(IEnumerable<string> channels)
    {
        if (channels.TryGetReadOnlyMemory<string>(out ReadOnlyMemory<string> channelsMemory))
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
    // ReSharper disable once InconsistentNaming
    public ValueTask LeaveChannelsAsync(List<string> channels)
        => LeaveChannelsAsync(SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(channels)));

    /// <inheritdoc cref="LeaveChannelsAsync(ReadOnlyMemory{string})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask LeaveChannelsAsync(params string[] channels) => LeaveChannelsAsync(channels.AsMemory());

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
            await LeaveChannelsAsync(SpanMarshal.AsMemory(_ircChannels.AsSpan()));
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

    private void IrcClient_OnDataReceived(object? sender, ReceivedData data)
    {
        _ircHandler.Handle(data.Span);
        if (OnDataReceived is null)
        {
            data.Dispose();
            return;
        }

        OnDataReceived.Invoke(this, data);
    }

    private void IrcHandlerOnChatMessageReceived(object? sender, IChatMessage msg)
        => OnChatMessageReceived?.Invoke(this, msg);

    private void IrcHandlerOnRoomstateReceived(object? sender, Roomstate roomstate)
    {
        Channels.Update(in roomstate);
        OnRoomstateReceived?.Invoke(this, roomstate);
    }

    private async ValueTask IrcHandler_OnPingReceivedAsync(ReceivedData data)
    {
        try
        {
            using PooledStringBuilder builder = new(50);
            builder.Append(PongPrefix, data.Span);
            await SendRawAsync(builder.WrittenMemory);
        }
        finally
        {
            data.Dispose();
        }
    }

    private async Task ReconnectAfterConnectionExceptionAsync()
    {
        await _reconnectionLock.WaitAsync();

        try
        {
            if (_client.State is WebSocketState.Open or WebSocketState.Connecting)
            {
                return;
            }

            _client.CancelTasks();
            await Task.Delay(TimeSpan.FromSeconds(10));
            await _client.ConnectAsync(SpanMarshal.AsMemory(_ircChannels.AsSpan()));
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        finally
        {
            _reconnectionLock.Release();
        }
    }

    [SkipLocalsInit]
    private static string FormatChannel(ReadOnlySpan<char> channel, bool withHashtag = true)
    {
        Span<char> result = stackalloc char[channel.Length + 1];
        int length = FormatChannel(channel, withHashtag, result);
        return StringPool.Shared.GetOrAdd(result[..length]);
    }

    private static int FormatChannel(ReadOnlySpan<char> channel, bool withHashtag, Span<char> result)
    {
        if (!GetChannelPattern().IsMatch(channel))
        {
            ThrowInvalidChannelFormat(channel);
        }

        if (withHashtag)
        {
            if (channel[0] == ChannelPrefix)
            {
                channel.ToLowerInvariant(result);
                return channel.Length;
            }

            result[0] = ChannelPrefix;
            channel.ToLowerInvariant(result[1..]);
            return channel.Length + 1;
        }

        if (channel[0] == ChannelPrefix)
        {
            channel[1..].ToLowerInvariant(result);
            return channel.Length - 1;
        }

        channel.ToLowerInvariant(result);
        return channel.Length;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidChannelFormat(ReadOnlySpan<char> channel)
        => throw new FormatException($"The channel name (\"{channel}\") is in an invalid format.");

    [GeneratedRegex(@"^#?[a-z\d]\w{2,24}$", RegexOptions.Compiled | RegexOptions.IgnoreCase, 250)]
    private static partial Regex GetChannelPattern();

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        _client.Dispose();
        _reconnectionLock.Dispose();
        _ircChannels.Dispose();
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
