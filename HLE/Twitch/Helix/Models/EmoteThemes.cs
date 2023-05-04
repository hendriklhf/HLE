using System;

namespace HLE.Twitch.Helix.Models;

[Flags]
public enum EmoteThemes
{
    Light = 1 << 0,
    Dark = 1 << 1
}
