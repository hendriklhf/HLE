using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// A class that represents a Twitch chat client.
/// </summary>
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
    public bool IsConnected => _client.IsConnected;

    /// <summary>
    /// The client type of the connection. Can be either a websocket or a TCP connection.
    /// </summary>
    public ClientType ClientType { get; }

    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL => _client.UseSSL;

    /// <summary>
    /// The list of channels the client is connected to. Channels can be retrieved by the owner's username or user id in order to read the room state, e.g. if slow-mode is on.
    /// </summary>
    public ChannelList Channels { get; } = new();

    #region Events

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
    public event EventHandler<JoinedChannelArgs>? OnJoinedChannel;

    /// <summary>
    /// Is invoked if a user leaves a channel.
    /// </summary>
    public event EventHandler<LeftChannelArgs>? OnLeftChannel;

    /// <summary>
    /// Is invoked if a room state has been received.
    /// </summary>
    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;

    /// <summary>
    /// Is invoked if a chat message has been received.
    /// </summary>
    public event EventHandler<ChatMessage>? OnChatMessageReceived;

    /// <summary>
    /// Is invoked if data is received from the chat server. If this event is subscribed to, the <see cref="ReceivedData"/> instance has to be manually disposed.
    /// Read more in the documentation of the <see cref="ReceivedData"/> class.
    /// </summary>
    public event EventHandler<ReceivedData>? OnDataReceived;

    #endregion Events

    private readonly IrcClient _client;
    private readonly IrcHandler _ircHandler = new();
    private readonly List<string> _ircChannels = new();
    private readonly Memory<char> _pingResponseBuffer = new char[50];
    private bool _reconnecting;

    private static readonly Regex _channelPattern = new(@"^#?\w{3,25}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
    private static readonly Regex _anonymousLoginPattern = new(@"^justinfan[0-9]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

    private const string _anonymousUsername = "justinfan123";
    private const char _channelPrefix = '#';
    private const string _pongPrefix = "PONG :";

    /// <summary>
    /// The constructor for an anonymous chat client. An anonymous chat client can only receive messages, but cannot send any messages.
    /// Connects with the username "justinfan123".
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    /// </summary>
    public TwitchClient(ClientOptions options = default)
    {
        ClientType = options.ClientType;
        _client = ClientType switch
        {
            ClientType.WebSocket => new WebSocketIrcClient(_anonymousUsername, null, options),
            _ => throw new ArgumentOutOfRangeException($"Unknown {nameof(Models.ClientType)}: {ClientType}")
        };
        IsAnonymousLogin = true;

        SetEvents();
    }

    /// <summary>
    /// The constructor for a normal chat client.
    /// </summary>
    /// <param name="username">The username of the client</param>
    /// <param name="oAuthToken">The OAuth token of the client</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if <paramref name="username"/> or <paramref name="oAuthToken"/> are in a wrong format.</exception>
    public TwitchClient(string username, string oAuthToken, ClientOptions options = default)
    {
        username = FormatChannel(username, false);
        oAuthToken = ValidateOAuthToken(oAuthToken);
        ClientType = options.ClientType;
        _client = ClientType switch
        {
            ClientType.WebSocket => new WebSocketIrcClient(username, oAuthToken, options),
            _ => throw new ArgumentOutOfRangeException($"Unknown {nameof(Models.ClientType)}: {ClientType}")
        };
        IsAnonymousLogin = false;

        SetEvents();
    }

    /// <summary>
    /// The constructor for a chat client witch an already created <see cref="IrcClient"/>.
    /// </summary>
    /// <param name="ircClient">The IRC client.</param>
    public TwitchClient(IrcClient ircClient)
    {
        ClientType = ircClient switch
        {
            WebSocketIrcClient => ClientType.WebSocket,
            _ => (ClientType)(-1)
        };
        IsAnonymousLogin = _anonymousLoginPattern.IsMatch(ircClient.Username);
        _client = ircClient;

        SetEvents();
    }

    private void SetEvents()
    {
        _client.OnConnected += (_, e) => OnConnected?.Invoke(this, e);
        _client.OnDisconnected += (_, e) => OnDisconnected?.Invoke(this, e);
        _client.OnDataReceived += IrcClient_OnDataReceived;
        _client.OnConnectionException += (_, _) => { ReconnectAfterConnectionException().Wait(); };

        _ircHandler.OnJoinedChannel += (_, e) => OnJoinedChannel?.Invoke(this, e);
        _ircHandler.OnLeftChannel += (_, e) => OnLeftChannel?.Invoke(this, e);
        _ircHandler.OnRoomstateReceived += IrcHandler_OnRoomstateReceived;
        _ircHandler.OnChatMessageReceived += IrcHandler_OnChatMessageReceived;
        _ircHandler.OnPingReceived += IrcHandler_OnPingReceived;
        _ircHandler.OnReconnectReceived += (_, _) => _client.Reconnect(CollectionsMarshal.AsSpan(_ircChannels).AsMemoryDangerous());
    }

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    public async Task SendAsync(string channel, string message)
    {
        await SendAsync(channel.AsMemory(), message.AsMemory());
    }

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    public async Task SendAsync(string channel, PoolBufferStringBuilder builder)
    {
        await SendAsync(channel.AsMemory(), builder.WrittenMemory);
    }

    /// <inheritdoc cref="SendAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    public async Task SendAsync(string channel, ReadOnlyMemory<char> message)
    {
        await SendAsync(channel.AsMemory(), message);
    }

    /// <summary>
    /// Asynchronously sends a chat messages.
    /// </summary>
    /// <param name="channel">The username of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if <paramref name="channel"/> is in the wrong format.</exception>
    public async Task SendAsync(ReadOnlyMemory<char> channel, ReadOnlyMemory<char> message)
    {
        if (!IsConnected)
        {
            throw ThrowHelper.NotConnected;
        }

        if (IsAnonymousLogin)
        {
            throw ThrowHelper.AnonymousConnection;
        }

        string prefixedChannel = (Channels[channel.Span]?._prefixedName) ?? throw ThrowHelper.NotConnectedToTheSpecifiedChannel;
        await _client.SendMessageAsync(prefixedChannel.AsMemory(), message);
    }

    /// <inheritdoc cref="Send(long,ReadOnlyMemory{char})"/>
    public void Send(long channelId, string message)
    {
        SendAsync(channelId, message.AsMemory());
    }

    /// <summary>
    /// Sends a chat message.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    public void Send(long channelId, ReadOnlyMemory<char> message)
    {
        SendAsync(channelId, message);
    }

    /// <inheritdoc cref="SendAsync(long,System.ReadOnlyMemory{char})"/>
    public async Task SendAsync(long channelId, string message)
    {
        await SendAsync(channelId, message.AsMemory());
    }

    /// <summary>
    /// Asynchronously sends a chat message and also disposes the <see cref="PoolBufferStringBuilder"/> by default.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner.</param>
    /// <param name="builder">The builder that contains the message that will be sent.</param>
    /// <param name="disposeBuilder">If true, disposes the builder after the message was sent.</param>
    public async Task SendAsync(long channelId, PoolBufferStringBuilder builder, bool disposeBuilder = true)
    {
        await SendAsync(channelId, builder.WrittenMemory);
        if (disposeBuilder)
        {
            builder.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously sends a chat message.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    public async Task SendAsync(long channelId, ReadOnlyMemory<char> message)
    {
        if (!IsConnected)
        {
            throw ThrowHelper.NotConnected;
        }

        if (IsAnonymousLogin)
        {
            throw ThrowHelper.AnonymousConnection;
        }

        string prefixedChannel = (Channels[channelId]?._prefixedName) ?? throw ThrowHelper.NotConnectedToTheSpecifiedChannel;
        await _client.SendMessageAsync(prefixedChannel.AsMemory(), message);
    }

    /// <inheritdoc cref="SendRaw(ReadOnlyMemory{char})"/>
    public void SendRaw(string rawMessage)
    {
        SendRawAsync(rawMessage.AsMemory());
    }

    /// <summary>
    /// Sends a raw message to the chat server.
    /// </summary>
    /// <param name="rawMessage">The raw message</param>
    public void SendRaw(ReadOnlyMemory<char> rawMessage)
    {
        SendRawAsync(rawMessage);
    }

    /// <inheritdoc cref="SendRawAsync(System.ReadOnlyMemory{char})"/>
    public async Task SendRawAsync(string rawMessage)
    {
        await SendRawAsync(rawMessage.AsMemory());
    }

    /// <summary>
    /// Asynchronously sends a raw message to the chat server.
    /// </summary>
    /// <param name="rawMessage">The raw message</param>
    public async Task SendRawAsync(ReadOnlyMemory<char> rawMessage)
    {
        if (!IsConnected)
        {
            throw ThrowHelper.NotConnected;
        }

        await _client.SendRawAsync(rawMessage);
    }

    /// <summary>
    /// Connects the client to the chat server.
    /// </summary>
    public void Connect()
    {
        ConnectAsync(_ircChannels);
    }

    /// <summary>
    /// Asynchronously connects the client to the chat server. This method will be exited after the client has joined all channels.
    /// </summary>
    public async Task ConnectAsync()
    {
        if (IsConnected)
        {
            return;
        }

        await ConnectAsync(_ircChannels);
    }

    private async ValueTask ConnectAsync(List<string> ircChannels)
    {
        await _client.ConnectAsync(ircChannels);
    }

    /// <inheritdoc cref="JoinChannelsAsync(ReadOnlyMemory{string})"/>
    public async Task JoinChannelsAsync(List<string> channels)
    {
        await JoinChannelsAsync(CollectionsMarshal.AsSpan(channels).AsMemoryDangerous());
    }

    /// <inheritdoc cref="JoinChannelsAsync(ReadOnlyMemory{string})"/>
    public async Task JoinChannelsAsync(string[] channels)
    {
        await JoinChannelsAsync(channels.AsMemory());
    }

    /// <summary>
    /// If the client is not connected, adds the channels to the channel list, otherwise asynchronously connects the client to the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    /// // <exception cref="FormatException">Throws a <see cref="FormatException"/> if any of <paramref name="channels"/> is in the wrong format.</exception>
    public async Task JoinChannelsAsync(ReadOnlyMemory<string> channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            await JoinChannelAsync(channels.Span[i]);
        }
    }

    /// <inheritdoc cref="JoinChannels(ReadOnlyMemory{string})"/>
    public void JoinChannels(List<string> channels)
    {
        JoinChannels(CollectionsMarshal.AsSpan(channels).AsMemoryDangerous());
    }

    /// <inheritdoc cref="JoinChannels(ReadOnlyMemory{string})"/>
    public void JoinChannels(params string[] channels)
    {
        JoinChannels(channels.AsMemory());
    }

    /// <summary>
    /// If the client is not connected, adds the channels to the channel list, otherwise connects the client to the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    /// // <exception cref="FormatException">Throws a <see cref="FormatException"/> if any of <paramref name="channels"/> is in the wrong format.</exception>
    public void JoinChannels(ReadOnlyMemory<string> channels)
    {
        JoinChannelsAsync(channels);
    }

    /// <inheritdoc cref="JoinChannel(ReadOnlyMemory{char})"/>
    public void JoinChannel(string channel)
    {
        JoinChannelAsync(channel.AsMemory());
    }

    /// <summary>
    /// If the client is not connected, adds the channel to the channel list, otherwise connects the client to the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if the <paramref name="channel"/> is in the wrong format.</exception>
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
    /// If the client is not connected, adds the channel to the channel list, otherwise asynchronously connects the client to the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if the <paramref name="channel"/> is in the wrong format.</exception>
    public async Task JoinChannelAsync(ReadOnlyMemory<char> channel)
    {
        string channelString = FormatChannel(channel.Span);
        _ircChannels.Add(channelString);
        if (IsConnected)
        {
            await _client.JoinChannelAsync(channelString.AsMemory());
        }
    }

    /// <inheritdoc cref="LeaveChannelAsync(ReadOnlyMemory{char})"/>
    public async Task LeaveChannelAsync(string channel)
    {
        await LeaveChannelAsync(channel.AsMemory());
    }

    /// <summary>
    /// If the client is not connected, removes the channel from the channel list, otherwise asynchronously leaves the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    public async Task LeaveChannelAsync(ReadOnlyMemory<char> channel)
    {
        if (_ircChannels.Count == 0)
        {
            return;
        }

        string channelString = FormatChannel(channel.Span);
        _ircChannels.Remove(channelString);
        Channels.Remove(channel.Span);
        if (IsConnected)
        {
            await _client.LeaveChannelAsync(channel);
        }
    }

    /// <inheritdoc cref="LeaveChannel(ReadOnlyMemory{char})"/>
    public void LeaveChannel(string channel)
    {
        LeaveChannelAsync(channel.AsMemory());
    }

    /// <summary>
    /// If the client is not connected, removes the channel from the channel list, otherwise leaves the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    public void LeaveChannel(ReadOnlyMemory<char> channel)
    {
        LeaveChannelAsync(channel);
    }

    /// <inheritdoc cref="LeaveChannelsAsync(ReadOnlyMemory{string})"/>
    public async Task LeaveChannelsAsync(List<string> channels)
    {
        await LeaveChannelsAsync(CollectionsMarshal.AsSpan(channels).AsMemoryDangerous());
    }

    /// <inheritdoc cref="LeaveChannelsAsync(ReadOnlyMemory{string})"/>
    public async Task LeaveChannelsAsync(params string[] channels)
    {
        await LeaveChannelsAsync(channels.AsMemory());
    }

    /// <summary>
    /// If the client is not connected, removes the channels from the channel list, otherwise leaves the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    public async Task LeaveChannelsAsync(ReadOnlyMemory<string> channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            await LeaveChannelAsync(channels.Span[i]);
        }
    }

    /// <inheritdoc cref="LeaveChannels(ReadOnlyMemory{string})"/>
    public void LeaveChannels(List<string> channels)
    {
        LeaveChannels(CollectionsMarshal.AsSpan(channels).AsMemoryDangerous());
    }

    /// <inheritdoc cref="LeaveChannels(ReadOnlyMemory{string})"/>
    public void LeaveChannels(params string[] channels)
    {
        LeaveChannels(channels.AsMemory());
    }

    /// <summary>
    /// If the client is not connected, removes the channels from the channel list, otherwise leaves the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    public void LeaveChannels(ReadOnlyMemory<string> channels)
    {
        LeaveChannelsAsync(channels);
    }

    /// <summary>
    /// If the client is not connected, clears the channel list, otherwise also leaves all channels.
    /// </summary>
    public void LeaveChannels()
    {
        LeaveChannelsAsync();
    }

    /// <summary>
    /// If the client is not connected, clears the channel list, otherwise also asynchronously leaves all channels.
    /// </summary>
    public async Task LeaveChannelsAsync()
    {
        if (IsConnected)
        {
            await LeaveChannelsAsync(_ircChannels);
        }

        Channels.Clear();
        _ircChannels.Clear();
    }

    /// <summary>
    /// Disconnects the client from the chat server.
    /// </summary>
    public void Disconnect()
    {
        DisconnectAsync();
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

    private void IrcHandler_OnChatMessageReceived(object? sender, ChatMessage msg)
    {
        OnChatMessageReceived?.Invoke(this, msg);
    }

    private void IrcHandler_OnRoomstateReceived(object? sender, RoomstateArgs roomstateArgs)
    {
        Channels.Update(in roomstateArgs);
        OnRoomstateReceived?.Invoke(this, roomstateArgs);
    }

    private void IrcHandler_OnPingReceived(object? sender, ReceivedData data)
    {
        try
        {
            ((ReadOnlySpan<char>)_pongPrefix).CopyTo(_pingResponseBuffer.Span);
            int bufferLength = _pongPrefix.Length;
            data.Span.CopyTo(_pingResponseBuffer.Span);
            bufferLength += data.Length;
            SendRawAsync(_pingResponseBuffer[..bufferLength]).Wait();
        }
        finally
        {
            data.Dispose();
        }
    }

    private async Task ReconnectAfterConnectionException()
    {
        if (_reconnecting || IsConnected)
        {
            return;
        }

        _reconnecting = true;
        try
        {
            _client.CancelTasks();
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
        finally
        {
            _reconnecting = false;
        }

        await _client.ConnectAsync(CollectionsMarshal.AsSpan(_ircChannels).AsMemoryDangerous());
    }

    private static string FormatChannel(ReadOnlySpan<char> channel, bool withHashtag = true)
    {
        Span<char> result = stackalloc char[channel.Length + 1];
        int length = FormatChannel(channel, withHashtag, result);
        return new(result[..length]);
    }

    private static int FormatChannel(ReadOnlySpan<char> channel, bool withHashtag, Span<char> result)
    {
        if (!_channelPattern.IsMatch(channel))
        {
            throw new FormatException($"The channel name (\"{channel}\") is in an invalid format.");
        }

        if (withHashtag)
        {
            if (channel[0] == _channelPrefix)
            {
                channel.ToLower(result, CultureInfo.InvariantCulture);
                return channel.Length;
            }

            result[0] = _channelPrefix;
            channel.ToLower(result[1..], CultureInfo.InvariantCulture);
            return channel.Length + 1;
        }

        if (channel[0] == _channelPrefix)
        {
            channel[1..].ToLower(result, CultureInfo.InvariantCulture);
            return channel.Length - 1;
        }

        channel.ToLower(result, CultureInfo.InvariantCulture);
        return channel.Length;
    }

    private static string ValidateOAuthToken(string oAuthToken)
    {
        Regex oAuthPattern = new(@"^oauth:[a-zA-Z0-9]{30}$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        Regex oAuthPatternNoPrefix = new(@"^[a-zA-Z0-9]{30}$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

        if (oAuthPattern.IsMatch(oAuthToken))
        {
            return oAuthToken.ToLowerInvariant();
        }

        if (oAuthPatternNoPrefix.IsMatch(oAuthToken))
        {
            return $"oauth:{oAuthToken}".ToLowerInvariant();
        }

        throw new FormatException("The OAuthToken is in an invalid format.");
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public bool Equals(TwitchClient? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Username, Channels, _client, _ircHandler, _ircChannels, ClientType);
    }
}
