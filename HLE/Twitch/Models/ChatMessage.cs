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
[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[DebuggerDisplay("<#{Channel}> {Username}: {Message}")]
public sealed class ChatMessage
{
    /// <summary>
    /// Holds information about a badge, that can be obtained by its name found in <see cref="Badges"/>.
    /// </summary>
    public ReadOnlyDictionary<string, int> BadgeInfo { get; init; } = _emptyDictionary;

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

    private static readonly ReadOnlyDictionary<string, int> _emptyDictionary = new(new Dictionary<string, int>());

    private const string _actionPrefix = ":\u0001ACTION";
    private const string _nameWithSpaceEnding = "\\s";

    private const string _badgeInfoTag = "badge-info";
    private const string _badgesTag = "badges";
    private const string _colorTag = "color";
    private const string _displayNameTag = "display-name";
    private const string _firstMsgTag = "first-mgs";
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
    /// <param name="ircMessage">The IRC message.</param>
    public ChatMessage(string ircMessage)
    {
        ReadOnlySpan<char> ircSpan = ircMessage;
        Range[] ircRanges = ircSpan.GetRangesOfSplit();
        ReadOnlySpan<char> tagsSpan = ircSpan[ircRanges[0]][1..];
        Range[] tagsRanges = tagsSpan.GetRangesOfSplit(';');
        foreach (Range r in tagsRanges)
        {
            ReadOnlySpan<char> tag = tagsSpan[r];
            Range[] tagRanges = tag.GetRangesOfSplit('=');
            ReadOnlySpan<char> key = tag[tagRanges[0]];
            ReadOnlySpan<char> value = tag[tagRanges[1]];
            if (key.SequenceEqual(_badgeInfoTag))
            {
                BadgeInfo = GetBadgeInfo(value);
            }
            else if (key.SequenceEqual(_badgesTag))
            {
                Badges = GetBadges(value);
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
                IsFirstMessage = GetIsFirstMsg(value);
            }
            else if (key.SequenceEqual(_idTag))
            {
                Id = GetId(value);
            }
            else if (key.SequenceEqual(_modTag))
            {
                IsModerator = GetIsModerator(value);
            }
            else if (key.SequenceEqual(_roomIdTag))
            {
                ChannelId = GetChannelId(value);
            }
            else if (key.SequenceEqual(_subscriberTag))
            {
                IsSubscriber = GetIsSubscriber(value);
            }
            else if (key.SequenceEqual(_tmiSentTsTag))
            {
                TmiSentTs = GetTmiSentTs(value);
            }
            else if (key.SequenceEqual(_turboTag))
            {
                IsTurboUser = GetIsTurboUser(value);
            }
            else if (key.SequenceEqual(_userIdTag))
            {
                UserId = GetUserId(value);
            }
        }

        IsAction = ircSpan[ircRanges[4]].SequenceEqual(_actionPrefix);
        Username = DisplayName.ToLower();
        Channel = new(ircSpan[ircRanges[3]][1..]);
        Message = GetMessage(ircSpan);
        RawIrcMessage = ircMessage;
    }

    /// <summary>
    /// An empty constructor. Can be used to set properties on initialization.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public ChatMessage()
    {
        Username = string.Empty;
        Channel = string.Empty;
        Message = string.Empty;
        RawIrcMessage = string.Empty;
    }

    private string GetMessage(ReadOnlySpan<char> ircSpan)
    {
        return new(IsAction ? ircSpan[(ircSpan.IndexOf('\u0001') + 5)..^1] : ircSpan[(ircSpan.GetRangesOfSplit()[3].End.Value + 2)..]);
    }

    private static ReadOnlyDictionary<string, int> GetBadgeInfo(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return _emptyDictionary;
        }

        Dictionary<string, int> result = new();
        Range[] ranges = value.GetRangesOfSplit(',');
        foreach (Range r in ranges)
        {
            ReadOnlySpan<char> info = value[r];
            Range[] infoRanges = info.GetRangesOfSplit('/');
            string key = new(info[infoRanges[0]]);
            int val = int.Parse(info[infoRanges[1]]);
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

        Range[] badgesRanges = value.GetRangesOfSplit(',');
        Badge[] result = new Badge[badgesRanges.Length];
        for (int i = 0; i < result.Length; i++)
        {
            ReadOnlySpan<char> info = value[badgesRanges[i]];
            Range[] infoRanges = info.GetRangesOfSplit('/');
            string name = new(info[infoRanges[0]]);
            int level = int.Parse(info[infoRanges[1]]);
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

    private static bool GetIsFirstMsg(ReadOnlySpan<char> value) => value[^1] == '1';

    private static Guid GetId(ReadOnlySpan<char> value) => Guid.Parse(value);

    private static bool GetIsModerator(ReadOnlySpan<char> value) => value[^1] == '1';

    private static long GetChannelId(ReadOnlySpan<char> value) => long.Parse(value);

    private static bool GetIsSubscriber(ReadOnlySpan<char> value) => value[^1] == '1';

    private static long GetTmiSentTs(ReadOnlySpan<char> value) => long.Parse(value);

    private static bool GetIsTurboUser(ReadOnlySpan<char> value) => value[^1] == '1';

    private static long GetUserId(ReadOnlySpan<char> value) => long.Parse(value);

    public override string ToString()
    {
        return $"<#{Channel}> {Username}: {Message}";
    }
}
