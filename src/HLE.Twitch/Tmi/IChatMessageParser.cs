using System;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public interface IChatMessageParser
{
    IChatMessage Parse(ReadOnlySpan<byte> ircMessage);

    IChatMessage Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
