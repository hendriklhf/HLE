using System;

namespace HLE.Twitch.Args;

public class RoomstateArgs : EventArgs
{
    public bool EmoteOnly { get; init; }

    public int FollowerOnly { get; init; }

    public bool R9K { get; init; }

    public bool Rituals { get; init; }

    public long ChannelId { get; init; }

    public string Channel { get; }

    public int SlowMode { get; init; }

    public bool SubOnly { get; init; }

    public RoomstateArgs(string ircMessage)
    {
        string[] split = ircMessage.Split();
        string[] roomstateSplit = split[0][1..].Split(';');
        Channel = split[^1][1..];
        EmoteOnly = roomstateSplit[0][^1] == '1';
        FollowerOnly = Utils.EndingNumbersPattern.Match(roomstateSplit[1]).Value.ToInt();
        R9K = roomstateSplit[2][^1] == '1';
        Rituals = roomstateSplit[3][^1] == '1';
        ChannelId = Utils.EndingNumbersPattern.Match(roomstateSplit[4]).Value.ToInt();
        SlowMode = Utils.EndingNumbersPattern.Match(roomstateSplit[5]).Value.ToInt();
        SubOnly = roomstateSplit[6][^1] == '1';
    }
}
