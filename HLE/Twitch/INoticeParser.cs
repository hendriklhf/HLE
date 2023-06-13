using System;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public interface INoticeParser
{
    Notice Parse(ReadOnlySpan<char> ircMessage);

    Notice Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
