using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Threading;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Represents a Twitch chat client.
/// </summary>
public sealed partial class TwitchClient : IAsyncDisposable, IEquatable<TwitchClient>
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
    /// </summary>
    public event AsyncEventHandler<TwitchClient>? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event AsyncEventHandler<TwitchClient>? OnDisconnected;

    internal readonly WebSocketIrcClient _client;
    internal readonly IrcHandler _ircHandler;
    private readonly IrcChannelList _ircChannels;

    private readonly ChannelReader<ChatMessage>? _messageReader;
    private readonly Channel<Roomstate>? _publicRoomstateChannel;
    private readonly ChannelReader<JoinChannelMessage>? _joinReader;
    private readonly ChannelReader<PartChannelMessage>? _partReader;
    private readonly ChannelReader<Notice>? _noticeReader;

    [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "analyzer is wrong, it is assigned")]
    private readonly ChannelReader<Roomstate> _roomstateReader;
    [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "analyzer is wrong, it is assigned")]
    private readonly ChannelReader<Bytes> _pingReader;

    private const string AnonymousUsername = "justinfan123";

    /// <summary>
    /// The constructor for an anonymous chat client. An anonymous chat client can only receive messages, but cannot send any messages.
    /// Connects with the username "justinfan123".
    /// </summary>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TwitchClient(ClientOptions options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        _client = new(AnonymousUsername, OAuthToken.Empty, options);
        IsAnonymousLogin = true;
        _ircChannels = new();

        Readers readers = default;
        readers.ChatMessageReader = ref _messageReader;
        readers.RoomstateReader = ref _roomstateReader!;
        readers.JoinReader = ref _joinReader;
        readers.PartReader = ref _partReader;
        readers.NoticeReader = ref _noticeReader;
        readers.PingReader = ref _pingReader!;

        _ircHandler = CreateIrcHandler(options, ref readers);

        Debug.Assert(_roomstateReader is not null);
        Debug.Assert(_pingReader is not null);

        SubscribeToEvents(options, ref _publicRoomstateChannel);
    }

    /// <summary>
    /// The constructor for a normal chat client.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if <paramref name="username"/> or <paramref name="oAuthToken"/> are in a wrong format.</exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TwitchClient(string username, OAuthToken oAuthToken, ClientOptions options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        username = ChannelFormatter.Format(username, false);
        _client = new(username, oAuthToken, options);
        _ircChannels = new();

        Readers readers = default;
        readers.ChatMessageReader = ref _messageReader;
        readers.RoomstateReader = ref _roomstateReader!;
        readers.JoinReader = ref _joinReader;
        readers.PartReader = ref _partReader;
        readers.NoticeReader = ref _noticeReader;
        readers.PingReader = ref _pingReader!;

        _ircHandler = CreateIrcHandler(options, ref readers);

        Debug.Assert(_roomstateReader is not null);
        Debug.Assert(_pingReader is not null);

        SubscribeToEvents(options, ref _publicRoomstateChannel);
    }

    private void SubscribeToEvents(ClientOptions options, ref Channel<Roomstate>? publicRoomstateChannel)
    {
        _client.OnConnected += (_, ct) => EventInvoker.InvokeAsync(OnConnected, this, ct);
        _client.OnDisconnected += (_, ct) => EventInvoker.InvokeAsync(OnDisconnected, this, ct);

        _ircHandler.OnReconnectReceived += IrcHandler_OnReconnectReceivedAsync;

        _client.RegisterAfterAutomaticReconnectionEvent(AfterAutomaticReconnectionEventAsync, this);

        for (int i = 0; i < options.WebSocketProcessingThreadCount; i++)
        {
            // TODO: pass token
            BackgroundTasks.ReadWebsocketBytesAsync(this, CancellationToken.None).Ignore();
        }

        // TODO: pass token
        BackgroundTasks.ReadPingsAsync(_client, _pingReader, CancellationToken.None).Ignore();

        if (options.ReceiveRoomstateMessages)
        {
            publicRoomstateChannel = System.Threading.Channels.Channel.CreateUnbounded<Roomstate>();
        }

        // TODO: pass token
        BackgroundTasks.ReadRoomstatesAsync(Channels, _roomstateReader, publicRoomstateChannel?.Writer, CancellationToken.None).Ignore();
    }

    private static IrcHandler CreateIrcHandler(ClientOptions options, ref Readers readers)
    {
        Channel<Bytes> pingChannel = System.Threading.Channels.Channel.CreateUnbounded<Bytes>();
        readers.PingReader = pingChannel.Reader;

        Channel<ChatMessage>? messageChannel = options.ReceiveChatMessages
            ? System.Threading.Channels.Channel.CreateUnbounded<ChatMessage>()
            : null;

        readers.ChatMessageReader = messageChannel?.Reader;

        Channel<Roomstate> roomstateChannel = System.Threading.Channels.Channel.CreateUnbounded<Roomstate>(new()
        {
            SingleReader = true
        });

        readers.RoomstateReader = roomstateChannel.Reader;

        Channel<JoinChannelMessage>? joinChannel = options.ReceiveMembershipMessages
            ? System.Threading.Channels.Channel.CreateUnbounded<JoinChannelMessage>()
            : null;

        readers.JoinReader = joinChannel?.Reader;

        Channel<PartChannelMessage>? partChannel = options.ReceiveMembershipMessages
            ? System.Threading.Channels.Channel.CreateUnbounded<PartChannelMessage>()
            : null;

        readers.PartReader = partChannel?.Reader;

        Channel<Notice>? noticeChannel = options.ReceiveNoticeMessages
            ? System.Threading.Channels.Channel.CreateUnbounded<Notice>()
            : null;

        readers.NoticeReader = noticeChannel?.Reader;

        return new(
            pingChannel.Writer,
            messageChannel?.Writer,
            roomstateChannel.Writer,
            joinChannel?.Writer,
            partChannel?.Writer,
            noticeChannel?.Writer
        );
    }

    private static Task AfterAutomaticReconnectionEventAsync(object state)
    {
        TwitchClient client = (TwitchClient)state;
        ReadOnlyMemory<ReadOnlyMemory<byte>> channels = client._ircChannels.GetUtf8Names().AsMemory();
        return client._client.AuthenticateAndJoinChannelsAsync(channels);
    }

    public ValueTask<ChatMessage> ReceiveChatMessageAsync(CancellationToken stoppingToken = default)
    {
        ChannelReader<ChatMessage>? reader = _messageReader;
        ArgumentNullException.ThrowIfNull(reader);
        return reader.ReadAsync(stoppingToken);
    }

    public ValueTask<Roomstate> ReceiveRoomstateAsync(CancellationToken stoppingToken = default)
    {
        ChannelReader<Roomstate>? reader = _publicRoomstateChannel?.Reader;
        ArgumentNullException.ThrowIfNull(reader);
        return reader.ReadAsync(stoppingToken);
    }

    public ValueTask<JoinChannelMessage> ReceiveJoinChannelMessageAsync(CancellationToken stoppingToken = default)
    {
        ChannelReader<JoinChannelMessage>? reader = _joinReader;
        ArgumentNullException.ThrowIfNull(reader);
        return reader.ReadAsync(stoppingToken);
    }

    public ValueTask<PartChannelMessage> ReceivePartChannelMessageAsync(CancellationToken stoppingToken = default)
    {
        ChannelReader<PartChannelMessage>? reader = _partReader;
        ArgumentNullException.ThrowIfNull(reader);
        return reader.ReadAsync(stoppingToken);
    }

    public ValueTask<Notice> ReceiveNoticeAsync(CancellationToken stoppingToken = default)
    {
        ChannelReader<Notice>? reader = _noticeReader;
        ArgumentNullException.ThrowIfNull(reader);
        return reader.ReadAsync(stoppingToken);
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
        await _client.SendMessageAsync(prefixedChannel.AsMemory(), messageUtf8.AsMemory()).ConfigureAwait(false);
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
        await _client.SendMessageAsync(prefixedChannel.AsMemory(), messageUtf8.AsMemory()).ConfigureAwait(false);
    }

    [DoesNotReturn]
    private static void ThrowNotConnectedToChannelException(ReadOnlyMemory<char> channel)
        => throw new NotConnectedToChannelException(new string(channel.Span));

    [DoesNotReturn]
    private static void ThrowNotConnectedToChannelException(long channelId)
        => throw new NotConnectedToChannelException(channelId);

    [DoesNotReturn]
    private static void ThrowAnonymousClientException() => throw new AnonymousClientException();

    /// <inheritdoc cref="SendRawAsync(ReadOnlyMemory{char})"/>
    public ValueTask SendRawAsync(string rawMessage) => SendRawAsync(rawMessage.AsMemory());

    /// <summary>
    /// Asynchronously sends a raw message to the chat server.
    /// </summary>
    /// <param name="rawMessage">The raw message</param>
    public async ValueTask SendRawAsync(ReadOnlyMemory<char> rawMessage)
    {
        using Bytes rawMessageUtf8 = Utf16ToUtf8(rawMessage.Span);
        await _client.SendRawAsync(rawMessageUtf8).ConfigureAwait(false);
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
            await JoinChannelsAsync(channelsMemory).ConfigureAwait(false);
            return;
        }

        foreach (string channel in channels)
        {
            await JoinChannelAsync(channel).ConfigureAwait(false);
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
            await JoinChannelAsync(channels.Span[i]).ConfigureAwait(false);
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
            await LeaveChannelsAsync(channelsMemory).ConfigureAwait(false);
            return;
        }

        foreach (string channel in channels)
        {
            await LeaveChannelAsync(channel).ConfigureAwait(false);
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
            await LeaveChannelAsync(channels.Span[i]).ConfigureAwait(false);
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
                await _client.LeaveChannelAsync(utf8Channels.Span[i]).ConfigureAwait(false);
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
        await _client.DisconnectAsync().ConfigureAwait(false);
        Channels.Clear();
    }

    private Task IrcHandler_OnReconnectReceivedAsync(IrcHandler _, CancellationToken cancellationToken)
        => _client.ReconnectAsync(_ircChannels.GetUtf8Names().AsMemory());

    [Pure]
    private static Bytes Utf16ToUtf8(ReadOnlySpan<char> chars)
    {
        Encoding utf8 = Encoding.UTF8;
        int maxByteCount = utf8.GetMaxByteCount(chars.Length);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxByteCount);
        int byteCount = utf8.GetBytes(chars, buffer);
        return Bytes.AsBytes(buffer, byteCount);
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();

    [Pure]
    public bool Equals([NotNullWhen(true)] TwitchClient? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(TwitchClient? left, TwitchClient? right) => Equals(left, right);

    public static bool operator !=(TwitchClient? left, TwitchClient? right) => !(left == right);
}
