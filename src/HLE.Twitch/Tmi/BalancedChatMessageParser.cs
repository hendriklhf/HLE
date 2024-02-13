using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Strings;
using HLE.Twitch.Tmi.Models;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Twitch.Tmi;

public sealed class BalancedChatMessageParser : ChatMessageParser, IEquatable<BalancedChatMessageParser>
{
    [Pure]
    [SkipLocalsInit]
    [MustDisposeResource]
    public override BalancedChatMessage Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        Badge[] badgeInfos = [];
        Badge[] badges = [];
        Color color = Color.Empty;
        ReadOnlySpan<byte> displayName = [];
        ChatMessageFlags chatMessageFlags = 0;
        Guid id = Guid.Empty;
        long channelId = 0;
        long tmiSentTs = 0;
        long userId = 0;

        ReadOnlySpan<byte> tags = ircMessage[1..indicesOfWhitespaces[0]];

        int equalsSignIndex = tags.IndexOf((byte)'=');
        while (equalsSignIndex > 0)
        {
            int semicolonIndex = tags.IndexOf((byte)';');
            // semicolonIndex is -1 if no semicolon has been found, reinterpreting -1 as Index returns ^0
            ReadOnlySpan<byte> tag = tags[..Unsafe.As<int, Index>(ref semicolonIndex)];
            tags = semicolonIndex > 0 ? tags[(semicolonIndex + 1)..] : [];

            ReadOnlySpan<byte> key = tag[..equalsSignIndex];
            ReadOnlySpan<byte> value = tag[(equalsSignIndex + 1)..];
            equalsSignIndex = tags.IndexOf((byte)'=');
            switch (key[0])
            {
                case (byte)'b' when key.SequenceEqual(BadgeInfoTag):
                    badgeInfos = GetBadges(value);
                    break;
                case (byte)'b' when key.SequenceEqual(BadgesTag):
                    badges = GetBadges(value);
                    break;
                case (byte)'c' when key.SequenceEqual(ColorTag):
                    color = GetColor(value);
                    break;
                case (byte)'d' when key.SequenceEqual(DisplayNameTag):
                    displayName = GetDisplayName(value);
                    break;
                case (byte)'f' when key.SequenceEqual(FirstMsgTag):
                    chatMessageFlags |= GetIsFirstMsg(value);
                    break;
                case (byte)'i' when key.SequenceEqual(IdTag):
                    GetId(value, out id);
                    break;
                case (byte)'m' when key.SequenceEqual(ModTag):
                    chatMessageFlags |= GetIsModerator(value);
                    break;
                case (byte)'r' when key.SequenceEqual(RoomIdTag):
                    channelId = GetChannelId(value);
                    break;
                case (byte)'s' when key.SequenceEqual(SubscriberTag):
                    chatMessageFlags |= GetIsSubscriber(value);
                    break;
                case (byte)'t' when key.SequenceEqual(TmiSentTsTag):
                    tmiSentTs = GetTmiSentTs(value);
                    break;
                case (byte)'t' when key.SequenceEqual(TurboTag):
                    chatMessageFlags |= GetIsTurboUser(value);
                    break;
                case (byte)'u' when key.SequenceEqual(UserIdTag):
                    userId = GetUserId(value);
                    break;
            }
        }

        chatMessageFlags |= GetIsAction(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<byte> username = GetUsername(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<byte> channel = GetChannel(ircMessage, indicesOfWhitespaces);
        ReadOnlySpan<byte> message = GetMessage(ircMessage, indicesOfWhitespaces, (chatMessageFlags & ChatMessageFlags.IsAction) != 0);

        return new(badgeInfos, badges, chatMessageFlags)
        {
            Channel = StringPool.Shared.GetOrAdd(channel, Encoding.ASCII),
            ChannelId = channelId,
            Color = color,
            DisplayName = BytesToLazyString(displayName, Encoding.UTF8),
            Id = id,
            Message = BytesToLazyString(message, Encoding.UTF8),
            TmiSentTs = tmiSentTs,
            UserId = userId,
            Username = BytesToLazyString(username, Encoding.ASCII)
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
