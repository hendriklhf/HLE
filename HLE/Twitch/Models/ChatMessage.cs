using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using HLE.Collections;
using HLE.Twitch.Attributes;

namespace HLE.Twitch.Models;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ChatMessage
{
    [IrcTagName("badge-info")]
    public Dictionary<string, int> BadgeInfo { get; init; } = new();

    [IrcTagName("badges")]
    public Badge[] Badges { get; init; } = Array.Empty<Badge>();

    [IrcTagName("color")]
    public Color Color { get; init; }

    [IrcTagName("display-name")]
    public string DisplayName { get; init; } = string.Empty;

    [IrcTagName("first-msg")]
    public bool IsFirstMessage { get; init; }

    [IrcTagName("id")]
    public Guid Id { get; init; }

    [IrcTagName("mod")]
    public bool IsModerator { get; init; }

    [IrcTagName("room-id")]
    public long ChannelId { get; init; }

    [IrcTagName("subscriber")]
    public bool IsSubscriber { get; init; }

    [IrcTagName("tmi-sent-ts")]
    public long TmiSentTs { get; init; }

    [IrcTagName("turbo")]
    public bool IsTurboUser { get; init; }

    [IrcTagName("user-id")]
    public long UserId { get; init; }

    public string Username { get; init; }

    public string Channel { get; init; }

    public string Message { get; init; }

    public string RawIrcMessage { get; init; }

    private static readonly PropertyInfo[] _ircProps = typeof(ChatMessage).GetProperties().Where(p => p.GetCustomAttribute<IrcTagName>() is not null).ToArray();

    private static readonly MethodInfo[] _ircMethods = typeof(ChatMessage).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.GetCustomAttribute<MsgPropName>() is not null)
        .ToArray();

    public ChatMessage(string ircMessage)
    {
        string[] split = ircMessage.Split();
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

        Username = string.IsNullOrEmpty(DisplayName) ? string.Empty : DisplayName.ToLower();
        Channel = split[3][1..];
        Message = split[4..].JoinToString(' ')[1..];
        RawIrcMessage = ircMessage;
    }

    [MsgPropName(nameof(BadgeInfo))]
    private Dictionary<string, int> GetBadgeInfo(string value)
    {
        return string.IsNullOrEmpty(value)
            ? new()
            : value.Split(',').Select(s => s.Split('/')).ToDictionary(s => s[0], s => s[1].ToInt());
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
            return new Badge(bSplit[0], bSplit[1].ToInt());
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
        string displayName = Utils.EndingWordPattern.Match(value).Value;
        return displayName.EndsWith(@"\s") ? displayName[..^2] : displayName;
    }

    [MsgPropName(nameof(IsFirstMessage))]
    private bool GetIsFirstMsg(string value) => value[^1] == '1';

    [MsgPropName(nameof(Id))]
    private Guid GetId(string value) => Guid.Parse(value);

    [MsgPropName(nameof(IsModerator))]
    private bool GetIsModerator(string value) => value[^1] == '1';

    [MsgPropName(nameof(ChannelId))]
    private long GetChannelId(string value) => Utils.EndingNumbersPattern.Match(value).Value.ToLong();

    [MsgPropName(nameof(IsSubscriber))]
    private bool GetIsSubscriber(string value) => value[^1] == '1';

    [MsgPropName(nameof(TmiSentTs))]
    private long GetTmiSentTs(string value) => Utils.EndingNumbersPattern.Match(value).Value.ToLong();

    [MsgPropName(nameof(IsTurboUser))]
    private bool GetIsTurboUser(string value) => value[^1] == '1';

    [MsgPropName(nameof(UserId))]
    private long GetUserId(string value) => Utils.EndingNumbersPattern.Match(value).Value.ToLong();
}
