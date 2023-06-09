using System;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public interface IChatMessageParser
{
    IChatMessage Parse(ReadOnlySpan<char> ircMessage);

    IChatMessage Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
