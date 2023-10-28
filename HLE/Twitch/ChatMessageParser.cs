using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Strings;
using HLE.Twitch.Models;

namespace HLE.Twitch;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class ChatMessageParser : IChatMessageParser, IEquatable<ChatMessageParser>
{
    private protected const string _actionPrefix = ":\u0001ACTION";
    private protected const string _escapedWhitespace = "\\s";

    private protected const char _upperCaseAMinus10 = (char)('A' - 10);
    private protected const char _charZero = '0';

    private protected const string _badgeInfoTag = "badge-info";
    private protected const string _badgesTag = "badges";
    private protected const string _colorTag = "color";
    private protected const string _displayNameTag = "display-name";
    private protected const string _firstMsgTag = "first-msg";
    private protected const string _idTag = "id";
    private protected const string _modTag = "mod";
    private protected const string _roomIdTag = "room-id";
    private protected const string _subscriberTag = "subscriber";
    private protected const string _tmiSentTsTag = "tmi-sent-ts";
    private protected const string _turboTag = "turbo";
    private protected const string _userIdTag = "user-id";

    private const int _maximumWhitespacesNeededToHandle = 5;

    [Pure]
    [SkipLocalsInit]
    public IChatMessage Parse(ReadOnlySpan<char> ircMessage)
    {
        Span<int> indicesOfWhitespacesBuffer = stackalloc int[_maximumWhitespacesNeededToHandle];
        int whitespaceCount = ParsingHelper.GetIndicesOfWhitespaces(ircMessage, ref MemoryMarshal.GetReference(indicesOfWhitespacesBuffer), _maximumWhitespacesNeededToHandle);
        return Parse(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
    }

    [Pure]
    public abstract IChatMessage Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ReadOnlySpan<char> GetChannel(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => ircMessage[(indicesOfWhitespaces[2] + 1)..indicesOfWhitespaces[3]][1..];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ChatMessageTags GetIsAction(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        if (indicesOfWhitespaces.Length < 5)
        {
            return 0;
        }

        ReadOnlySpan<char> actionPrefix = ircMessage[(indicesOfWhitespaces[3] + 1)..indicesOfWhitespaces[4]];
        bool isAction = actionPrefix.SequenceEqual(_actionPrefix);
        int asByte = Unsafe.As<bool, byte>(ref isAction);
        return (ChatMessageTags)(asByte << 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ReadOnlySpan<char> GetUsername(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, int displayNameLength)
    {
        Debug.Assert(displayNameLength != 0);
        ReadOnlySpan<char> username = ircMessage[(indicesOfWhitespaces[0] + 2)..][..displayNameLength];
        return username;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ReadOnlySpan<char> GetMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, bool isAction)
    {
        if (!isAction)
        {
            Debug.Assert(!ircMessage.Contains(" :\u0001ACTION", StringComparison.Ordinal));
            return ircMessage[(indicesOfWhitespaces[3] + 2)..];
        }

        // skipping chars to speed up .IndexOf
        const int maximumAmountOfCharsThatCanBeIgnored = 242;
        ircMessage = ircMessage[maximumAmountOfCharsThatCanBeIgnored..];
        return ircMessage[(ircMessage.IndexOf('\u0001') + 8)..^1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static Badge[] GetBadges(ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            return [];
        }

        // TODO: IndicesOf
        Badge[] badges = new Badge[value.Count(',') + 1];
        int badgeCount = 0;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static Color GetColor(ReadOnlySpan<char> value)
        => value.Length == 0 ? Color.Empty : ParseHexColor(ref Unsafe.Add(ref MemoryMarshal.GetReference(value), 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ReadOnlySpan<char> GetDisplayName(ReadOnlySpan<char> value)
    {
        bool isBackSlash = value[^2] == '\\';
        int asByte = Unsafe.As<bool, byte>(ref isBackSlash) << 1;
        ReadOnlySpan<char> displayName = value[..^asByte];
        Debug.Assert(displayName.Length != 0);
        return displayName;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ChatMessageTags GetIsFirstMsg(ReadOnlySpan<char> value)
    {
        bool isFirstMessage = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isFirstMessage);
        return (ChatMessageTags)asByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static Guid GetId(ReadOnlySpan<char> value) => Guid.ParseExact(value, "D");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ChatMessageTags GetIsModerator(ReadOnlySpan<char> value)
    {
        bool isModerator = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isModerator);
        return (ChatMessageTags)(asByte << 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static long GetChannelId(ReadOnlySpan<char> value) => ParseInt64(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ChatMessageTags GetIsSubscriber(ReadOnlySpan<char> value)
    {
        bool isSubscriber = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isSubscriber);
        return (ChatMessageTags)(asByte << 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static long GetTmiSentTs(ReadOnlySpan<char> value) => ParseInt64(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static ChatMessageTags GetIsTurboUser(ReadOnlySpan<char> value)
    {
        bool isTurboUser = value[0] == '1';
        int asByte = Unsafe.As<bool, byte>(ref isTurboUser);
        return (ChatMessageTags)(asByte << 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected static long GetUserId(ReadOnlySpan<char> value) => ParseInt64(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long ParseInt64(ReadOnlySpan<char> value)
    {
        int length = value.Length;
        ref char charsReference = ref MemoryMarshal.GetReference(value);
        long result = 0;
        for (int i = 0; i < length; i++)
        {
            result = 10 * result + Unsafe.Add(ref charsReference, i) - '0';
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color ParseHexColor(ref char numberReference)
    {
        const int charAAndZeroDiff = _upperCaseAMinus10 - _charZero;

        char firstChar = Unsafe.Add(ref numberReference, 0);
        char thirdChar = Unsafe.Add(ref numberReference, 2);
        char fifthChar = Unsafe.Add(ref numberReference, 4);

        bool isFirstCharHexLetter = IsHexLetter(firstChar);
        bool isThirdCharHexLetter = IsHexLetter(thirdChar);
        bool isFifthCharHexLetter = IsHexLetter(fifthChar);

        int red = firstChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isFirstCharHexLetter)));
        int green = thirdChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isThirdCharHexLetter)));
        int blue = fifthChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isFifthCharHexLetter)));

        red <<= 4;
        green <<= 4;
        blue <<= 4;

        char secondChar = Unsafe.Add(ref numberReference, 1);
        char forthChar = Unsafe.Add(ref numberReference, 3);
        char sixthChar = Unsafe.Add(ref numberReference, 5);

        bool isSecondCharHexLetter = IsHexLetter(secondChar);
        bool isForthCharHexLetter = IsHexLetter(forthChar);
        bool isSixthCharHexLetter = IsHexLetter(sixthChar);

        red |= secondChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isSecondCharHexLetter)));
        green |= forthChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isForthCharHexLetter)));
        blue |= sixthChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isSixthCharHexLetter)));

        return new((byte)red, (byte)green, (byte)blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHexLetter(char hexChar) => hexChar > '9';

    [Pure]
    public bool Equals(ChatMessageParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ChatMessageParser? left, ChatMessageParser? right) => Equals(left, right);

    public static bool operator !=(ChatMessageParser? left, ChatMessageParser? right) => !(left == right);
}
