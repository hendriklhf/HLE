using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// A class that represents a Twitch chat client.
/// </summary>
public sealed class TwitchClient : IDisposable
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
    /// Is invoked if the client sends a chat messages.
    /// </summary>
    public event EventHandler<ChatMessage>? OnChatMessageSent;

    /// <summary>
    /// Is invoked if data is received from the chat server.
    /// </summary>
    public event EventHandler<ReadOnlyMemory<char>>? OnDataReceived;

    /// <summary>
    /// Is invoked if data is sent to the chat server.
    /// </summary>
    public event EventHandler<ReadOnlyMemory<char>>? OnDataSent;

    #endregion Events

    private readonly IrcClient _client;
    private readonly IrcHandler _ircHandler = new();
    private readonly List<string> _ircChannels = new();

    private static readonly Regex _channelPattern = new(@"^#?\w{3,25}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
    private static readonly Regex _anonymousLoginPattern = new(@"^justinfan\w+$", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

    private const string _anonymousUsername = "justinfan123";
    private const char _channelPrefix = '#';

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
            ClientType.Tcp => new TcpIrcClient(_anonymousUsername, null, options),
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
            ClientType.Tcp => new TcpIrcClient(username, oAuthToken, options),
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
            TcpIrcClient => ClientType.Tcp,
            _ => throw new ArgumentOutOfRangeException($"Unknown {nameof(Models.ClientType)}: {ClientType}")
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
        _client.OnDataSent += (_, e) => OnDataSent?.Invoke(this, e);

        _ircHandler.OnJoinedChannel += (_, e) => OnJoinedChannel?.Invoke(this, e);
        _ircHandler.OnLeftChannel += (_, e) => OnLeftChannel?.Invoke(this, e);
        _ircHandler.OnRoomstateReceived += IrcHandler_OnRoomstateReceived;
        _ircHandler.OnChatMessageReceived += IrcHandler_OnChatMessageReceived;
        _ircHandler.OnPingReceived += (_, e) => SendRaw("PONG :" + new string(e.Span));
        _ircHandler.OnReconnectReceived += (_, _) => _client.Reconnect(CollectionsMarshal.AsSpan(_ircChannels).AsMemoryUnsafe());
    }

    /// <inheritdoc cref="Send(ReadOnlySpan{char},ReadOnlyMemory{char})"/>
    public void Send(string channel, string message)
    {
        Send((ReadOnlySpan<char>)channel, message.AsMemory());
    }

    /// <inheritdoc cref="Send(ReadOnlySpan{char},ReadOnlyMemory{char})"/>
    public void Send(string channel, ReadOnlyMemory<char> message)
    {
        Send((ReadOnlySpan<char>)channel, message);
    }

    /// <summary>
    /// Sends a chat messages.
    /// </summary>
    /// <param name="channel">The username of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if <paramref name="channel"/> is in the wrong format.</exception>
    public void Send(ReadOnlySpan<char> channel, ReadOnlyMemory<char> message)
    {
        if (!IsConnected || IsAnonymousLogin)
        {
            return;
        }

        string? prefixedChannel = Channels[channel]?.PrefixedName;
        if (prefixedChannel is null)
        {
            return;
        }

        _client.SendMessage(prefixedChannel.AsMemory(), message);
    }

    /// <inheritdoc cref="Send(long, ReadOnlyMemory{char})"/>
    public void Send(long channelId, string message)
    {
        Send(channelId, message.AsMemory());
    }

    /// <summary>
    /// Sends a chat message.
    /// </summary>
    /// <param name="channelId">The user id of the channel owner</param>
    /// <param name="message">The message that will be sent</param>
    public void Send(long channelId, ReadOnlyMemory<char> message)
    {
        if (!IsConnected || IsAnonymousLogin)
        {
            return;
        }

        string? prefixedChannel = Channels[channelId]?.PrefixedName;
        if (prefixedChannel is null)
        {
            return;
        }

        _client.SendMessage(prefixedChannel.AsMemory(), message);
    }

    /// <inheritdoc cref="SendRaw(ReadOnlyMemory{char})"/>
    public void SendRaw(string rawMessage)
    {
        SendRaw(rawMessage.AsMemory());
    }

    /// <summary>
    /// Sends a raw message to the chat server.
    /// </summary>
    /// <param name="rawMessage">The raw message</param>
    public void SendRaw(ReadOnlyMemory<char> rawMessage)
    {
        if (!IsConnected)
        {
            return;
        }

        _client.SendRaw(rawMessage);
    }

    /// <summary>
    /// Connects the client to the chat server.
    /// </summary>
    public void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        _client.Connect(_ircChannels);
    }

    /// <summary>
    /// If the client is not connected, adds the channel to the channel list, otherwise connects the client to the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <exception cref="FormatException">Throws a <see cref="FormatException"/> if the <paramref name="channel"/> is in the wrong format.</exception>
    public void JoinChannel(string channel)
    {
        channel = FormatChannel(channel);
        _ircChannels.Add(channel);
        if (IsConnected)
        {
            _client.JoinChannel(channel);
        }
    }

    /// <inheritdoc cref="JoinChannels(ReadOnlySpan{String})"/>
    public void JoinChannels(IEnumerable<string> channels)
    {
        JoinChannels(channels.ToArray());
    }

    /// <inheritdoc cref="JoinChannels(ReadOnlySpan{String})"/>
    public void JoinChannels(List<string> channels)
    {
        JoinChannels(CollectionsMarshal.AsSpan(channels));
    }

    /// <inheritdoc cref="JoinChannels(ReadOnlySpan{String})"/>
    public void JoinChannels(params string[] channels)
    {
        JoinChannels((ReadOnlySpan<string>)channels);
    }

    /// <summary>
    /// If the client is not connected, adds the channels to the channel list, otherwise connects the client to the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    /// // <exception cref="FormatException">Throws a <see cref="FormatException"/> if any of <paramref name="channels"/> is in the wrong format.</exception>
    public void JoinChannels(ReadOnlySpan<string> channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            JoinChannel(channels[i]);
        }
    }

    /// <summary>
    /// If the client is not connected, removes the channel from the channel list, otherwise leaves the channel.
    /// </summary>
    /// <param name="channel">The channel</param>
    public void LeaveChannel(string channel)
    {
        if (_ircChannels.Count == 0)
        {
            return;
        }

        channel = FormatChannel(channel);
        _ircChannels.Remove(channel);
        Channels.Remove(channel);
        if (IsConnected)
        {
            _client.LeaveChannel(channel);
        }
    }

    /// <inheritdoc cref="LeaveChannels(ReadOnlySpan{string})"/>
    public void LeaveChannels(IEnumerable<string> channels)
    {
        LeaveChannels(channels.ToArray());
    }

    /// <inheritdoc cref="LeaveChannels(ReadOnlySpan{string})"/>
    public void LeaveChannels(List<string> channels)
    {
        LeaveChannels(CollectionsMarshal.AsSpan(channels));
    }

    /// <inheritdoc cref="LeaveChannels(ReadOnlySpan{string})"/>
    public void LeaveChannels(params string[] channels)
    {
        LeaveChannels(channels.AsEnumerable());
    }

    /// <summary>
    /// If the client is not connected, removes the channels from the channel list, otherwise leaves the channels.
    /// </summary>
    /// <param name="channels">The channels</param>
    public void LeaveChannels(ReadOnlySpan<string> channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            LeaveChannel(channels[i]);
        }
    }

    /// <summary>
    /// If the client is not connected, clears the channel list, otherwise also leaves all channels.
    /// </summary>
    public void LeaveChannels()
    {
        if (IsConnected)
        {
            foreach (string channel in _ircChannels)
            {
                LeaveChannel(channel);
            }
        }

        Channels.Clear();
        _ircChannels.Clear();
    }

    /// <summary>
    /// Disconnects the client from the chat server.
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected)
        {
            return;
        }

        _client.Disconnect();
        Channels.Clear();
    }

    private void IrcClient_OnDataReceived(object? sender, ReadOnlyMemory<char> data)
    {
        _ircHandler.Handle(data);
        OnDataReceived?.Invoke(this, data);
    }

    private void IrcHandler_OnChatMessageReceived(object? sender, ChatMessage msg)
    {
        if (msg.Username == Username)
        {
            OnChatMessageSent?.Invoke(this, msg);
        }
        else
        {
            OnChatMessageReceived?.Invoke(this, msg);
        }
    }

    private void IrcHandler_OnRoomstateReceived(object? sender, RoomstateArgs roomstateArgs)
    {
        Channels.Update(in roomstateArgs);
        OnRoomstateReceived?.Invoke(this, roomstateArgs);
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
}
