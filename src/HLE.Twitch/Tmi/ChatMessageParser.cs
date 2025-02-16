using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Numerics;
using HLE.Text;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public static class ChatMessageParser
{
    private static ReadOnlySpan<byte> BadgeInfoTag => "badge-info"u8;

    private static ReadOnlySpan<byte> BadgesTag => "badges"u8;

    private static ReadOnlySpan<byte> ColorTag => "color"u8;

    private static ReadOnlySpan<byte> DisplayNameTag => "display-name"u8;

    private static ReadOnlySpan<byte> FirstMsgTag => "first-msg"u8;

    private static ReadOnlySpan<byte> IdTag => "id"u8;

    private static ReadOnlySpan<byte> ModTag => "mod"u8;

    private static ReadOnlySpan<byte> RoomIdTag => "room-id"u8;

    private static ReadOnlySpan<byte> SubscriberTag => "subscriber"u8;

    private static ReadOnlySpan<byte> TmiSentTsTag => "tmi-sent-ts"u8;

    private static ReadOnlySpan<byte> TurboTag => "turbo"u8;

    private static ReadOnlySpan<byte> UserIdTag => "user-id"u8;

    private static ReadOnlySpan<byte> ActionPrefix => ":\u0001ACTION"u8;

    private const int UpperCaseAMinus10 = 'A' - 10;
    private const int CharZero = '0';

    private const int MaximumWhitespacesNeededToHandle = 5;

    [Pure]
    [SkipLocalsInit]
    public static ChatMessage Parse(ReadOnlySpan<byte> ircMessage)
    {
        Span<int> indicesOfWhitespacesBuffer = stackalloc int[MaximumWhitespacesNeededToHandle];
        int whitespaceCount = ParsingHelpers.IndicesOf(ircMessage, (byte)' ', indicesOfWhitespacesBuffer, MaximumWhitespacesNeededToHandle);
        return Parse(ircMessage, indicesOfWhitespacesBuffer.SliceUnsafe(..whitespaceCount));
    }

    [Pure]
    [SkipLocalsInit]
    public static ChatMessage Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        Badge[] badgeInfos = [];
        int badgeInfoCount = 0;
        Badge[] badges = [];
        int badgeCount = 0;
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
            ReadOnlySpan<byte> tag = tags[..Unsafe.BitCast<int, Index>(semicolonIndex)];
            tags = semicolonIndex > 0 ? tags[(semicolonIndex + 1)..] : [];

            ReadOnlySpan<byte> key = tag[..equalsSignIndex];
            ReadOnlySpan<byte> value = tag[(equalsSignIndex + 1)..];
            equalsSignIndex = tags.IndexOf((byte)'=');
            switch (key[0])
            {
                case (byte)'b' when key.SequenceEqual(BadgeInfoTag):
                    badgeInfos = GetBadges(value, out badgeInfoCount);
                    break;
                case (byte)'b' when key.SequenceEqual(BadgesTag):
                    badges = GetBadges(value, out badgeCount);
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

        return new(badgeInfos, badgeInfoCount, badges, badgeCount, chatMessageFlags)
        {
            Channel = StringPool.Shared.GetOrAdd(channel, Encoding.ASCII),
            ChannelId = channelId,
            Color = color,
            Id = id,
            TmiSentTs = tmiSentTs,
            UserId = userId,
            DisplayName = BytesToLazyString(displayName, Encoding.UTF8),
            Message = BytesToLazyString(message, Encoding.UTF8),
            Username = BytesToLazyString(username, Encoding.ASCII)
        };
    }

    private static Badge[] GetBadges(ReadOnlySpan<byte> value, out int badgeCount)
    {
        badgeCount = 0;
        if (value.Length == 0)
        {
            return [];
        }

        Badge[] badges = ArrayPool<Badge>.Shared.Rent(5);
        do
        {
            int indexOfComma = value.IndexOf((byte)',');
            // indexOfComma is -1 if no comma has been found, reinterpreting -1 as Index returns ^0
            ReadOnlySpan<byte> info = value[..Unsafe.BitCast<int, Index>(indexOfComma)];
            value = indexOfComma < 0 ? [] : value[(indexOfComma + 1)..];
            int slashIndex = info.IndexOf((byte)'/');
            string name = StringPool.Shared.GetOrAdd(info[..slashIndex], Encoding.UTF8);
            string level = StringPool.Shared.GetOrAdd(info[(slashIndex + 1)..], Encoding.UTF8);
            badges[badgeCount++] = new(name, level);
        }
        while (value.Length != 0);

        return badges;
    }

    private static LazyString BytesToLazyString(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        Debug.Assert(bytes.Length != 0, "check if it is 0 before calling this method, return LazyString.Empty if it is");

        int maximumCharCount = encoding.GetMaxCharCount(bytes.Length);
        char[] chars = ArrayPool<char>.Shared.Rent(maximumCharCount);
        int charCount = encoding.GetChars(bytes, chars);
        return new(chars, charCount);
    }

    private static ReadOnlySpan<byte> GetChannel(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => ircMessage[(indicesOfWhitespaces[2] + 1)..indicesOfWhitespaces[3]][1..];

    private static ChatMessageFlags GetIsAction(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        if (indicesOfWhitespaces.Length < 5)
        {
            return 0;
        }

        ReadOnlySpan<byte> actionPrefix = ircMessage[(indicesOfWhitespaces[3] + 1)..indicesOfWhitespaces[4]];
        bool isAction = actionPrefix.SequenceEqual(ActionPrefix);
        return (ChatMessageFlags)(isAction.AsByte() << 4);
    }

    private static ReadOnlySpan<byte> GetUsername(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> username = ircMessage[(indicesOfWhitespaces[0] + 2)..];
        int indexOfExclamationMark = username.IndexOf((byte)'!');
        return username[..indexOfExclamationMark];
    }

    private static ReadOnlySpan<byte> GetMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, bool isAction)
    {
        if (!isAction)
        {
            Debug.Assert(!ircMessage.Contains((byte)1)); // '\u0001'
            return ircMessage[(indicesOfWhitespaces[3] + 2)..];
        }

        // skipping chars to speed up .IndexOf
        const int MaximumAmountOfCharsThatCanBeIgnored = 242;
        ircMessage = ircMessage[MaximumAmountOfCharsThatCanBeIgnored..];
        return ircMessage[(ircMessage.IndexOf((byte)1) + ActionPrefix.Length)..^1];
    }

    private static Color GetColor(ReadOnlySpan<byte> value)
        => value.Length == 0 ? Color.Empty : ParseHexColor(ref Unsafe.Add(ref MemoryMarshal.GetReference(value), 1));

    private static ReadOnlySpan<byte> GetDisplayName(ReadOnlySpan<byte> value)
    {
        bool isBackSlash = value[^2] == '\\';
        int asByte = isBackSlash.AsByte() << 1;
        ReadOnlySpan<byte> displayName = value[..^asByte];
        Debug.Assert(displayName.Length != 0);
        return displayName;
    }

    private static ChatMessageFlags GetIsFirstMsg(ReadOnlySpan<byte> value)
    {
        bool isFirstMessage = value[0] == '1';
        return (ChatMessageFlags)isFirstMessage.AsByte();
    }

    [SkipLocalsInit]
    private static void GetId(ReadOnlySpan<byte> value, out Guid id)
    {
        Span<char> chars = stackalloc char[value.Length];
        int charCount = Encoding.ASCII.GetChars(value, chars);
        id = Guid.ParseExact(chars[..charCount], "D");
    }

    private static ChatMessageFlags GetIsModerator(ReadOnlySpan<byte> value)
    {
        bool isModerator = value[0] == '1';
        return (ChatMessageFlags)(isModerator.AsByte() << 1);
    }

    private static long GetChannelId(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<long>(value);

    private static ChatMessageFlags GetIsSubscriber(ReadOnlySpan<byte> value)
    {
        bool isSubscriber = value[0] == '1';
        return (ChatMessageFlags)(isSubscriber.AsByte() << 2);
    }

    private static long GetTmiSentTs(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<long>(value);

    private static ChatMessageFlags GetIsTurboUser(ReadOnlySpan<byte> value)
    {
        bool isTurboUser = value[0] == '1';
        return (ChatMessageFlags)(isTurboUser.AsByte() << 3);
    }

    private static long GetUserId(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<long>(value);

    private static Color ParseHexColor(ref byte numberReference)
    {
        const int CharAAndZeroDiff = UpperCaseAMinus10 - CharZero;

        byte firstChar = Unsafe.Add(ref numberReference, 0);
        bool isFirstCharHexLetter = IsHexLetter(firstChar);
        int red = firstChar - (CharZero + (CharAAndZeroDiff * isFirstCharHexLetter.AsByte()));
        red <<= 4;

        byte thirdChar = Unsafe.Add(ref numberReference, 2);
        bool isThirdCharHexLetter = IsHexLetter(thirdChar);
        int green = thirdChar - (CharZero + (CharAAndZeroDiff * isThirdCharHexLetter.AsByte()));
        green <<= 4;

        byte fifthChar = Unsafe.Add(ref numberReference, 4);
        bool isFifthCharHexLetter = IsHexLetter(fifthChar);
        int blue = fifthChar - (CharZero + (CharAAndZeroDiff * isFifthCharHexLetter.AsByte()));
        blue <<= 4;

        byte secondChar = Unsafe.Add(ref numberReference, 1);
        bool isSecondCharHexLetter = IsHexLetter(secondChar);
        red |= secondChar - (CharZero + (CharAAndZeroDiff * isSecondCharHexLetter.AsByte()));

        byte forthChar = Unsafe.Add(ref numberReference, 3);
        bool isForthCharHexLetter = IsHexLetter(forthChar);
        green |= forthChar - (CharZero + (CharAAndZeroDiff * isForthCharHexLetter.AsByte()));

        byte sixthChar = Unsafe.Add(ref numberReference, 5);
        bool isSixthCharHexLetter = IsHexLetter(sixthChar);
        blue |= sixthChar - (CharZero + (CharAAndZeroDiff * isSixthCharHexLetter.AsByte()));

        return new((byte)red, (byte)green, (byte)blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHexLetter(byte hexChar) => hexChar > '9';
}
