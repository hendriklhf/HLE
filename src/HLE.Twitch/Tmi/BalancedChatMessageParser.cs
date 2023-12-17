using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Strings;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public sealed class BalancedChatMessageParser : ChatMessageParser, IEquatable<BalancedChatMessageParser>
{
    [Pure]
    [SkipLocalsInit]
    public override IChatMessage Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        Badge[] badgeInfos = [];
        Badge[] badges = [];
        Color color = Color.Empty;
        ReadOnlySpan<char> displayName = [];
        ChatMessageFlags chatMessageFlags = 0;
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
                case BadgeInfoTag:
                    badgeInfos = GetBadges(value);
                    break;
                case BadgesTag:
                    badges = GetBadges(value);
                    break;
                case ColorTag:
                    color = GetColor(value);
                    break;
                case DisplayNameTag:
                    displayName = GetDisplayName(value);
                    break;
                case FirstMsgTag:
                    chatMessageFlags |= GetIsFirstMsg(value);
                    break;
                case IdTag:
                    id = GetId(value);
                    break;
                case ModTag:
                    chatMessageFlags |= GetIsModerator(value);
                    break;
                case RoomIdTag:
                    channelId = GetChannelId(value);
                    break;
                case SubscriberTag:
                    chatMessageFlags |= GetIsSubscriber(value);
                    break;
                case TmiSentTsTag:
                    tmiSentTs = GetTmiSentTs(value);
                    break;
                case TurboTag:
                    chatMessageFlags |= GetIsTurboUser(value);
                    break;
                case UserIdTag:
                    userId = GetUserId(value);
                    break;
            }
        }

        chatMessageFlags |= GetIsAction(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<char> username = GetUsername(ircMessage, indicesOfWhitespaces, displayName.Length);
        ReadOnlySpan<char> channel = GetChannel(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<char> message = GetMessage(ircMessage, indicesOfWhitespaces, (chatMessageFlags & ChatMessageFlags.IsAction) != 0);

        return new BalancedChatMessage(badgeInfos, badges, chatMessageFlags)
        {
            Channel = StringPool.Shared.GetOrAdd(channel),
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

    [Pure]
    public bool Equals([NotNullWhen(true)] BalancedChatMessageParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(BalancedChatMessageParser? left, BalancedChatMessageParser? right) => Equals(left, right);

    public static bool operator !=(BalancedChatMessageParser? left, BalancedChatMessageParser? right) => !(left == right);
}
