using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public sealed class TimeEfficientChatMessageParser : ChatMessageParser, IEquatable<TimeEfficientChatMessageParser>
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public override IChatMessage Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        Badge[] badgeInfos = Array.Empty<Badge>();
        int badgeInfoCount = 0;
        Badge[] badges = Array.Empty<Badge>();
        int badgeCount = 0;
        Color color = Color.Empty;
        ReadOnlySpan<char> displayName = ReadOnlySpan<char>.Empty;
        ChatMessageFlag flags = 0;
        Guid id = Guid.Empty;
        long channelId = 0;
        long tmiSentTs = 0;
        long userId = 0;

        ReadOnlySpan<char> tags = ircMessage[1..indicesOfWhitespaces[0]];

        int equalsSignIndex = tags.IndexOf('=');
        while (equalsSignIndex > 0)
        {
            int semicolonIndex = tags.IndexOf(';');
            ReadOnlySpan<char> tag = tags[..Unsafe.As<int, Index>(ref semicolonIndex)];
            tags = semicolonIndex > 0 ? tags[(semicolonIndex + 1)..] : ReadOnlySpan<char>.Empty;

            ReadOnlySpan<char> key = tag[..equalsSignIndex];
            ReadOnlySpan<char> value = tag[(equalsSignIndex + 1)..];
            equalsSignIndex = tags.IndexOf('=');
            switch (key)
            {
                case _badgeInfoTag:
                    badgeInfos = GetBadges(value, out badgeInfoCount);
                    break;
                case _badgesTag:
                    badges = GetBadges(value, out badgeCount);
                    break;
                case _colorTag:
                    color = GetColor(value);
                    break;
                case _displayNameTag:
                    displayName = GetDisplayName(value);
                    break;
                case _firstMsgTag:
                    flags |= GetIsFirstMsg(value);
                    break;
                case _idTag:
                    id = GetId(value);
                    break;
                case _modTag:
                    flags |= GetIsModerator(value);
                    break;
                case _roomIdTag:
                    channelId = GetChannelId(value);
                    break;
                case _subscriberTag:
                    flags |= GetIsSubscriber(value);
                    break;
                case _tmiSentTsTag:
                    tmiSentTs = GetTmiSentTs(value);
                    break;
                case _turboTag:
                    flags |= GetIsTurboUser(value);
                    break;
                case _userIdTag:
                    userId = GetUserId(value);
                    break;
            }
        }

        flags |= GetIsAction(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<char> username = GetUsername(ircMessage, indicesOfWhitespaces, displayName.Length);
        ReadOnlySpan<char> channel = GetChannel(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<char> message = GetMessage(ircMessage, indicesOfWhitespaces, (flags & ChatMessageFlag.IsAction) == ChatMessageFlag.IsAction);

        return new TimeEfficientChatMessage(badgeInfos, badgeInfoCount, badges, badgeCount, flags)
        {
            Channel = new(channel),
            ChannelId = channelId,
            Color = color,
            DisplayName = new(displayName),
            Id = id,
            Message = new(message),
            TmiSentTs = tmiSentTs,
            UserId = userId,
            Username = new(username)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Badge[] GetBadges(ReadOnlySpan<char> value, out int badgeCount)
    {
        badgeCount = 0;
        if (value.Length == 0)
        {
            return Array.Empty<Badge>();
        }

        Badge[] badges = new Badge[5];
        while (value.Length > 0)
        {
            int indexOfComma = value.IndexOf(',');
            ReadOnlySpan<char> info = value[..Unsafe.As<int, Index>(ref indexOfComma)];
            value = indexOfComma == -1 ? ReadOnlySpan<char>.Empty : value[(indexOfComma + 1)..];
            int slashIndex = info.IndexOf('/');
            string name = new(info[..slashIndex]);
            string level = new(info[(slashIndex + 1)..]);
            badges[badgeCount++] = new(name, level);
        }

        return badges;
    }

    public bool Equals(TimeEfficientChatMessageParser? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is TimeEfficientChatMessageParser other && Equals(other);
    }

    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }

    public static bool operator ==(TimeEfficientChatMessageParser? left, TimeEfficientChatMessageParser? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TimeEfficientChatMessageParser? left, TimeEfficientChatMessageParser? right)
    {
        return !(left == right);
    }
}
