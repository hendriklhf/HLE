using System;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public interface IRoomstateParser
{
    void Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, out Roomstate roomstate);

    void Parse(ReadOnlySpan<char> ircMessage, out Roomstate roomstate);
}
