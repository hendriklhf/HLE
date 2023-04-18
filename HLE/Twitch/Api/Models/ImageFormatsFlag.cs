using System;

namespace HLE.Twitch.Api.Models;

[Flags]
public enum ImageFormatsFlag : byte
{
    Static = 1 << 0,
    Animated = 1 << 1
}
