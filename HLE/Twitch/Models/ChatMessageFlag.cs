using System;

namespace HLE.Twitch.Models;

[Flags]
internal enum ChatMessageFlag
{
    IsFirstMessage = 1 << 0,
    IsModerator = 1 << 1,
    IsSubscriber = 1 << 2,
    IsTurboUser = 1 << 3,
    IsAction = 1 << 4
}
