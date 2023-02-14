using System;
using System.Runtime.CompilerServices;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when the state of a chat room changed.
/// For example, if emote-only mode has been turned on.<br/>
/// Since this struct has a instance size bigger than a reference size, it should always be passed by reference.
/// </summary>
public readonly struct RoomstateArgs
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
    /// The username of the channel owner. All lower case, without '#'.
    /// </summary>
    public string Channel { get; init; }

    /// <summary>
    /// Indicates whether slow mode is turned on or off.
    /// Value is "0" if turned off, otherwise the value indicates the number of seconds between each message a user can send.
    /// </summary>
    public int SlowMode { get; init; }

    /// <summary>
    /// Indicates whether subs-only mode is turned on or off.
    /// </summary>
    public bool SubsOnly { get; init; }

    internal readonly ChangedRoomstate _changedStatesFlags = 0;

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
    /// <param name="indicesOfWhitespaces">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public RoomstateArgs(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> tags = ircMessage[1..indicesOfWhitespaces[0]];
        int equalsSignIndex = tags.IndexOf('=');
        while (equalsSignIndex != -1)
        {
            int semicolonIndex = tags.IndexOf(';');
            Index valueEnd = Unsafe.As<int, Index>(ref semicolonIndex);
            ReadOnlySpan<char> key = tags[..equalsSignIndex];
            ReadOnlySpan<char> value = tags[(equalsSignIndex + 1)..valueEnd];
            tags = semicolonIndex == -1 ? tags[tags.Length..] : tags[(semicolonIndex + 1)..];
            equalsSignIndex = tags.IndexOf('=');

            if (key.Equals(_emoteOnlyTag, StringComparison.Ordinal))
            {
                EmoteOnly = GetEmoteOnly(value);
                _changedStatesFlags |= ChangedRoomstate.EmoteOnly;
            }
            else if (key.Equals(_followersOnlyTag, StringComparison.Ordinal))
            {
                FollowersOnly = GetFollowersOnly(value);
                _changedStatesFlags |= ChangedRoomstate.FollowersOnly;
            }
            else if (key.Equals(_r9KTag, StringComparison.Ordinal))
            {
                R9K = GetR9K(value);
                _changedStatesFlags |= ChangedRoomstate.R9K;
            }
            else if (key.Equals(_roomIdTag, StringComparison.Ordinal))
            {
                ChannelId = GetChannelId(value);
            }
            else if (key.Equals(_slowModeTag, StringComparison.Ordinal))
            {
                SlowMode = GetSlowMode(value);
                _changedStatesFlags |= ChangedRoomstate.SlowMode;
            }
            else if (key.Equals(_subsOnlyTag, StringComparison.Ordinal))
            {
                SubsOnly = GetSubsOnly(value);
                _changedStatesFlags |= ChangedRoomstate.SubsOnly;
            }
        }

        Channel = new(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetEmoteOnly(ReadOnlySpan<char> value) => value[0] == '1';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFollowersOnly(ReadOnlySpan<char> value)
    {
        if (value[0] == '-')
        {
            return -1;
        }

        return NumberHelper.ParsePositiveInt32(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetR9K(ReadOnlySpan<char> value) => value[0] == '1';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetChannelId(ReadOnlySpan<char> value) => NumberHelper.ParsePositiveInt64(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetSlowMode(ReadOnlySpan<char> value) => NumberHelper.ParsePositiveInt32(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetSubsOnly(ReadOnlySpan<char> value) => value[0] == '1';
}
