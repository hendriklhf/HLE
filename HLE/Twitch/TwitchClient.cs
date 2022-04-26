using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Twitch.Args;

namespace HLE.Twitch;

public class TwitchClient
{
    public string Username { get; }

    public bool IsAnonymousLogin { get; }

    public bool IsConnected => _client.IsConnected;

    public ChannelList Channels { get; } = new();

    #region Events

    // public event EventHandler? OnConnected;
    public event EventHandler<JoinedChannelArgs>? OnJoinedChannel;

    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;
    // public event EventHandler? OnChatMessageReceived;
    // public event EventHandler? OnWhisperReceived;

    #endregion Events

    private readonly IrcClient _client;
    private readonly IrcHandler _ircHandler = new();
    private List<string> _ircChannels = new();

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
        _client.OnDataReceived += IrcClient_OnDataReceived;
        _client.OnDataSent += IrcClient_OnDataSent;

        _ircHandler.OnJoinedChannel += (_, e) => OnJoinedChannel?.Invoke(this, e);
        _ircHandler.OnRoomstateReceived += IrcClient_OnRoomstateReceived;
        _ircHandler.OnRoomstateReceived += (_, e) => OnRoomstateReceived?.Invoke(this, e);
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

    public void Connect()
    {
        if (IsConnected)
        {
            return;
        }

        if (_ircChannels.Count == 0)
        {
            throw new Exception("The channel list can't be empty.");
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

    public void LeaveChannel(string channel)
    {
        _ircChannels.Remove(channel);
        if (IsConnected)
        {
            _client.LeaveChannel(channel);
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
            _client.LeaveChannels(_ircChannels);
        }

        _ircChannels.Clear();
    }

    public void Disconnect()
    {
        if (IsConnected)
        {
            _client.Disconnect();
        }
    }

    public void SetChannels(IEnumerable<string> channels)
    {
        if (IsConnected)
        {
            return;
        }

        _ircChannels = FormatChannels(channels);
    }

    private void IrcClient_OnDataReceived(object? sender, Memory<byte> e)
    {
        string message = Encoding.UTF8.GetString(e.ToArray());
        string[] lines = message.Remove("\r").Split("\n");
        lines.ForEach(l => _ircHandler.Handle(l));
#if DEBUG
        Console.WriteLine(message);
#endif
    }

    private static void IrcClient_OnDataSent(object? sender, Memory<byte> e)
    {
#if DEBUG
        Console.WriteLine(Encoding.UTF8.GetString(e.ToArray()));
#endif
    }

    private void IrcClient_OnRoomstateReceived(object? sender, RoomstateArgs e)
    {
        Channels.Add(e);
    }

    private static string FormatChannel(string channel)
    {
        return (channel.StartsWith('#') ? channel : $"#{channel}").ToLower();
    }

    private static List<string> FormatChannels(IEnumerable<string> channels)
    {
        return channels.Where(c => _channelPattern.IsMatch(c)).Select(FormatChannel).ToList();
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
