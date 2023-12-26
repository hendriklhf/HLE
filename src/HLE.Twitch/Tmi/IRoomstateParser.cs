using System;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public interface IRoomstateParser
{
    void Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, out Roomstate roomstate);

    void Parse(ReadOnlySpan<byte> ircMessage, out Roomstate roomstate);
}
