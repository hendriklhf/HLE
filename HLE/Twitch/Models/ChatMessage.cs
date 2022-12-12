using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a chat message.
/// </summary>
[DebuggerDisplay("<#{Channel}> {Username}: {Message}")]
[SuppressMessage("Reliability", "CA2014:Do not use stackalloc in loops")]
public sealed class ChatMessage
{
    /// <summary>
    /// Holds information about a badge, that can be obtained by its name found in <see cref="Badges"/>.
    /// </summary>
    public ReadOnlyDictionary<string, string> BadgeInfo { get; init; } = _emptyDictionary;

    /// <summary>
    /// Holds all the badges the user has.
    /// </summary>
    public Badge[] Badges { get; init; } = Array.Empty<Badge>();

    /// <summary>
    /// The color of the user's name in a Twitch chat overlay.
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    /// The display name of the user with the preferred casing.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the message is the first message the user has sent in this channel or not.
    /// </summary>
    public bool IsFirstMessage { get; init; }

    /// <summary>
    /// The unique message id.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Indicates whether the user is a moderator or not.
    /// </summary>
    public bool IsModerator { get; init; }

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    public long ChannelId { get; init; }

    /// <summary>
    /// Indicates whether the user is a subscriber or not.
    /// The subscription age can be obtained from <see cref="Badges"/> and <see cref="BadgeInfo"/>.
    /// </summary>
    public bool IsSubscriber { get; init; }

    /// <summary>
    /// The unix timestamp in milliseconds of the moment the message has been sent.
    /// </summary>
    public long TmiSentTs { get; init; }

    /// <summary>
    /// Indicates whether the user is subscribing to Twitch Turbo or not.
    /// </summary>
    public bool IsTurboUser { get; init; }

    /// <summary>
    /// The user id of the user who sent the message.
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// Indicates whether the message was sent as an action (prefixed with "/me") or not.
    /// </summary>
    public bool IsAction { get; init; }

    /// <summary>
    /// The username of the user who sent the message. All lower case.
    /// </summary>
    public string Username { get; init; }

    /// <summary>
    /// The username of the channel owner. All lower case.
    /// </summary>
    public string Channel { get; init; }

    /// <summary>
    /// The message content.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// The raw IRC message.
    /// </summary>
    public string RawIrcMessage { get; init; }

    private static readonly ReadOnlyDictionary<string, string> _emptyDictionary = new(new Dictionary<string, string>());

    private const string _actionPrefix = ":\u0001ACTION";
    private const string _nameWithSpaceEnding = "\\s";

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
    /// <param name="rawIrcMessage">The IRC message as a <see cref="String"/>. If passed, avoids the conversion of <paramref name="ircMessage"/> to a <see cref="String"/>, and therefore a <see cref="String"/> allocation.</param>
    public ChatMessage(ReadOnlySpan<char> ircMessage, string? rawIrcMessage = null)
    {
        Span<Range> ircRanges = stackalloc Range[ircMessage.Length];
        int ircRangesLength = ircMessage.GetRangesOfSplit(' ', ircRanges);
        ircRanges = ircRanges[..ircRangesLength];

        ReadOnlySpan<char> tags = ircMessage[ircRanges[0]][1..];
        Span<Range> tagsRanges = stackalloc Range[tags.Length];
        int tagsRangesLength = tags.GetRangesOfSplit(';', tagsRanges);

        for (int i = 0; i < tagsRangesLength; i++)
        {
            ReadOnlySpan<char> tag = tags[tagsRanges[i]];
            // ReSharper disable once StackAllocInsideLoop
            Span<Range> tagRanges = stackalloc Range[2];
            tag.GetRangesOfSplit('=', tagRanges);

            ReadOnlySpan<char> key = tag[tagRanges[0]];
            ReadOnlySpan<char> value = tag[tagRanges[1]];
            if (key.Equals(_badgeInfoTag, StringComparison.Ordinal))
            {
                BadgeInfo = GetBadgeInfo(value);
            }
            else if (key.Equals(_badgesTag, StringComparison.Ordinal))
            {
                Badges = GetBadges(value);
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
                IsFirstMessage = GetIsFirstMsg(value);
            }
            else if (key.Equals(_idTag, StringComparison.Ordinal))
            {
                Id = GetId(value);
            }
            else if (key.Equals(_modTag, StringComparison.Ordinal))
            {
                IsModerator = GetIsModerator(value);
            }
            else if (key.Equals(_roomIdTag, StringComparison.Ordinal))
            {
                ChannelId = GetChannelId(value);
            }
            else if (key.Equals(_subscriberTag, StringComparison.Ordinal))
            {
                IsSubscriber = GetIsSubscriber(value);
            }
            else if (key.Equals(_tmiSentTsTag, StringComparison.Ordinal))
            {
                TmiSentTs = GetTmiSentTs(value);
            }
            else if (key.Equals(_turboTag, StringComparison.Ordinal))
            {
                IsTurboUser = GetIsTurboUser(value);
            }
            else if (key.Equals(_userIdTag, StringComparison.Ordinal))
            {
                UserId = GetUserId(value);
            }
        }

        IsAction = ircMessage[ircRanges[4]].SequenceEqual(_actionPrefix);
        Username = DisplayName.ToLower();
        Channel = new(ircMessage[ircRanges[3]][1..]);
        Message = GetMessage(ircMessage, ircRanges);
        RawIrcMessage = rawIrcMessage ?? new(ircMessage);
    }

    /// <summary>
    /// An empty constructor. Can be used to set properties on initialization.
    /// </summary>
    public ChatMessage()
    {
        Username = string.Empty;
        Channel = string.Empty;
        Message = string.Empty;
        RawIrcMessage = string.Empty;
    }

    private string GetMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<Range> ircRanges)
    {
        return new(IsAction ? ircMessage[(ircMessage.IndexOf('\u0001') + 8)..^1] : ircMessage[(ircRanges[3].End.Value + 2)..]);
    }

    private static ReadOnlyDictionary<string, string> GetBadgeInfo(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return _emptyDictionary;
        }

        Span<Range> ranges = stackalloc Range[value.Length];
        int rangesLength = value.GetRangesOfSplit(',', ranges);
        Dictionary<string, string> result = new(rangesLength);
        for (int i = 0; i < rangesLength; i++)
        {
            ReadOnlySpan<char> info = value[ranges[i]];
            // ReSharper disable once StackAllocInsideLoop
            Span<Range> infoRanges = stackalloc Range[2];
            info.GetRangesOfSplit('/', infoRanges);

            string key = new(info[infoRanges[0]]);
            string val = new(info[infoRanges[1]]);
            result.Add(key, val);
        }

        return new(result);
    }

    private static Badge[] GetBadges(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return Array.Empty<Badge>();
        }

        Span<Range> badgesRanges = stackalloc Range[value.Length];
        int badgesRangesLength = value.GetRangesOfSplit(',', badgesRanges);
        badgesRanges = badgesRanges[..badgesRangesLength];
        Badge[] result = new Badge[badgesRangesLength];
        for (int i = 0; i < badgesRangesLength; i++)
        {
            ReadOnlySpan<char> info = value[badgesRanges[i]];
            // ReSharper disable once StackAllocInsideLoop
            Span<Range> infoRanges = stackalloc Range[2];
            info.GetRangesOfSplit('/', infoRanges);

            string name = new(info[infoRanges[0]]);
            string level = new(info[infoRanges[1]]);
            result[i] = new(name, level);
        }

        return result;
    }

    private static Color GetColor(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return Color.Empty;
        }

        byte r = byte.Parse(value[1..3], NumberStyles.HexNumber);
        byte g = byte.Parse(value[3..5], NumberStyles.HexNumber);
        byte b = byte.Parse(value[5..7], NumberStyles.HexNumber);
        return Color.FromArgb(r, g, b);
    }

    private static string GetDisplayName(ReadOnlySpan<char> value)
    {
        return new(value.EndsWith(_nameWithSpaceEnding) ? value[..^2] : value);
    }

    private static bool GetIsFirstMsg(ReadOnlySpan<char> value) => value[0] == '1';

    private static Guid GetId(ReadOnlySpan<char> value) => Guid.Parse(value);

    private static bool GetIsModerator(ReadOnlySpan<char> value) => value[0] == '1';

    private static long GetChannelId(ReadOnlySpan<char> value) => long.Parse(value);

    private static bool GetIsSubscriber(ReadOnlySpan<char> value) => value[0] == '1';

    private static long GetTmiSentTs(ReadOnlySpan<char> value) => long.Parse(value);

    private static bool GetIsTurboUser(ReadOnlySpan<char> value) => value[0] == '1';

    private static long GetUserId(ReadOnlySpan<char> value) => long.Parse(value);

    public override string ToString()
    {
        return $"<#{Channel}> {Username}: {Message}";
    }
}
