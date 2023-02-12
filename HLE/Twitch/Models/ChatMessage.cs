using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a chat message.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class ChatMessage
{
    /// <summary>
    /// Holds information about a badge, that can be obtained by its name found in <see cref="Badges"/>.
    /// </summary>
    public ReadOnlySpan<Badge> BadgeInfo => _badgeInfo;

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
    /// The subscription age can be obtained from <see cref="Badges"/> and <see cref="BadgeInfo"/>.
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

    private const byte _semicolon = (byte)';';
    private const byte _equalsSign = (byte)'=';
    private const byte _charOne = (byte)'1';
    private const byte _charNine = (byte)'9';
    private const byte _charAMinus10 = 'A' - 10;
    private const byte _charZero = (byte)'0';
    private const byte _comma = (byte)',';
    private const byte _slash = (byte)'/';
    private const string _nameWithSpaceEnding = "\\s";
    private const string _guidFormat = "D";

    private static readonly byte[] _actionPrefix = ":\u0001ACTION"u8.ToArray();

    private static readonly byte[] _badgeInfoTag = "badge-info"u8.ToArray();
    private static readonly byte[] _badgesTag = "badges"u8.ToArray();
    private static readonly byte[] _colorTag = "color"u8.ToArray();
    private static readonly byte[] _displayNameTag = "display-name"u8.ToArray();
    private static readonly byte[] _firstMsgTag = "first-msg"u8.ToArray();
    private static readonly byte[] _idTag = "id"u8.ToArray();
    private static readonly byte[] _modTag = "mod"u8.ToArray();
    private static readonly byte[] _roomIdTag = "room-id"u8.ToArray();
    private static readonly byte[] _subscriberTag = "subscriber"u8.ToArray();
    private static readonly byte[] _tmiSentTsTag = "tmi-sent-ts"u8.ToArray();
    private static readonly byte[] _turboTag = "turbo"u8.ToArray();
    private static readonly byte[] _userIdTag = "user-id"u8.ToArray();

    /// <summary>
    /// The default constructor of <see cref="ChatMessage"/>. This will parse the given IRC message.
    /// </summary>
    /// <param name="ircMessage">The IRC message as a <see cref="ReadOnlySpan{Char}"/>.</param>
    /// <param name="indicesOfWhitespace">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public ChatMessage(ReadOnlySpan<byte> ircMessage, Span<int> indicesOfWhitespace)
    {
        ReadOnlySpan<byte> tags = ircMessage[1..indicesOfWhitespace[0]];

        int semicolonIndex = tags.IndexOf(_semicolon);
        while (semicolonIndex != -1)
        {
            ReadOnlySpan<byte> tag = tags[..semicolonIndex];
            tags = tags[(semicolonIndex + 1)..];
            semicolonIndex = tags.IndexOf(_semicolon);

            int equalsSignIndex = tag.IndexOf(_equalsSign);
            ReadOnlySpan<byte> key = tag[..equalsSignIndex];
            ReadOnlySpan<byte> value = tag[(equalsSignIndex + 1)..];
            if (key.SequenceEqual(_badgeInfoTag))
            {
                _badgeInfo = GetBadges(value);
            }
            else if (key.SequenceEqual(_badgesTag))
            {
                _badges = GetBadges(value);
            }
            else if (key.SequenceEqual(_colorTag))
            {
                Color = GetColor(value);
            }
            else if (key.SequenceEqual(_displayNameTag))
            {
                DisplayName = GetDisplayName(value);
            }
            else if (key.SequenceEqual(_firstMsgTag))
            {
                _flags |= GetIsFirstMsg(value);
            }
            else if (key.SequenceEqual(_idTag))
            {
                Id = GetId(value);
            }
            else if (key.SequenceEqual(_modTag))
            {
                _flags |= GetIsModerator(value);
            }
            else if (key.SequenceEqual(_roomIdTag))
            {
                ChannelId = GetChannelId(value);
            }
            else if (key.SequenceEqual(_subscriberTag))
            {
                _flags |= GetIsSubscriber(value);
            }
            else if (key.SequenceEqual(_tmiSentTsTag))
            {
                TmiSentTs = GetTmiSentTs(value);
            }
            else if (key.SequenceEqual(_turboTag))
            {
                _flags |= GetIsTurboUser(value);
            }
            else if (key.SequenceEqual(_userIdTag))
            {
                UserId = GetUserId(value);
            }
        }

        _flags |= GetIsAction(ircMessage, indicesOfWhitespace);
        Username = DisplayName.ToLowerInvariant();
        Channel = Encoding.UTF8.GetString(ircMessage[(indicesOfWhitespace[2] + 1)..indicesOfWhitespace[3]][1..]);
        Message = GetMessage(ircMessage, indicesOfWhitespace);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsAction(ReadOnlySpan<byte> ircMessage, Span<int> indicesOfWhitespace)
    {
        if (indicesOfWhitespace.Length < 5)
        {
            return 0;
        }

        ReadOnlySpan<byte> actionPrefix = ircMessage[(indicesOfWhitespace[3] + 1)..indicesOfWhitespace[4]];
        bool isAction = actionPrefix.SequenceEqual(_actionPrefix);
        byte asByte = Unsafe.As<bool, byte>(ref isAction);
        return (ChatMessageFlag)(asByte << 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetMessage(ReadOnlySpan<byte> ircMessage, Span<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> message = IsAction ? ircMessage[(ircMessage.IndexOf((byte)'\u0001') + 8)..^1] : ircMessage[(indicesOfWhitespaces[3] + 2)..];
        return Encoding.UTF8.GetString(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Badge[] GetBadges(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0)
        {
            return Array.Empty<Badge>();
        }

        Badge[] result = new Badge[value.ByteCount(_comma) + 1];
        ref Badge firstBadge = ref MemoryMarshal.GetArrayDataReference(result);
        int badgeCount = 0;
        while (value.Length > 0)
        {
            int indexOfComma = value.IndexOf(_comma);
            ReadOnlySpan<byte> info = value[..Unsafe.As<int, Index>(ref indexOfComma)];
            value = indexOfComma == -1 ? value[value.Length..] : value[(indexOfComma + 1)..];
            int slashIndex = info.IndexOf(_slash);
            string name = Encoding.UTF8.GetString(info[..slashIndex]);
            string level = Encoding.UTF8.GetString(info[(slashIndex + 1)..]);
            Unsafe.Add(ref firstBadge, badgeCount++) = new(name, level);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color GetColor(ReadOnlySpan<byte> value)
    {
        return value.Length == 0 ? Color.Empty : ParseHexColorFromUtf8Bytes(value[1..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetDisplayName(ReadOnlySpan<byte> value)
    {
        bool isBackSlash = value[^2] == _nameWithSpaceEnding[0];
        bool isLetterS = value[^1] == _nameWithSpaceEnding[1];
        byte asByte = (byte)(Unsafe.As<bool, byte>(ref isBackSlash) + Unsafe.As<bool, byte>(ref isLetterS));
        ReadOnlySpan<byte> displayName = value[..^asByte];
        return Encoding.UTF8.GetString(displayName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsFirstMsg(ReadOnlySpan<byte> value)
    {
        bool isFirstMessage = value[0] == _charOne;
        byte asByte = Unsafe.As<bool, byte>(ref isFirstMessage);
        return (ChatMessageFlag)asByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid GetId(ReadOnlySpan<byte> value)
    {
        Span<char> chars = stackalloc char[36];
        Encoding.UTF8.GetChars(value, chars);
        return Guid.ParseExact(chars, _guidFormat);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsModerator(ReadOnlySpan<byte> value)
    {
        bool isModerator = value[0] == _charOne;
        byte asByte = Unsafe.As<bool, byte>(ref isModerator);
        return (ChatMessageFlag)(asByte << 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetChannelId(ReadOnlySpan<byte> value) => NumberHelper.ParsePositiveInt64FromUtf8Bytes(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsSubscriber(ReadOnlySpan<byte> value)
    {
        bool isSubscriber = value[0] == _charOne;
        byte asByte = Unsafe.As<bool, byte>(ref isSubscriber);
        return (ChatMessageFlag)(asByte << 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetTmiSentTs(ReadOnlySpan<byte> value) => NumberHelper.ParsePositiveInt64FromUtf8Bytes(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlag GetIsTurboUser(ReadOnlySpan<byte> value)
    {
        bool isTurboUser = value[0] == _charOne;
        byte asByte = Unsafe.As<bool, byte>(ref isTurboUser);
        return (ChatMessageFlag)(asByte << 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetUserId(ReadOnlySpan<byte> value) => NumberHelper.ParsePositiveInt64FromUtf8Bytes(value);

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static Color ParseHexColorFromUtf8Bytes(ReadOnlySpan<byte> number)
    {
        byte firstChar = number[0];
        byte thirdChar = number[2];
        byte fifthChar = number[4];

        byte red = (byte)(IsLetter(firstChar) ? firstChar - _charAMinus10 : firstChar - _charZero);
        byte green = (byte)(IsLetter(thirdChar) ? thirdChar - _charAMinus10 : thirdChar - _charZero);
        byte blue = (byte)(IsLetter(fifthChar) ? fifthChar - _charAMinus10 : fifthChar - _charZero);

        red <<= 4;
        green <<= 4;
        blue <<= 4;

        byte secondChar = number[1];
        byte forthChar = number[3];
        byte sixthChar = number[5];

        red |= (byte)(IsLetter(secondChar) ? secondChar - _charAMinus10 : secondChar - _charZero);
        green |= (byte)(IsLetter(forthChar) ? forthChar - _charAMinus10 : forthChar - _charZero);
        blue |= (byte)(IsLetter(sixthChar) ? sixthChar - _charAMinus10 : sixthChar - _charZero);

        return Color.FromArgb(red, green, blue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsLetter(byte hexChar)
    {
        return hexChar > _charNine;
    }

    /// <summary>
    /// Returns the message in the following format: "&lt;#Channel&gt; Username: Message".
    /// </summary>
    /// <returns>The message in a readable format.</returns>
    public override string ToString()
    {
        return $"<#{Channel}> {Username}: {Message}";
    }
}
