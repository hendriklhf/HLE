using System;

namespace HLE.Twitch.Api.Models;

[Flags]
public enum EmoteThemes
{
    Light = 1 << 0,
    Dark = 1 << 1
}
