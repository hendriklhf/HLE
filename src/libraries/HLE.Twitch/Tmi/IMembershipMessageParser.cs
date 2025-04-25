using System;

namespace HLE.Twitch.Tmi;

public interface IMembershipMessageParser
{
    LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<byte> ircMessage);

    LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<byte> ircMessage);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
