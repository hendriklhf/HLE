using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using HLE.Memory;
using HLE.Numerics;
using HLE.Strings;
using HLE.Twitch.Tmi.Models;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Twitch.Tmi;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ChatMessageParser : IChatMessageParser, IEquatable<ChatMessageParser>
{
    private protected const string EscapedWhitespace = "\\s";

    private protected const int UpperCaseAMinus10 = 'A' - 10;
    private protected const int CharZero = '0';

    private protected const byte Semicolon = (byte)';';
    private protected const byte EqualsSign = (byte)'=';

    private protected static ReadOnlySpan<byte> BadgeInfoTag => "badge-info"u8;

    private protected static ReadOnlySpan<byte> BadgesTag => "badges"u8;

    private protected static ReadOnlySpan<byte> ColorTag => "color"u8;

    private protected static ReadOnlySpan<byte> DisplayNameTag => "display-name"u8;

    private protected static ReadOnlySpan<byte> FirstMsgTag => "first-msg"u8;

    private protected static ReadOnlySpan<byte> IdTag => "id"u8;

    private protected static ReadOnlySpan<byte> ModTag => "mod"u8;

    private protected static ReadOnlySpan<byte> RoomIdTag => "room-id"u8;

    private protected static ReadOnlySpan<byte> SubscriberTag => "subscriber"u8;

    private protected static ReadOnlySpan<byte> TmiSentTsTag => "tmi-sent-ts"u8;

    private protected static ReadOnlySpan<byte> TurboTag => "turbo"u8;

    private protected static ReadOnlySpan<byte> UserIdTag => "user-id"u8;

    private protected static ReadOnlySpan<byte> ActionPrefix => ":\u0001ACTION"u8;

    private const int MaximumWhitespacesNeededToHandle = 5;

    [Pure]
    [SkipLocalsInit]
    [MustDisposeResource]
    public IChatMessage Parse(ReadOnlySpan<byte> ircMessage)
    {
        Span<int> indicesOfWhitespacesBuffer = stackalloc int[MaximumWhitespacesNeededToHandle];
        int whitespaceCount = ParsingHelpers.IndicesOf(ircMessage, (byte)' ', indicesOfWhitespacesBuffer, MaximumWhitespacesNeededToHandle);
        return Parse(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
    }

    [Pure]
    [MustDisposeResource]
    public abstract IChatMessage Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);

    [MustDisposeResource]
    private protected static LazyString BytesToLazyString(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        int maximumCharCount = encoding.GetMaxCharCount(bytes.Length);
        RentedArray<char> chars = ArrayPool<char>.Shared.RentAsRentedArray(maximumCharCount);
        int charCount = encoding.GetChars(bytes, chars.AsSpan());
        return new(chars, charCount);
    }

    private protected static ReadOnlySpan<byte> GetChannel(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => ircMessage[(indicesOfWhitespaces[2] + 1)..indicesOfWhitespaces[3]][1..];

    private protected static ChatMessageFlags GetIsAction(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        if (indicesOfWhitespaces.Length < 5)
        {
            return 0;
        }

        ReadOnlySpan<byte> actionPrefix = ircMessage[(indicesOfWhitespaces[3] + 1)..indicesOfWhitespaces[4]];
        bool isAction = actionPrefix.SequenceEqual(ActionPrefix);
        int asByte = Unsafe.As<bool, byte>(ref isAction);
        return (ChatMessageFlags)(asByte << 4);
    }

    private protected static ReadOnlySpan<byte> GetUsername(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> username = ircMessage[(indicesOfWhitespaces[0] + 2)..];
        int indexOfExclamationMark = username.IndexOf((byte)'!');
        return username[..indexOfExclamationMark];
    }

    private protected static ReadOnlySpan<byte> GetMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, bool isAction)
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

    private protected static Badge[] GetBadges(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return [];
        }

        // TODO: IndicesOf
        Badge[] badges = new Badge[value.Count((byte)',') + 1];
        int badgeCount = 0;
        Encoding utf8 = Encoding.UTF8;
        do
        {
            int indexOfComma = value.IndexOf((byte)',');
            // indexOfComma is -1 if no comma has been found, reinterpreting -1 as Index returns ^0
            ReadOnlySpan<byte> info = value[..Unsafe.As<int, Index>(ref indexOfComma)];
            value = indexOfComma < 0 ? [] : value[(indexOfComma + 1)..];
            int slashIndex = info.IndexOf((byte)'/');
            string name = StringPool.Shared.GetOrAdd(info[..slashIndex], utf8);
            string level = StringPool.Shared.GetOrAdd(info[(slashIndex + 1)..], utf8);
            badges[badgeCount++] = new(name, level);
        }
        while (value.Length != 0);

        return badges;
    }

    private protected static Color GetColor(ReadOnlySpan<byte> value)
        => value.Length == 0 ? Color.Empty : ParseHexColor(ref Unsafe.Add(ref MemoryMarshal.GetReference(value), 1));

    private protected static ReadOnlySpan<byte> GetDisplayName(ReadOnlySpan<byte> value)
    {
        bool isBackSlash = value[^2] == '\\';
        int asByte = Unsafe.As<bool, byte>(ref isBackSlash) << 1;
        ReadOnlySpan<byte> displayName = value[..^asByte];
        Debug.Assert(displayName.Length != 0);
        return displayName;
    }

    private protected static ChatMessageFlags GetIsFirstMsg(ReadOnlySpan<byte> value)
    {
        bool isFirstMessage = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isFirstMessage);
        return (ChatMessageFlags)asByte;
    }

    private protected static void GetId(ReadOnlySpan<byte> value, out Guid id)
    {
        Span<char> chars = stackalloc char[value.Length];
        int charCount = Encoding.ASCII.GetChars(value, chars);
        id = Guid.ParseExact(chars[..charCount], "D");
    }

    private protected static ChatMessageFlags GetIsModerator(ReadOnlySpan<byte> value)
    {
        bool isModerator = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isModerator);
        return (ChatMessageFlags)(asByte << 1);
    }

    private protected static long GetChannelId(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<long>(value);

    private protected static ChatMessageFlags GetIsSubscriber(ReadOnlySpan<byte> value)
    {
        bool isSubscriber = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isSubscriber);
        return (ChatMessageFlags)(asByte << 2);
    }

    private protected static long GetTmiSentTs(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<long>(value);

    private protected static ChatMessageFlags GetIsTurboUser(ReadOnlySpan<byte> value)
    {
        bool isTurboUser = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isTurboUser);
        return (ChatMessageFlags)(asByte << 3);
    }

    private protected static long GetUserId(ReadOnlySpan<byte> value) => NumberHelpers.ParsePositiveNumber<long>(value);

    private static Color ParseHexColor(ref byte numberReference)
    {
        const int CharAAndZeroDiff = UpperCaseAMinus10 - CharZero;

        byte firstChar = Unsafe.Add(ref numberReference, 0);
        byte thirdChar = Unsafe.Add(ref numberReference, 2);
        byte fifthChar = Unsafe.Add(ref numberReference, 4);

        bool isFirstCharHexLetter = IsHexLetter(firstChar);
        bool isThirdCharHexLetter = IsHexLetter(thirdChar);
        bool isFifthCharHexLetter = IsHexLetter(fifthChar);

        int red = firstChar - (CharZero + (CharAAndZeroDiff * Unsafe.As<bool, byte>(ref isFirstCharHexLetter)));
        int green = thirdChar - (CharZero + (CharAAndZeroDiff * Unsafe.As<bool, byte>(ref isThirdCharHexLetter)));
        int blue = fifthChar - (CharZero + (CharAAndZeroDiff * Unsafe.As<bool, byte>(ref isFifthCharHexLetter)));

        red <<= 4;
        green <<= 4;
        blue <<= 4;

        byte secondChar = Unsafe.Add(ref numberReference, 1);
        byte forthChar = Unsafe.Add(ref numberReference, 3);
        byte sixthChar = Unsafe.Add(ref numberReference, 5);

        bool isSecondCharHexLetter = IsHexLetter(secondChar);
        bool isForthCharHexLetter = IsHexLetter(forthChar);
        bool isSixthCharHexLetter = IsHexLetter(sixthChar);

        red |= secondChar - (CharZero + (CharAAndZeroDiff * Unsafe.As<bool, byte>(ref isSecondCharHexLetter)));
        green |= forthChar - (CharZero + (CharAAndZeroDiff * Unsafe.As<bool, byte>(ref isForthCharHexLetter)));
        blue |= sixthChar - (CharZero + (CharAAndZeroDiff * Unsafe.As<bool, byte>(ref isSixthCharHexLetter)));

        return new((byte)red, (byte)green, (byte)blue);
    }

    private static bool IsHexLetter(byte hexChar) => hexChar > '9';

    [Pure]
    public bool Equals([NotNullWhen(true)] ChatMessageParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ChatMessageParser? left, ChatMessageParser? right) => Equals(left, right);

    public static bool operator !=(ChatMessageParser? left, ChatMessageParser? right) => !(left == right);
}
