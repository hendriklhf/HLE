using System;
using System.Text.RegularExpressions;
using HLE.Twitch.Args;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public class IrcHandler
{
    public event EventHandler<JoinedChannelArgs>? OnJoinedChannel;
    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;
    public event EventHandler<ChatMessage>? OnChatMessageReceived;

    private readonly Regex _joinChannelPattern = new(@"^:\w{3,25}!\w{3,25}@\w{3,25}\.tmi\.twitch\.tv\sJOIN\s#\w{3,25}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    private readonly Regex _roomstatePattern = new(@"^@emote-only=[01];followers-only=-?\d+;r9k=[01];rituals=[01];room-id=\d+;slow=\d+;subs-only=[01]\s:tmi\.twitch\.tv\sROOMSTATE\s#\w{3,25}$",
        RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    private readonly Regex _privmsgPattern = new(@"^@\S+=\S*(;\S+=\S*)*\s:\w{3,25}!\w{3,25}@\w{3,25}\.tmi\.twitch\.tv\sPRIVMSG\s#\w{3,25}\s:.*$", RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    public void Handle(string ircMessage)
    {
        if (_joinChannelPattern.IsMatch(ircMessage))
        {
            string[] split = ircMessage.Split();
            string username = split[0].TakeBetween(':', '!');
            string channel = split[^1][1..];
            OnJoinedChannel?.Invoke(this, new(username, channel));
        }
        else if (_roomstatePattern.IsMatch(ircMessage))
        {
            OnRoomstateReceived?.Invoke(this, new(ircMessage));
        }
        else if (_privmsgPattern.IsMatch(ircMessage))
        {
            ChatMessage message = new(ircMessage);
            OnChatMessageReceived?.Invoke(this, message);
        }
    }
}
