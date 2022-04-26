using System;
using System.Text.RegularExpressions;
using HLE.Twitch.Args;

namespace HLE.Twitch;

public class IrcHandler
{
    public event EventHandler<JoinedChannelArgs>? OnJoinedChannel;
    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;

    private readonly Regex _joinChannelPattern = new(@"^:\w{3,25}!\w{3,25}@\w{3,25}\.tmi\.twitch\.tv\sJOIN\s#\w{3,25}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    // @followers-only=15;room-id=616177816 :tmi.twitch.tv ROOMSTATE #lbnshlfe
    private readonly Regex _roomstatePattern = new(@"^@emote-only=[01];followers-only=-?\d+;r9k=[01];rituals=[01];room-id=\d+;slow=\d+;subs-only=[01]\s:tmi\.twitch\.tv\sROOMSTATE\s#\w{3,25}$",
        RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    private readonly Regex _endingNumbersPattern = new(@"-?\d+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    public void Handle(string ircMessage)
    {
        string[] split = ircMessage.Split();
        if (_joinChannelPattern.IsMatch(ircMessage))
        {
            string username = split[0].TakeBetween(':', '!');
            string channel = split[^1][1..];
            OnJoinedChannel?.Invoke(this, new(username, channel));
        }
        else if (_roomstatePattern.IsMatch(ircMessage))
        {
            string channel = split[^1][1..];
            string[] roomstateSplit = split[0].Split(';');
            RoomstateArgs args = new(channel)
            {
                EmoteOnly = roomstateSplit[0][^1] == '1',
                FollowerOnly = _endingNumbersPattern.Match(roomstateSplit[1]).Value.ToInt(),
                R9K = roomstateSplit[2][^1] == '1',
                Rituals = roomstateSplit[3][^1] == '1',
                ChannelId = _endingNumbersPattern.Match(roomstateSplit[4]).Value.ToInt(),
                SlowMode = _endingNumbersPattern.Match(roomstateSplit[5]).Value.ToInt(),
                SubOnly = roomstateSplit[6][^1] == '1'
            };
            OnRoomstateReceived?.Invoke(this, args);
        }
    }
}
