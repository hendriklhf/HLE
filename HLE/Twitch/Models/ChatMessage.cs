using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    public bool IsFirstMessage => (_flags & ChatMessageFlags.IsFirstMessage) == ChatMessageFlags.IsFirstMessage;

    /// <summary>
    /// The unique message id.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Indicates whether the user is a moderator or not.
    /// </summary>
    public bool IsModerator => (_flags & ChatMessageFlags.IsModerator) == ChatMessageFlags.IsModerator;

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    public long ChannelId { get; }

    /// <summary>
    /// Indicates whether the user is a subscriber or not.
    /// The subscription age can be obtained from <see cref="Badges"/> and <see cref="BadgeInfo"/>.
    /// </summary>
    public bool IsSubscriber => (_flags & ChatMessageFlags.IsSubscriber) == ChatMessageFlags.IsSubscriber;

    /// <summary>
    /// The unix timestamp in milliseconds of the moment the message has been sent.
    /// </summary>
    public long TmiSentTs { get; }

    /// <summary>
    /// Indicates whether the user is subscribing to Twitch Turbo or not.
    /// </summary>
    public bool IsTurboUser => (_flags & ChatMessageFlags.IsTurboUser) == ChatMessageFlags.IsTurboUser;

    /// <summary>
    /// The user id of the user who sent the message.
    /// </summary>
    public long UserId { get; }

    /// <summary>
    /// Indicates whether the message was sent as an action (prefixed with "/me") or not.
    /// </summary>
    public bool IsAction => (_flags & ChatMessageFlags.IsAction) == ChatMessageFlags.IsAction;

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
    private readonly ChatMessageFlags _flags = 0;

    private const string _actionPrefix = ":\u0001ACTION";
    private const string _nameWithSpaceEnding = "\\s";
    private const string _guidFormat = "D";

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
    /// <param name="ircRanges">The ranges of the split of <paramref name="ircMessage"/> on whitespaces.</param>
    public ChatMessage(ReadOnlySpan<char> ircMessage, Span<Range> ircRanges)
    {
        ReadOnlySpan<char> tags = ircMessage[ircRanges[0]][1..];

        int semicolonIndex = tags.IndexOf(';');
        while (semicolonIndex != -1)
        {
            ReadOnlySpan<char> tag = tags[..semicolonIndex];
            tags = tags[(semicolonIndex + 1)..];
            semicolonIndex = tags.IndexOf(';');

            int equalSignIndex = tag.IndexOf('=');
            ReadOnlySpan<char> key = tag[..equalSignIndex];
            ReadOnlySpan<char> value = tag[(equalSignIndex + 1)..];
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

        _flags |= GetIsAction(ircMessage, ircRanges);
        Username = DisplayName.ToLowerInvariant();
        Channel = new(ircMessage[ircRanges[3]][1..]);
        Message = GetMessage(ircMessage, ircRanges);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlags GetIsAction(ReadOnlySpan<char> ircMessage, Span<Range> ircRanges)
    {
        bool isAction = ircMessage[ircRanges[4]].Equals(_actionPrefix, StringComparison.Ordinal);
        byte asByte = Unsafe.As<bool, byte>(ref isAction);
        return (ChatMessageFlags)(asByte << 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<Range> ircRanges)
    {
        return new(IsAction ? ircMessage[(ircMessage.IndexOf('\u0001') + 8)..^1] : ircMessage[(ircRanges[3].End.Value + 2)..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Badge[] GetBadges(ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            return Array.Empty<Badge>();
        }

        Span<Range> ranges = stackalloc Range[value.Length];
        int rangesLength = value.GetRangesOfSplit(',', ranges);
        Badge[] result = new Badge[rangesLength];
        ref Badge firstBadge = ref MemoryMarshal.GetArrayDataReference(result);
        for (int i = 0; i < rangesLength; i++)
        {
            ReadOnlySpan<char> info = value[ranges[i]];
            int slashIndex = info.IndexOf('/');
            string name = new(info[..slashIndex]);
            string level = new(info[(slashIndex + 1)..]);
            Unsafe.Add(ref firstBadge, i) = new(name, level);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Color GetColor(ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            return Color.Empty;
        }

        byte r = byte.Parse(value[1..3], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte g = byte.Parse(value[3..5], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        byte b = byte.Parse(value[5..7], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Color.FromArgb(r, g, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetDisplayName(ReadOnlySpan<char> value)
    {
        bool isBackSlash = value[^2] == _nameWithSpaceEnding[0];
        bool isLetterS = value[^1] == _nameWithSpaceEnding[1];
        byte asByte = (byte)(Unsafe.As<bool, byte>(ref isBackSlash) + Unsafe.As<bool, byte>(ref isLetterS));
        return new(value[..^asByte]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlags GetIsFirstMsg(ReadOnlySpan<char> value)
    {
        bool isFirstMessage = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isFirstMessage);
        return (ChatMessageFlags)asByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid GetId(ReadOnlySpan<char> value) => Guid.ParseExact(value, _guidFormat);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlags GetIsModerator(ReadOnlySpan<char> value)
    {
        bool isModerator = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isModerator);
        return (ChatMessageFlags)(asByte << 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetChannelId(ReadOnlySpan<char> value) => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlags GetIsSubscriber(ReadOnlySpan<char> value)
    {
        bool isSubscriber = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isSubscriber);
        return (ChatMessageFlags)(asByte << 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetTmiSentTs(ReadOnlySpan<char> value) => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChatMessageFlags GetIsTurboUser(ReadOnlySpan<char> value)
    {
        bool isTurboUser = value[0] == '1';
        byte asByte = Unsafe.As<bool, byte>(ref isTurboUser);
        return (ChatMessageFlags)(asByte << 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetUserId(ReadOnlySpan<char> value) => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns the message in the following format: "&lt;#Channel&gt; Username: Message".
    /// </summary>
    /// <returns>The message in a readable format.</returns>
    public override string ToString()
    {
        return $"<#{Channel}> {Username}: {Message}";
    }
}
