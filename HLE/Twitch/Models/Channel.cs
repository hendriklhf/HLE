using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a channel with all its room states.
/// </summary>
public sealed class Channel : IEquatable<Channel>
{
    /// <summary>
    /// The username of the channel owner. All lower case, without '#'.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The user id of the channel owner.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Indicates whether emote-only mode is turned on or off.
    /// </summary>
    public bool EmoteOnly { get; private set; }

    /// <summary>
    /// Indicates whether followers-only mode is turned on or off.
    /// Value is "-1" if turned off, otherwise the value indicates the number of minutes a user has to follow the channel in order to be able to send messages.
    /// </summary>
    public int FollowersOnly { get; private set; }

    /// <summary>
    /// Indicates whether R9K mode is turned on or off.
    /// </summary>
    public bool R9K { get; private set; }

    /// <summary>
    /// Indicates whether slow mode is turned on or off.
    /// Value is "-1" if turned off, otherwise the value indicates the number of seconds between each message a user can send.
    /// </summary>
    public int SlowMode { get; private set; }

    /// <summary>
    /// Indicates whether subs-only mode is turned on or off.
    /// </summary>
    public bool SubsOnly { get; private set; }

    internal readonly string _prefixedName;

    private static readonly ChangedRoomStates[] _allChangedRoomStatesValues = Enum.GetValues<ChangedRoomStates>();

    internal Channel(in Roomstate args)
    {
        Name = args.Channel;
        _prefixedName = '#' + args.Channel;
        Id = args.ChannelId;
        EmoteOnly = args.EmoteOnly;
        FollowersOnly = args.FollowersOnly;
        R9K = args.R9K;
        SlowMode = args.SlowMode;
        SubsOnly = args.SubsOnly;
    }

    internal void Update(in Roomstate args)
    {
        ref ChangedRoomStates changedRoomStatesReference = ref MemoryMarshal.GetArrayDataReference(_allChangedRoomStatesValues);
        for (int i = 0; i < _allChangedRoomStatesValues.Length; i++)
        {
            ChangedRoomStates roomState = Unsafe.Add(ref changedRoomStatesReference, i);
            bool roomstateChanged = (args.ChangedStates & roomState) == roomState;
            if (!roomstateChanged)
            {
                continue;
            }

            switch (roomState)
            {
                case ChangedRoomStates.EmoteOnly:
                    EmoteOnly = args.EmoteOnly;
                    break;
                case ChangedRoomStates.FollowersOnly:
                    FollowersOnly = args.FollowersOnly;
                    break;
                case ChangedRoomStates.R9K:
                    R9K = args.R9K;
                    break;
                case ChangedRoomStates.SlowMode:
                    SlowMode = args.SlowMode;
                    break;
                case ChangedRoomStates.SubsOnly:
                    SubsOnly = args.SubsOnly;
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(roomState), (int)roomState, typeof(ChangedRoomStates));
            }
        }
    }

    public bool Equals(Channel? other)
    {
        return ReferenceEquals(this, other) || Id == other?.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Channel other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Channel? left, Channel? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Channel? left, Channel? right)
    {
        return !(left == right);
    }
}
