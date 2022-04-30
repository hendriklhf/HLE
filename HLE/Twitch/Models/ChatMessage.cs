using System;
using System.Drawing;
using System.Linq;
using HLE.Collections;

namespace HLE.Twitch.Models;

public class ChatMessage
{
    public object? BadgeInfo { get; init; }

    public Badge[] Badges { get; init; }

    public Color Color { get; init; }

    public string DisplayName { get; init; }

    public object? Emotes { get; init; }

    public bool IsFirstMessage { get; init; }

    public object? Flags { get; init; }

    public Guid Id { get; init; }

    public bool IsModerator { get; init; }

    public int ChannelId { get; init; }

    public bool IsSubscriber { get; init; }

    public long TmiSentTs { get; init; }

    public bool IsTurboUser { get; init; }

    public int UserId { get; init; }

    public object? UserType { get; init; }

    public string Username { get; init; }

    public string Channel { get; init; }

    public string Content { get; init; }

    public string RawIrcMessage { get; init; }

    public ChatMessage(string ircMessage)
    {
        string[] split = ircMessage.Split();
        string[] privmsgSplit = split[0][1..].Split(';');

        //BadgeInfo =
        string[] badgesComplete = privmsgSplit[1].Split('=');
        string[] badges = badgesComplete[1].Split(',');
        Badges = badges.Select(b =>
        {
            string[] bSplit = b.Split('/');
            return new Badge(bSplit[0], bSplit[1].ToInt());
        }).ToArray();
        int[] rgb = privmsgSplit[2][1..].Split(2).Select(e => Convert.ToInt32(e, 16)).ToArray();
        Color = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        DisplayName = Utils.EndingWordPattern.Match(privmsgSplit[3]).Value;
        //Emotes =
        IsFirstMessage = privmsgSplit[5][^1] == '1';
        //Flags =
        Id = Guid.Parse(privmsgSplit[7].Split('=')[1]);
        IsModerator = privmsgSplit[8][^1] == '1';
        ChannelId = Utils.EndingNumbersPattern.Match(privmsgSplit[9]).Value.ToInt();
        IsSubscriber = privmsgSplit[10][^1] == '1';
        TmiSentTs = Utils.EndingNumbersPattern.Match(privmsgSplit[11]).Value.ToInt();
        IsTurboUser = privmsgSplit[12][^1] == '1';
        UserId = Utils.EndingNumbersPattern.Match(privmsgSplit[13]).Value.ToInt();
        //UserType =
        Username = DisplayName.ToLower();
        Channel = split[3][1..];
        Content = split[4..].JoinToString(' ')[1..];
        RawIrcMessage = ircMessage;
    }
}
