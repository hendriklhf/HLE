﻿using System;

namespace HLE.Twitch.Api.Models;

[Flags]
public enum EmoteImageFormats
{
    Static = 1 << 0,
    Animated = 1 << 1
}