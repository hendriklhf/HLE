using System;
using System.Runtime.CompilerServices;
using System.Text;

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

    internal readonly ChangedRoomstate _changedStateFlags = 0;

    private const byte _equalsSign = (byte)'=';
    private const byte _semicolon = (byte)';';
    private const byte _charOne = (byte)'1';
    private const byte _dash = (byte)'-';

    private static readonly byte[] _emoteOnlyTag = "emote-only"u8.ToArray();
    private static readonly byte[] _followersOnlyTag = "followers-only"u8.ToArray();
    private static readonly byte[] _r9KTag = "r9k"u8.ToArray();
    private static readonly byte[] _roomIdTag = "room-id"u8.ToArray();
    private static readonly byte[] _slowModeTag = "slow"u8.ToArray();
    private static readonly byte[] _subsOnlyTag = "subs-only"u8.ToArray();

    /// <summary>
    /// The default constructor of <see cref="RoomstateArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="indicesOfWhitespaces">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public RoomstateArgs(ReadOnlySpan<byte> ircMessage, Span<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> tags = ircMessage[1..indicesOfWhitespaces[0]];
        int equalsSignIndex = tags.IndexOf(_equalsSign);
        while (equalsSignIndex != -1)
        {
            int semicolonIndex = tags.IndexOf(_semicolon);
            Index valueEnd = Unsafe.As<int, Index>(ref semicolonIndex);
            ReadOnlySpan<byte> key = tags[..equalsSignIndex];
            ReadOnlySpan<byte> value = tags[(equalsSignIndex + 1)..valueEnd];
            tags = semicolonIndex == -1 ? tags[tags.Length..] : tags[(semicolonIndex + 1)..];
            equalsSignIndex = tags.IndexOf(_equalsSign);

            if (key.SequenceEqual(_emoteOnlyTag))
            {
                EmoteOnly = GetEmoteOnly(value);
                _changedStateFlags |= ChangedRoomstate.EmoteOnly;
            }
            else if (key.SequenceEqual(_followersOnlyTag))
            {
                FollowersOnly = GetFollowersOnly(value);
                _changedStateFlags |= ChangedRoomstate.FollowersOnly;
            }
            else if (key.SequenceEqual(_r9KTag))
            {
                R9K = GetR9K(value);
                _changedStateFlags |= ChangedRoomstate.R9K;
            }
            else if (key.SequenceEqual(_roomIdTag))
            {
                ChannelId = GetChannelId(value);
            }
            else if (key.SequenceEqual(_slowModeTag))
            {
                SlowMode = GetSlowMode(value);
                _changedStateFlags |= ChangedRoomstate.SlowMode;
            }
            else if (key.SequenceEqual(_subsOnlyTag))
            {
                SubsOnly = GetSubsOnly(value);
                _changedStateFlags |= ChangedRoomstate.SubsOnly;
            }
        }

        Channel = Encoding.UTF8.GetString(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetEmoteOnly(ReadOnlySpan<byte> value) => value[0] == _charOne;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetFollowersOnly(ReadOnlySpan<byte> value)
    {
        if (value[0] == _dash)
        {
            return -1;
        }

        return NumberHelper.ParsePositiveInt32FromUtf8Bytes(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetR9K(ReadOnlySpan<byte> value) => value[0] == _charOne;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetChannelId(ReadOnlySpan<byte> value) => NumberHelper.ParsePositiveInt64FromUtf8Bytes(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetSlowMode(ReadOnlySpan<byte> value) => NumberHelper.ParsePositiveInt32FromUtf8Bytes(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetSubsOnly(ReadOnlySpan<byte> value) => value[0] == _charOne;
}
