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

    private static readonly ChangedRoomstateFlag[] _roomstates = Enum.GetValues<ChangedRoomstateFlag>();

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
        ref ChangedRoomstateFlag firstRoomstate = ref MemoryMarshal.GetArrayDataReference(_roomstates);
        for (int i = 0; i < _roomstates.Length; i++)
        {
            ChangedRoomstateFlag roomstateFlag = Unsafe.Add(ref firstRoomstate, i);
            bool roomstateChanged = (args.ChangedStates & roomstateFlag) == roomstateFlag;
            if (!roomstateChanged)
            {
                continue;
            }

            switch (roomstateFlag)
            {
                case ChangedRoomstateFlag.EmoteOnly:
                    EmoteOnly = args.EmoteOnly;
                    break;
                case ChangedRoomstateFlag.FollowersOnly:
                    FollowersOnly = args.FollowersOnly;
                    break;
                case ChangedRoomstateFlag.R9K:
                    R9K = args.R9K;
                    break;
                case ChangedRoomstateFlag.SlowMode:
                    SlowMode = args.SlowMode;
                    break;
                case ChangedRoomstateFlag.SubsOnly:
                    SubsOnly = args.SubsOnly;
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(roomstateFlag), (int)roomstateFlag, typeof(ChangedRoomstateFlag));
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
