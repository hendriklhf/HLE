using System;
using System.Runtime.CompilerServices;
using HLE.Memory;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when the state of a chat room changed.
/// For example, if emote-only mode has been turned on.<br/>
/// </summary>
public readonly struct Roomstate : IEquatable<Roomstate>
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
    public ChangedRoomStates ChangedStates { get; }

    /// <summary>
    /// The default constructor of <see cref="Roomstate"/>.
    /// </summary>
    public Roomstate(ChangedRoomStates changedStates)
    {
        ChangedStates = changedStates;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Roomstate other)
    {
        ref Roomstate thisRoomstate = ref Unsafe.AsRef(in this);
        return MemoryHelper.EqualsBytes(ref thisRoomstate, ref other);
    }

    public override bool Equals(object? obj)
    {
        return obj is Roomstate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ChannelId, EmoteOnly, FollowersOnly, R9K, SlowMode, SubsOnly, ChangedStates);
    }

    public static bool operator ==(Roomstate left, Roomstate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Roomstate left, Roomstate right)
    {
        return !(left == right);
    }
}
