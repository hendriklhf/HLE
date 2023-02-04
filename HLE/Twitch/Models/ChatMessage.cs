using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public ReadOnlyDictionary<string, string> BadgeInfo { get; init; } = _emptyDictionary;

    /// <summary>
    /// Holds all the badges the user has.
    /// </summary>
    public ReadOnlySpan<Badge> Badges => _badges;

    /// <summary>
    /// The color of the user's name in a Twitch chat overlay.
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    /// The display name of the user with the preferred casing.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the message is the first message the user has sent in the channel or not.
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

    private readonly Badge[] _badges = Array.Empty<Badge>();

    private static readonly ReadOnlyDictionary<string, string> _emptyDictionary = new(new Dictionary<string, string>());

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
        Span<Range> tagsRanges = stackalloc Range[tags.Length];
        int tagsRangesLength = tags.GetRangesOfSplit(';', tagsRanges);

        for (int i = 0; i < tagsRangesLength; i++)
        {
            ReadOnlySpan<char> tag = tags[tagsRanges[i]];
            int equalSignIndex = tag.IndexOf('=');
            ReadOnlySpan<char> key = tag[..equalSignIndex];
            ReadOnlySpan<char> value = tag[(equalSignIndex + 1)..];
            if (key.Equals(_badgeInfoTag, StringComparison.Ordinal))
            {
                BadgeInfo = GetBadgeInfo(value);
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

        IsAction = ircMessage[ircRanges[4]].Equals(_actionPrefix, StringComparison.Ordinal);
        Username = DisplayName.ToLowerInvariant();
        Channel = new(ircMessage[ircRanges[3]][1..]);
        Message = GetMessage(ircMessage, ircRanges);
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
            int slashIndex = info.IndexOf('/');
            string key = new(info[..slashIndex]);
            string val = new(info[(slashIndex + 1)..]);
            CollectionsMarshal.GetValueRefOrAddDefault(result, key, out _) = val;
        }

        return new(result);
    }

    private static Badge[] GetBadges(ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            return Array.Empty<Badge>();
        }

        Span<Range> badgesRanges = stackalloc Range[value.Length];
        int badgesRangesLength = value.GetRangesOfSplit(',', badgesRanges);
        badgesRanges = badgesRanges[..badgesRangesLength];
        Badge[] result = new Badge[badgesRangesLength];
        ref Badge firstBadge = ref MemoryMarshal.GetArrayDataReference(result);
        for (int i = 0; i < badgesRangesLength; i++)
        {
            ReadOnlySpan<char> info = value[badgesRanges[i]];
            int slashIndex = info.IndexOf('/');
            string name = new(info[..slashIndex]);
            string level = new(info[(slashIndex + 1)..]);
            Unsafe.Add(ref firstBadge, i) = new(name, level);
        }

        return result;
    }

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

    private static string GetDisplayName(ReadOnlySpan<char> value)
    {
        return new(value[^2] == _nameWithSpaceEnding[0] && value[^1] == _nameWithSpaceEnding[1] ? value[..^2] : value);
    }

    private static bool GetIsFirstMsg(ReadOnlySpan<char> value) => value[0] == '1';

    private static Guid GetId(ReadOnlySpan<char> value) => Guid.ParseExact(value, _guidFormat);

    private static bool GetIsModerator(ReadOnlySpan<char> value) => value[0] == '1';

    private static long GetChannelId(ReadOnlySpan<char> value) => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    private static bool GetIsSubscriber(ReadOnlySpan<char> value) => value[0] == '1';

    private static long GetTmiSentTs(ReadOnlySpan<char> value) => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

    private static bool GetIsTurboUser(ReadOnlySpan<char> value) => value[0] == '1';

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
