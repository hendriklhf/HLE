using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Strings;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a chat message.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class ChatMessage : IEquatable<ChatMessage>
{
    /// <summary>
    /// Holds information about a badge, that can be obtained by its name found in <see cref="Badges"/>.
    /// </summary>
    public ReadOnlySpan<Badge> BadgeInfos => _badgeInfo;

    /// <summary>
    /// Holds all the badges the user has.
    /// </summary>
    public ReadOnlySpan<Badge> Badges => _badges;

    /// <summary>
    /// The color of the user's name in a Twitch chat overlay.
    /// If the user does not have a color, the value is "Color.Empty".
    /// </summary>
    public Color Color { get; }

    /// <summary>
    /// The display name of the user with the preferred casing.
    /// </summary>
    public string DisplayName { get; } = string.Empty;

    /// <summary>
    /// Indicates whether the message is the first message the user has sent in the channel or not.
    /// </summary>
    public bool IsFirstMessage => (_flags & ChatMessageFlag.IsFirstMessage) == ChatMessageFlag.IsFirstMessage;

    /// <summary>
    /// The unique message id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Indicates whether the user is a moderator or not.
    /// </summary>
    public bool IsModerator => (_flags & ChatMessageFlag.IsModerator) == ChatMessageFlag.IsModerator;

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    public long ChannelId { get; }

    /// <summary>
    /// Indicates whether the user is a subscriber or not.
    /// The subscription age can be obtained from <see cref="Badges"/> and <see cref="BadgeInfos"/>.
    /// </summary>
    public bool IsSubscriber => (_flags & ChatMessageFlag.IsSubscriber) == ChatMessageFlag.IsSubscriber;

    /// <summary>
    /// The unix timestamp in milliseconds of the moment the message has been sent.
    /// </summary>
    public long TmiSentTs { get; }

    /// <summary>
    /// Indicates whether the user is subscribing to Twitch Turbo or not.
    /// </summary>
    public bool IsTurboUser => (_flags & ChatMessageFlag.IsTurboUser) == ChatMessageFlag.IsTurboUser;

    /// <summary>
    /// The user id of the user who sent the message.
    /// </summary>
    public long UserId { get; }

    /// <summary>
    /// Indicates whether the message was sent as an action (prefixed with "/me") or not.
    /// </summary>
    public bool IsAction => (_flags & ChatMessageFlag.IsAction) == ChatMessageFlag.IsAction;

    /// <summary>
    /// The username of the user who sent the message. All lower case.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The username of the channel owner. All lower case, without '#'.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The message content.
    /// </summary>
    public string Message { get; }

    private readonly Badge[]? _badgeInfo;
    private readonly Badge[]? _badges;
    private readonly ChatMessageFlag _flags = 0;

    private const string _actionPrefix = ":\u0001ACTION";
    private const string _nameWithSpaceEnding = "\\s";

    private const char _upperCaseAMinus10 = (char)('A' - 10);
    private const char _charZero = '0';

    private const string _badgeInfoTag = "badge-info";
    private const string _badgesTag = "badges";
    private const string _colorTag = "color";
    private const string _displayNameTag = "display-name";
    private const string _firstMsgTag = "first-msg";
    private const string _idTag = "id";
    private const string _modTag = "mod";
    private const string _roomIdTag = "room-id";
    private const string _subscriberTag = "subscriber";
    private const string _tmiSentTsTag = "tmi-sent-ts";
    private const string _turboTag = "turbo";
    private const string _userIdTag = "user-id";

    /// <summary>
    /// The default constructor of <see cref="ChatMessage"/>. This will parse the given IRC message.
    /// </summary>
    /// <param name="ircMessage">The IRC message as a <see cref="ReadOnlySpan{Char}"/>.</param>
    /// <param name="indicesOfWhitespace">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public ChatMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespace)
    {
        ReadOnlySpan<char> tags = ircMessage[1..indicesOfWhitespace[0]];

        int semicolonIndex = tags.IndexOf(';');
        while (semicolonIndex != -1)
        {
            ReadOnlySpan<char> tag = tags[..semicolonIndex];
            tags = tags[(semicolonIndex + 1)..];
            semicolonIndex = tags.IndexOf(';');

            int equalsSignIndex = tag.IndexOf('=');
            ReadOnlySpan<char> key = tag[..equalsSignIndex];
            ReadOnlySpan<char> value = tag[(equalsSignIndex + 1)..];
            switch (key)
            {
                case _badgeInfoTag:
                    _badgeInfo = GetBadges(value);
                    break;
                case _badgesTag:
                    _badges = GetBadges(value);
                    break;
                case _colorTag:
                    Color = GetColor(value);
                    break;
                case _displayNameTag:
                    DisplayName = GetDisplayName(value);
                    break;
                case _firstMsgTag:
                    _flags |= GetIsFirstMsg(value);
                    break;
                case _idTag:
                    Id = GetId(value);
                    break;
                case _modTag:
                    _flags |= GetIsModerator(value);
                    break;
                case _roomIdTag:
                    ChannelId = GetChannelId(value);
                    break;
                case _subscriberTag:
                    _flags |= GetIsSubscriber(value);
                    break;
                case _tmiSentTsTag:
                    TmiSentTs = GetTmiSentTs(value);
                    break;
                case _turboTag:
                    _flags |= GetIsTurboUser(value);
                    break;
                case _userIdTag:
                    UserId = GetUserId(value);
                    break;
            }
        }

        _flags |= GetIsAction(ircMessage, indicesOfWhitespace);
        Username = GetUsername(ircMessage, indicesOfWhitespace);
        Channel = StringPool.Shared.GetOrAdd(ircMessage[(indicesOfWhitespace[2] + 1)..indicesOfWhitespace[3]][1..]);
        Message = GetMessage(ircMessage, indicesOfWhitespace);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsAction(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespace)
    {
        if (indicesOfWhitespace.Length < 5)
        {
            return 0;
        }

        ReadOnlySpan<char> actionPrefix = ircMessage[(indicesOfWhitespace[3] + 1)..indicesOfWhitespace[4]];
        bool isAction = actionPrefix.Equals(_actionPrefix, StringComparison.Ordinal);
        byte asByte = Unsafe.As<bool, byte>(ref isAction);
        return (ChatMessageFlag)(asByte << 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetUsername(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        Debug.Assert(DisplayName is not null && DisplayName.Length > 0);
        ReadOnlySpan<char> username = ircMessage[(indicesOfWhitespaces[0] + 2)..][..DisplayName.Length];
        return StringPool.Shared.GetOrAdd(username);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        if (IsAction)
        {
            // skipping chars to speed up .IndexOf
            const int maximumOfCharsThatCanBeIgnored = 242;
            ircMessage = ircMessage[maximumOfCharsThatCanBeIgnored..];
            return new(ircMessage[(ircMessage.IndexOf('\u0001') + 8)..^1]);
        }

        Debug.Assert(!ircMessage.Contains(" :\u0001ACTION", StringComparison.Ordinal));
        return new(ircMessage[(indicesOfWhitespaces[3] + 2)..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Badge[] GetBadges(ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            return Array.Empty<Badge>();
        }

        Badge[] result = new Badge[value.CharCount(',') + 1];
        ref Badge firstBadge = ref MemoryMarshal.GetArrayDataReference(result);
        int badgeCount = 0;
        while (value.Length > 0)
        {
            int indexOfComma = value.IndexOf(',');
            ReadOnlySpan<char> info = value[..Unsafe.As<int, Index>(ref indexOfComma)];
            value = indexOfComma == -1 ? ReadOnlySpan<char>.Empty : value[(indexOfComma + 1)..];
            int slashIndex = info.IndexOf('/');
            string name = StringPool.Shared.GetOrAdd(info[..slashIndex]);
            string level = StringPool.Shared.GetOrAdd(info[(slashIndex + 1)..]);
            Unsafe.Add(ref firstBadge, badgeCount++) = new(name, level);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color GetColor(ReadOnlySpan<char> value)
    {
        return value.Length == 0 ? Color.Empty : ParseHexColor(ref MemoryMarshal.GetReference(value[1..]));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetDisplayName(ReadOnlySpan<char> value)
    {
        bool isBackSlash = value[^2] == _nameWithSpaceEnding[0];
        byte asByte = (byte)(Unsafe.As<bool, byte>(ref isBackSlash) << 1);
        return StringPool.Shared.GetOrAdd(value[..^asByte]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsFirstMsg(ReadOnlySpan<char> value)
    {
        bool isFirstMessage = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isFirstMessage);
        return (ChatMessageFlag)asByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid GetId(ReadOnlySpan<char> value) => Guid.ParseExact(value, "D");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsModerator(ReadOnlySpan<char> value)
    {
        bool isModerator = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isModerator);
        return (ChatMessageFlag)(asByte << 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetChannelId(ReadOnlySpan<char> value) => NumberHelper.ParsePositiveInt64(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsSubscriber(ReadOnlySpan<char> value)
    {
        bool isSubscriber = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isSubscriber);
        return (ChatMessageFlag)(asByte << 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetTmiSentTs(ReadOnlySpan<char> value) => NumberHelper.ParsePositiveInt64(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsTurboUser(ReadOnlySpan<char> value)
    {
        bool isTurboUser = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isTurboUser);
        return (ChatMessageFlag)(asByte << 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetUserId(ReadOnlySpan<char> value) => NumberHelper.ParsePositiveInt64(value);

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Color ParseHexColor(ref char number)
    {
        const byte charAAndZeroDiff = _upperCaseAMinus10 - _charZero;

        char firstChar = Unsafe.Add(ref number, 0);
        char thirdChar = Unsafe.Add(ref number, 2);
        char fifthChar = Unsafe.Add(ref number, 4);

        bool isFirstCharHexLetter = IsHexLetter(firstChar);
        bool isThirdCharHexLetter = IsHexLetter(thirdChar);
        bool isFifthCharHexLetter = IsHexLetter(fifthChar);

        byte red = (byte)(firstChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isFirstCharHexLetter))));
        byte green = (byte)(thirdChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isThirdCharHexLetter))));
        byte blue = (byte)(fifthChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isFifthCharHexLetter))));

        red <<= 4;
        green <<= 4;
        blue <<= 4;

        char secondChar = Unsafe.Add(ref number, 1);
        char forthChar = Unsafe.Add(ref number, 3);
        char sixthChar = Unsafe.Add(ref number, 5);

        bool isSecondCharHexLetter = IsHexLetter(secondChar);
        bool isForthCharHexLetter = IsHexLetter(forthChar);
        bool isSixthCharHexLetter = IsHexLetter(sixthChar);

        red |= (byte)(secondChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isSecondCharHexLetter))));
        green |= (byte)(forthChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isForthCharHexLetter))));
        blue |= (byte)(sixthChar - (_charZero + (charAAndZeroDiff * Unsafe.As<bool, byte>(ref isSixthCharHexLetter))));

        return new(red, green, blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHexLetter(char hexChar)
    {
        return hexChar > '9';
    }

    /// <summary>
    /// Returns the message in the following format: "&lt;#Channel&gt; Username: Message".
    /// </summary>
    /// <returns>The message in a readable format.</returns>
    public override string ToString()
    {
        ValueStringBuilder builder = stackalloc char[Channel.Length + Username.Length + Message.Length + 6];
        builder.Append("<#", Channel, "> ", Username, ": ", Message);
        return builder.ToString();
    }

    public bool Equals(ChatMessage? other)
    {
        return ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ChatMessage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, TmiSentTs);
    }
}
