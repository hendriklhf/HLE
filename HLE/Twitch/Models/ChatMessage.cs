using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    //private const char _lowerCaseAMinus10 = (char)('a' - 10);
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
            if (key.Equals(_badgeInfoTag, StringComparison.Ordinal))
            {
                _badgeInfo = GetBadges(value);
            }
            else if (key.Equals(_badgesTag, StringComparison.Ordinal))
            {
                _badges = GetBadges(value);
            }
            else if (key.Equals(_colorTag, StringComparison.Ordinal))
            {
                Color = GetColor(value);
            }
            else if (key.Equals(_displayNameTag, StringComparison.Ordinal))
            {
                DisplayName = GetDisplayName(value);
            }
            else if (key.Equals(_firstMsgTag, StringComparison.Ordinal))
            {
                _flags |= GetIsFirstMsg(value);
            }
            else if (key.Equals(_idTag, StringComparison.Ordinal))
            {
                Id = GetId(value);
            }
            else if (key.Equals(_modTag, StringComparison.Ordinal))
            {
                _flags |= GetIsModerator(value);
            }
            else if (key.Equals(_roomIdTag, StringComparison.Ordinal))
            {
                ChannelId = GetChannelId(value);
            }
            else if (key.Equals(_subscriberTag, StringComparison.Ordinal))
            {
                _flags |= GetIsSubscriber(value);
            }
            else if (key.Equals(_tmiSentTsTag, StringComparison.Ordinal))
            {
                TmiSentTs = GetTmiSentTs(value);
            }
            else if (key.Equals(_turboTag, StringComparison.Ordinal))
            {
                _flags |= GetIsTurboUser(value);
            }
            else if (key.Equals(_userIdTag, StringComparison.Ordinal))
            {
                UserId = GetUserId(value);
            }
        }

        _flags |= GetIsAction(ircMessage, indicesOfWhitespace);
        Username = DisplayName.ToLowerInvariant();
        Channel = new(ircMessage[(indicesOfWhitespace[2] + 1)..indicesOfWhitespace[3]][1..]);
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
    private string GetMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> message = IsAction ? ircMessage[(ircMessage.IndexOf('\u0001') + 8)..^1] : ircMessage[(indicesOfWhitespaces[3] + 2)..];
        return new(message);
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
            value = indexOfComma == -1 ? value[value.Length..] : value[(indexOfComma + 1)..];
            int slashIndex = info.IndexOf('/');
            string name = new(info[..slashIndex]);
            string level = new(info[(slashIndex + 1)..]);
            Unsafe.Add(ref firstBadge, badgeCount++) = new(name, level);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color GetColor(ReadOnlySpan<char> value)
    {
        return value.Length == 0 ? Color.Empty : ParseHexColor(value[1..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetDisplayName(ReadOnlySpan<char> value)
    {
        bool isBackSlash = value[^2] == _nameWithSpaceEnding[0];
        byte asByte = (byte)(Unsafe.As<bool, byte>(ref isBackSlash) << 1);
        return new(value[..^asByte]);
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
    private static Color ParseHexColor(ReadOnlySpan<char> number)
    {
        char firstChar = number[0];
        char thirdChar = number[2];
        char fifthChar = number[4];

        byte red = (byte)(IsHexLetter(firstChar) ? firstChar - _upperCaseAMinus10 : firstChar - _charZero);
        byte green = (byte)(IsHexLetter(thirdChar) ? thirdChar - _upperCaseAMinus10 : thirdChar - _charZero);
        byte blue = (byte)(IsHexLetter(fifthChar) ? fifthChar - _upperCaseAMinus10 : fifthChar - _charZero);

        red <<= 4;
        green <<= 4;
        blue <<= 4;

        char secondChar = number[1];
        char forthChar = number[3];
        char sixthChar = number[5];

        red |= (byte)(IsHexLetter(secondChar) ? secondChar - _upperCaseAMinus10 : secondChar - _charZero);
        green |= (byte)(IsHexLetter(forthChar) ? forthChar - _upperCaseAMinus10 : forthChar - _charZero);
        blue |= (byte)(IsHexLetter(sixthChar) ? sixthChar - _upperCaseAMinus10 : sixthChar - _charZero);

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
        return $"<#{Channel}> {Username}: {Message}";
    }

    public bool Equals(ChatMessage? other)
    {
        return ReferenceEquals(this, other) || (Id == other?.Id && TmiSentTs == other.TmiSentTs);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is ChatMessage other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, TmiSentTs);
    }
}
