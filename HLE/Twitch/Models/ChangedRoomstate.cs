using System;

namespace HLE.Twitch.Models;

[Flags]
public enum ChangedRoomstate : byte
{
    EmoteOnly = 1,
    FollowersOnly = 2,
    R9K = 4,
    SlowMode = 8,
    SubsOnly = 16
}
