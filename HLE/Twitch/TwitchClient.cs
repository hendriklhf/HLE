using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Twitch.Args;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public class TwitchClient
{
    public string Username { get; }

    public bool IsAnonymousLogin { get; }

    public bool IsConnected => _client.IsConnected;

    public ChannelList Channels { get; } = new();

    #region Events

    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;
    public event EventHandler<JoinedChannelArgs>? OnJoinedChannel;
    public event EventHandler<LeftChannelArgs>? OnLeftChannel;
    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;
    public event EventHandler<ChatMessage>? OnChatMessageReceived;
    public event EventHandler<ChatMessage>? OnChatMessageSent;
    public event EventHandler<string>? OnDataReceived;
    public event EventHandler<string>? OnDataSent;

    #endregion Events

    private readonly IrcClient _client;
    private readonly IrcHandler _ircHandler = new();
    private readonly List<string> _ircChannels = new();

    private static readonly Regex _channelPattern = new(@"^#?\w{3,25}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    public TwitchClient()
    {
        Username = "justinfan123";
        _client = new(Username, null);
        IsAnonymousLogin = true;

        SetEvents();
    }

    public TwitchClient(string username, string oAuthToken, bool isVerifiedBot = false)
    {
        Username = username;
        oAuthToken = ValidateOAuthToken(oAuthToken);
        _client = new(Username, oAuthToken, isVerifiedBot);

        SetEvents();
    }

    private void SetEvents()
    {
        _client.OnConnected += (_, e) => OnConnected?.Invoke(this, e);
        _client.OnDisconnected += (_, e) => OnDisconnected?.Invoke(this, e);
        _client.OnDataReceived += IrcClient_OnDataReceived;
        _client.OnDataSent += IrcClient_OnDataSent;

        _ircHandler.OnJoinedChannel += (_, e) => OnJoinedChannel?.Invoke(this, e);
        _ircHandler.OnRoomstateReceived += IrcClient_OnRoomstateReceived;
        _ircHandler.OnRoomstateReceived += (_, e) => OnRoomstateReceived?.Invoke(this, e);
        _ircHandler.OnChatMessageReceived += IrcHandler_OnChatMessageReceived;
        _ircHandler.OnPingReceived += (_, e) => SendRaw($"PONG :{e.Message}");
        _ircHandler.OnLeftChannel += (_, e) => OnLeftChannel?.Invoke(this, e);
    }

    public void Send(string channel, string message)
    {
        if (!IsConnected || IsAnonymousLogin)
        {
            return;
        }

        channel = FormatChannel(channel);
        if (!_ircChannels.Contains(channel))
        {
            return;
        }

        _client.SendMessage(channel, message);
    }

    public void Send(long channelId, string message)
    {
        string? channel = Channels.FirstOrDefault(c => c.Id == channelId)?.Name;
        if (channel is null)
        {
            return;
        }

        Send($"#{channel}", message);
    }

    public void SendRaw(string message)
    {
        if (!IsConnected || IsAnonymousLogin)
        {
            return;
        }

        _client.SendRaw(message);
    }

    public void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        _client.Connect(_ircChannels);
    }

    public void JoinChannel(string channel)
    {
        channel = FormatChannel(channel);
        _ircChannels.Add(channel);
        if (IsConnected)
        {
            _client.JoinChannel(channel);
        }
    }

    public void JoinChannels(params string[] channels)
    {
        channels = FormatChannels(channels).ToArray();
        _ircChannels.AddRange(channels);
        if (IsConnected)
        {
            channels.ForEach(_client.JoinChannel);
        }
    }

    public void JoinChannels(IEnumerable<string> channels)
    {
        string[] chnls = FormatChannels(channels).ToArray();
        _ircChannels.AddRange(chnls);
        if (IsConnected)
        {
            chnls.ForEach(_client.JoinChannel);
        }
    }

    public void LeaveChannel(string channel)
    {
        if (_ircChannels.Count == 0)
        {
            return;
        }

        channel = FormatChannel(channel);
        _ircChannels.Remove(channel);
        Channels.Remove(channel[1..]);
        if (IsConnected)
        {
            _client.LeaveChannel(channel);
        }
    }

    public void LeaveChannels(params string[] channels)
    {
        foreach (string channel in channels)
        {
            LeaveChannel(channel);
        }
    }

    public void LeaveChannels()
    {
        if (_ircChannels.Count == 0)
        {
            return;
        }

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

    public void Disconnect()
    {
        if (IsConnected)
        {
            _client.Disconnect();
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    private void IrcClient_OnDataReceived(object? sender, Memory<byte> e)
    {
        string message = Encoding.UTF8.GetString(e.ToArray());
        string[] lines = message.Remove("\r").Split('\n');
        lines.ForEach(l =>
        {
            _ircHandler.Handle(l);
            OnDataReceived?.Invoke(this, l);
        });
#if DEBUG
        Console.WriteLine(message);
#endif
    }

    private void IrcClient_OnDataSent(object? sender, Memory<byte> e)
    {
        string message = Encoding.UTF8.GetString(e.ToArray());
        OnDataSent?.Invoke(this, message);
#if DEBUG
        Console.WriteLine(message);
#endif
    }

    private void IrcClient_OnRoomstateReceived(object? sender, RoomstateArgs e)
    {
        Channels.Update(e);
    }

    private void IrcHandler_OnChatMessageReceived(object? sender, ChatMessage e)
    {
        if (string.Equals(e.Username, Username, StringComparison.OrdinalIgnoreCase))
        {
            OnChatMessageSent?.Invoke(this, e);
        }
        else
        {
            OnChatMessageReceived?.Invoke(this, e);
        }
    }

    private static string FormatChannel(string channel)
    {
        return (channel.StartsWith('#') ? channel : $"#{channel}").ToLower();
    }

    private static IEnumerable<string> FormatChannels(IEnumerable<string> channels)
    {
        return channels.Where(c => _channelPattern.IsMatch(c)).Select(FormatChannel);
    }

    private static string ValidateOAuthToken(string oAuthToken)
    {
        Regex oAuthPattern = new(@"^oauth:[a-z0-9]{30}$", RegexOptions.None, TimeSpan.FromMilliseconds(250));
        Regex oAuthPatternNoPrefix = new(@"^[a-z0-9]{30}$", RegexOptions.None, TimeSpan.FromMilliseconds(250));

        if (oAuthPattern.IsMatch(oAuthToken))
        {
            return oAuthToken.ToLower();
        }

        if (oAuthPatternNoPrefix.IsMatch(oAuthToken))
        {
            return $"oauth:{oAuthToken.ToLower()}";
        }

        throw new FormatException("The OAuthToken is in an invalid format.");
    }
}
