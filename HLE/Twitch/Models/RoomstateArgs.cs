using System;

namespace HLE.Twitch.Models;

/// <summary>
/// <see cref="EventArgs"/> used when the state of a chat room changed.
/// For example if emote-only mode has been turned on.
/// </summary>
public sealed class RoomstateArgs : EventArgs
{
    /// <summary>
    /// Indicates whether emote-only mode is turned on or off.
    /// </summary>
    public bool EmoteOnly { get; init; }

    /// <summary>
    /// Indicates whether followers-only mode is turned on or off.
    /// Value is "-1" if turned off, otherwise the value indicates the number of minutes a user has to follow the channel in order to be able to send messages.
    /// </summary>
    public int FollowersOnly { get; init; } = -1;

    /// <summary>
    /// Indicates whether R9K mode is turned on or off.
    /// </summary>
    public bool R9K { get; init; }

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    public long ChannelId { get; init; }

    /// <summary>
    /// The username of the channel owner.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// Indicates whether slow mode is turned on or off.
    /// Value is "0" if turned off, otherwise the value indicates the number of seconds between each message a user can send.
    /// </summary>
    public int SlowMode { get; init; }

    /// <summary>
    /// Indicates whether subs-only mode is turned on or off.
    /// </summary>
    public bool SubsOnly { get; init; }

    internal ChangedRoomstate ChangedStates { get; } = 0;

    private const string _emoteOnlyTag = "emote-only";
    private const string _followersOnlyTag = "followers-only";
    private const string _r9KTag = "r9k";
    private const string _roomIdTag = "room-id";
    private const string _slowModeTag = "slow";
    private const string _subsOnlyTag = "subs-only";

    /// <summary>
    /// The default constructor of <see cref="RoomstateArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="ircRanges">Ranges that represent the message split on whitespaces.</param>
    public RoomstateArgs(ReadOnlySpan<char> ircMessage, Span<Range> ircRanges)
    {
        ReadOnlySpan<char> tags = ircMessage[ircRanges[0]][1..];
        ReadOnlySpan<Range> tagsRanges = tags.GetRangesOfSplit(';');
        for (int i = 0; i < tagsRanges.Length; i++)
        {
            ReadOnlySpan<char> tag = tags[tagsRanges[i]];
            ReadOnlySpan<Range> tagRanges = tag.GetRangesOfSplit('=');
            ReadOnlySpan<char> key = tag[tagRanges[0]];
            ReadOnlySpan<char> value = tag[tagRanges[1]];
            if (key.SequenceEqual(_emoteOnlyTag))
            {
                EmoteOnly = GetEmoteOnly(value);
                ChangedStates |= ChangedRoomstate.EmoteOnly;
            }
            else if (key.SequenceEqual(_followersOnlyTag))
            {
                FollowersOnly = GetFollowersOnly(value);
                ChangedStates |= ChangedRoomstate.FollowersOnly;
            }
            else if (key.SequenceEqual(_r9KTag))
            {
                R9K = GetR9K(value);
                ChangedStates |= ChangedRoomstate.R9K;
            }
            else if (key.SequenceEqual(_roomIdTag))
            {
                ChannelId = GetChannelId(value);
            }
            else if (key.SequenceEqual(_slowModeTag))
            {
                SlowMode = GetSlowMode(value);
                ChangedStates |= ChangedRoomstate.SlowMode;
            }
            else if (key.SequenceEqual(_subsOnlyTag))
            {
                SubsOnly = GetSubsOnly(value);
                ChangedStates |= ChangedRoomstate.SubsOnly;
            }
        }

        Channel = new(ircMessage[ircRanges[^1]][1..]);
    }

    private static bool GetEmoteOnly(ReadOnlySpan<char> value) => value[^1] == '1';

    private static int GetFollowersOnly(ReadOnlySpan<char> value) => int.Parse(value);

    private static bool GetR9K(ReadOnlySpan<char> value) => value[^1] == '1';

    private static long GetChannelId(ReadOnlySpan<char> value) => long.Parse(value);

    private static int GetSlowMode(ReadOnlySpan<char> value) => int.Parse(value);

    private static bool GetSubsOnly(ReadOnlySpan<char> value) => value[^1] == '1';
}
