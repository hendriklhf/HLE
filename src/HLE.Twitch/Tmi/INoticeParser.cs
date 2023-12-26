using System;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public interface INoticeParser
{
    Notice Parse(ReadOnlySpan<byte> ircMessage);

    Notice Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
