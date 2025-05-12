using System;

namespace HLE.Twitch.Tmi;

public interface IMembershipMessageParser
{
    PartChannelMessage ParsePartChannelMessage(ReadOnlySpan<byte> ircMessage);

    PartChannelMessage ParsePartChannelMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<byte> ircMessage);

    JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);
}
