using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Arguments used when the state of a chat room changed.
/// For example, if emote-only mode has been turned on.<br/>
/// </summary>
/// <param name="changedRoomStates">Flags of states that have changed.</param>
[StructLayout(LayoutKind.Auto)]
public readonly struct Roomstate(ChangedRoomStates changedRoomStates) : IEquatable<Roomstate>
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
    public required string Channel { get; init; }

    /// <summary>
    /// Indicates whether slow mode is turned on or off.
    /// Value is "0" if turned off, otherwise the value indicates the number of seconds between each message a user can send.
    /// </summary>
    public int SlowMode { get; init; }

    /// <summary>
    /// Indicates whether subs-only mode is turned on or off.
    /// </summary>
    public bool SubsOnly { get; init; }

    /// <summary>
    /// Flags of the states that changed.
    /// </summary>
    public ChangedRoomStates ChangedStates { get; } = changedRoomStates;

    [Pure]
    public bool Equals(Roomstate other) =>
        EmoteOnly == other.EmoteOnly && FollowersOnly == other.FollowersOnly && R9K == other.R9K &&
        Channel == other.Channel && Channel == other.Channel && SlowMode == other.SlowMode
        && SubsOnly == other.SubsOnly && ChangedStates == other.ChangedStates;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Roomstate other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(ChannelId, EmoteOnly, FollowersOnly, R9K, SlowMode, SubsOnly, ChangedStates);

    public static bool operator ==(Roomstate left, Roomstate right) => left.Equals(right);

    public static bool operator !=(Roomstate left, Roomstate right) => !(left == right);
}
