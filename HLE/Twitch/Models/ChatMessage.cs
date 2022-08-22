using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HLE.Twitch.Attributes;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a chat message.
/// </summary>
[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ChatMessage
{
    /// <summary>
    /// Holds information about a badge, that can be obtained by its name found in <see cref="Badges"/>.
    /// </summary>
    [IrcTagName("badge-info")]
    public Dictionary<string, int> BadgeInfo { get; init; } = new();

    /// <summary>
    /// Holds all the badges the user has.
    /// </summary>
    [IrcTagName("badges")]
    public Badge[] Badges { get; init; } = Array.Empty<Badge>();

    /// <summary>
    /// The color of the user's name in a Twitch chat overlay.
    /// </summary>
    [IrcTagName("color")]
    public Color Color { get; init; }

    /// <summary>
    /// The display name of the user with the preferred casing.
    /// </summary>
    [IrcTagName("display-name")]
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the message is the first message the user has sent in this channel or not.
    /// </summary>
    [IrcTagName("first-msg")]
    public bool IsFirstMessage { get; init; }

    /// <summary>
    /// The unique message id.
    /// </summary>
    [IrcTagName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Indicates whether the user is a moderator or not.
    /// </summary>
    [IrcTagName("mod")]
    public bool IsModerator { get; init; }

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    [IrcTagName("room-id")]
    public long ChannelId { get; init; }

    /// <summary>
    /// Indicates whether the user is a subscriber or not.
    /// The subscription age can be obtained from <see cref="Badges"/> and <see cref="BadgeInfo"/>.
    /// </summary>
    [IrcTagName("subscriber")]
    public bool IsSubscriber { get; init; }

    /// <summary>
    /// The unix timestamp in milliseconds of the moment the message has been sent.
    /// </summary>
    [IrcTagName("tmi-sent-ts")]
    public long TmiSentTs { get; init; }

    /// <summary>
    /// Indicates whether the user is subscribing to Twitch Turbo or not.
    /// </summary>
    [IrcTagName("turbo")]
    public bool IsTurboUser { get; init; }

    /// <summary>
    /// The user id of the user who sent the message.
    /// </summary>
    [IrcTagName("user-id")]
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

    private static readonly PropertyInfo[] _ircProps = typeof(ChatMessage).GetProperties().Where(p => p.GetCustomAttribute<IrcTagName>() is not null).ToArray();

    private static readonly MethodInfo[] _ircMethods = typeof(ChatMessage).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .Where(m => m.GetCustomAttribute<MsgPropName>() is not null).ToArray();

    private static readonly Regex _endingNumbersPattern = new(@"-?\d+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
    private static readonly Regex _endingWordPattern = new(@"\w+$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

    private const string _actionPrefix = ":\u0001ACTION";

    /// <summary>
    /// The default constructor of <see cref="ChatMessage"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// /// <param name="split">The IRC message split on whitespaces. Optional if a split has been done prior to calling this method.</param>
    public ChatMessage(string ircMessage, string[]? split = null)
    {
        split ??= ircMessage.Split();
        string[] privmsgSplit = split[0][1..].Split(';').ToArray();
        Dictionary<string, string> tagDic = privmsgSplit.Select(s => s.Split('=')).ToDictionary(sp => sp[0], sp => sp[1]);

        foreach (PropertyInfo prop in _ircProps)
        {
            IrcTagName attr = prop.GetCustomAttribute<IrcTagName>()!;
            if (!tagDic.TryGetValue(attr.Value, out string? value))
            {
                continue;
            }

            MethodInfo method = _ircMethods.First(m => m.GetCustomAttribute<MsgPropName>()!.Value == prop.Name);
            object? result = method.Invoke(this, new object[]
            {
                value
            });

            if (result is null)
            {
                continue;
            }

            prop.SetValue(this, result);
        }

        IsAction = split[4] == _actionPrefix;
        Username = string.IsNullOrEmpty(DisplayName) ? string.Empty : DisplayName.ToLower();
        Channel = split[3][1..];
        Message = GetMessage(ircMessage, split);
        RawIrcMessage = ircMessage;
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

    private string GetMessage(string ircMessage, string[] split)
    {
        return IsAction
            ? ircMessage[(split[..5].Sum(s => s.Length) + 5)..^1]
            : ircMessage[(split[..4].Sum(s => s.Length) + 5)..];
    }

    [MsgPropName(nameof(BadgeInfo))]
    private Dictionary<string, int> GetBadgeInfo(string value)
    {
        return string.IsNullOrEmpty(value)
            ? new()
            : value.Split(',').Select(s => s.Split('/')).ToDictionary(s => s[0], s => int.Parse(s[1]));
    }

    [MsgPropName(nameof(Badges))]
    private Badge[] GetBadges(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<Badge>();
        }

        string[] badges = value.Split(',');
        return badges.Select(b =>
        {
            string[] bSplit = b.Split('/');
            return new Badge(bSplit[0], int.Parse(bSplit[1]));
        }).ToArray();
    }

    [MsgPropName(nameof(Color))]
    private Color GetColor(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Color.Empty;
        }

        int[] rgb = value[1..].Split(2).Select(e => Convert.ToInt32(e, 16)).ToArray();
        return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
    }

    [MsgPropName(nameof(DisplayName))]
    private string GetDisplayName(string value)
    {
        string displayName = _endingWordPattern.Match(value).Value;
        return displayName.EndsWith(@"\s") ? displayName[..^2] : displayName;
    }

    [MsgPropName(nameof(IsFirstMessage))]
    private bool GetIsFirstMsg(string value) => value[^1] == '1';

    [MsgPropName(nameof(Id))]
    private Guid GetId(string value) => Guid.Parse(value);

    [MsgPropName(nameof(IsModerator))]
    private bool GetIsModerator(string value) => value[^1] == '1';

    [MsgPropName(nameof(ChannelId))]
    private long GetChannelId(string value) => long.Parse(_endingNumbersPattern.Match(value).Value);

    [MsgPropName(nameof(IsSubscriber))]
    private bool GetIsSubscriber(string value) => value[^1] == '1';

    [MsgPropName(nameof(TmiSentTs))]
    private long GetTmiSentTs(string value) => long.Parse(_endingNumbersPattern.Match(value).Value);

    [MsgPropName(nameof(IsTurboUser))]
    private bool GetIsTurboUser(string value) => value[^1] == '1';

    [MsgPropName(nameof(UserId))]
    private long GetUserId(string value) => long.Parse(_endingNumbersPattern.Match(value).Value);
}
