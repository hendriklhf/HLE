using System;

namespace HLE.Twitch.Args;

public class RoomstateArgs : EventArgs
{
    public bool EmoteOnly { get; init; }

    public int FollowerOnly { get; init; }

    public bool R9K { get; init; }

    public bool Rituals { get; init; }

    public int ChannelId { get; init; }

    public string Channel { get; }

    public int SlowMode { get; init; }

    public bool SubOnly { get; init; }

    public RoomstateArgs(string channel)
    {
        Channel = channel;
    }
}
