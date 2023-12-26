using System;

namespace HLE.Twitch.Tmi.Models;

[Flags]
public enum ChangedRoomStates
{
    None = 0,
    EmoteOnly = 1 << 0,
    FollowersOnly = 1 << 1,
    R9K = 1 << 2,
    SlowMode = 1 << 3,
    SubsOnly = 1 << 4
}
