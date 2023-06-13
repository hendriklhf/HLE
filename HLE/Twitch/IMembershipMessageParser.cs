using System;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public interface IMembershipMessageParser
{
    LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<char> ircMessage);

    LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
