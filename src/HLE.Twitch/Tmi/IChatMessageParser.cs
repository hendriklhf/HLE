using System;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public interface IChatMessageParser
{
    IChatMessage Parse(ReadOnlySpan<char> ircMessage);

    IChatMessage Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
