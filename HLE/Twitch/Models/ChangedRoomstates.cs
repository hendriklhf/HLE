using System;

namespace HLE.Twitch.Models;

[Flags]
public enum ChangedRoomstates : byte
{
    EmoteOnly = 1,
    FollowersOnly = 2,
    R9K = 4,
    SlowMode = 8,
    SubsOnly = 16
}
