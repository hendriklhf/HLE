using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public sealed class MemoryEfficientChatMessageParser : ChatMessageParser, IEquatable<MemoryEfficientChatMessageParser>
{
    [Pure]
    public override IChatMessage Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        Badge[] badgeInfos = [];
        int badgeInfoCount = 0;
        Badge[] badges = [];
        int badgeCount = 0;
        Color color = Color.Empty;
        ReadOnlySpan<char> displayName = [];
        ChatMessageTags chatMessageTags = 0;
        Guid id = Guid.Empty;
        long channelId = 0;
        long tmiSentTs = 0;
        long userId = 0;

        ReadOnlySpan<char> tags = ircMessage[1..indicesOfWhitespaces[0]];

        int equalsSignIndex = tags.IndexOf('=');
        while (equalsSignIndex > 0)
        {
            int semicolonIndex = tags.IndexOf(';');
            // semicolonIndex is -1 if no semicolon has been found, reinterpreting -1 as Index returns ^0
            ReadOnlySpan<char> tag = tags[..Unsafe.As<int, Index>(ref semicolonIndex)];
            tags = semicolonIndex > 0 ? tags[(semicolonIndex + 1)..] : [];

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
                    chatMessageTags |= GetIsFirstMsg(value);
                    break;
                case _idTag:
                    id = GetId(value);
                    break;
                case _modTag:
                    chatMessageTags |= GetIsModerator(value);
                    break;
                case _roomIdTag:
                    channelId = GetChannelId(value);
                    break;
                case _subscriberTag:
                    chatMessageTags |= GetIsSubscriber(value);
                    break;
                case _tmiSentTsTag:
                    tmiSentTs = GetTmiSentTs(value);
                    break;
                case _turboTag:
                    chatMessageTags |= GetIsTurboUser(value);
                    break;
                case _userIdTag:
                    userId = GetUserId(value);
                    break;
            }
        }

        chatMessageTags |= GetIsAction(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<char> username = GetUsername(ircMessage, indicesOfWhitespaces, displayName.Length);
        ReadOnlySpan<char> channel = GetChannel(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<char> message = GetMessage(ircMessage, indicesOfWhitespaces, (chatMessageTags & ChatMessageTags.IsAction) == ChatMessageTags.IsAction);

        char[] usernameBuffer = ArrayPool<char>.Shared.Rent(25);
        char[] displayNameBuffer = ArrayPool<char>.Shared.Rent(25);
        char[] messageBuffer = ArrayPool<char>.Shared.Rent(512);

        username.CopyTo(usernameBuffer);
        displayName.CopyTo(displayNameBuffer);
        message.CopyTo(messageBuffer);

        return new MemoryEfficientChatMessage(badgeInfos, badgeInfoCount, badges, badgeCount, chatMessageTags, displayNameBuffer, usernameBuffer, username.Length, messageBuffer, message.Length)
        {
            Channel = StringPool.Shared.GetOrAdd(channel),
            ChannelId = channelId,
            Color = color,
            Id = id,
            TmiSentTs = tmiSentTs,
            UserId = userId,
            DisplayName = string.Empty,
            Message = string.Empty,
            Username = string.Empty
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Badge[] GetBadges(ReadOnlySpan<char> value, out int badgeCount)
    {
        badgeCount = 0;
        if (value.Length == 0)
        {
            return [];
        }

        Badge[] badges = ArrayPool<Badge>.Shared.Rent(5);
        while (value.Length != 0)
        {
            int indexOfComma = value.IndexOf(',');
            // indexOfComma is -1 if no comma has been found, reinterpreting -1 as Index returns ^0
            ReadOnlySpan<char> info = value[..Unsafe.As<int, Index>(ref indexOfComma)];
            value = indexOfComma < 0 ? [] : value[(indexOfComma + 1)..];
            int slashIndex = info.IndexOf('/');
            string name = StringPool.Shared.GetOrAdd(info[..slashIndex]);
            string level = StringPool.Shared.GetOrAdd(info[(slashIndex + 1)..]);
            badges[badgeCount++] = new(name, level);
        }

        return badges;
    }

    [Pure]
    public bool Equals(MemoryEfficientChatMessageParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(MemoryEfficientChatMessageParser? left, MemoryEfficientChatMessageParser? right) => Equals(left, right);

    public static bool operator !=(MemoryEfficientChatMessageParser? left, MemoryEfficientChatMessageParser? right) => !(left == right);
}
