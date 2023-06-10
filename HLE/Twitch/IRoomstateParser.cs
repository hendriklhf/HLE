using System;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public interface IRoomstateParser
{
    void Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, out Roomstate roomstate);

    void Parse(ReadOnlySpan<char> ircMessage, out Roomstate roomstate);
}
