using System;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public interface IMembershipMessageParser
{
    LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<char> ircMessage);

    LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
