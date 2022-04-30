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

    // @badge-info=;badges=moderator/1,twitchconEU2022/1;color=#C29900;display-name=Strbhlfe;emotes=;first-msg=0;flags=;id=961a90d4-30bd-45c2-b9b4-7b679dd0dac5;mod=1;room-id=616177816;subscriber=0;tmi-sent-ts=1651339278001;turbo=0;user-id=87633910;user-type=mod :strbhlfe!strbhlfe@strbhlfe.tmi.twitch.tv PRIVMSG #lbnshlfe :text text abc text
    private readonly Regex _privmsgPattern = new(@"^@badge-info=.*;badges=(\w+/\d+,?)*;color=#[A-Fa-f0-9]{6};display-name=\w{3,25};emotes=.*;first-msg=[01];flags=.*;id=[A-Fa-f0-9]{8}-
        ([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{8};mod=[01];room-id=\d+;subscriber=[01];tmi-sent-ts=\d+;turbo=[01];user-id=\d+;user-type=\w+\s:\w{3,25}!\w{3,25}@{3,25}\.tmi\.twitch\.tv\sPRIVMSG\s#\w{3,25}\s:.*$",
        RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

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
