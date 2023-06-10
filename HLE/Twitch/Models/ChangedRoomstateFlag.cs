using System;

namespace HLE.Twitch.Models;

[Flags]
public enum ChangedRoomstateFlag
{
    EmoteOnly = 1 << 0,
    FollowersOnly = 1 << 1,
    R9K = 1 << 2,
    SlowMode = 1 << 3,
    SubsOnly = 1 << 4
}
