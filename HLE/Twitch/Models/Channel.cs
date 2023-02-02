using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Twitch.Models;

/// <summary>
/// A class that represents a channel with all its room states.
/// </summary>
public sealed class Channel
{
    /// <summary>
    /// The username of the channel owner. All lower case.
    /// </summary>
    public string Name { get; }

    internal string PrefixedName { get; }

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

    private static readonly ChangedRoomstate[] _roomstates = Enum.GetValues<ChangedRoomstate>();

    internal Channel(in RoomstateArgs args)
    {
        Name = args.Channel;
        PrefixedName = '#' + args.Channel;
        Id = args.ChannelId;
        EmoteOnly = args.EmoteOnly;
        FollowersOnly = args.FollowersOnly;
        R9K = args.R9K;
        SlowMode = args.SlowMode;
        SubsOnly = args.SubsOnly;
    }

    internal void Update(in RoomstateArgs args)
    {
        ref ChangedRoomstate firstRoomstate = ref MemoryMarshal.GetArrayDataReference(_roomstates);
        for (int i = 0; i < _roomstates.Length; i++)
        {
            ChangedRoomstate roomstate = Unsafe.Add(ref firstRoomstate, i);
            bool roomstateChanged = (args.ChangedStates & roomstate) == roomstate;
            if (!roomstateChanged)
            {
                continue;
            }

            switch (roomstate)
            {
                case ChangedRoomstate.EmoteOnly:
                    EmoteOnly = args.EmoteOnly;
                    break;
                case ChangedRoomstate.FollowersOnly:
                    FollowersOnly = args.FollowersOnly;
                    break;
                case ChangedRoomstate.R9K:
                    R9K = args.R9K;
                    break;
                case ChangedRoomstate.SlowMode:
                    SlowMode = args.SlowMode;
                    break;
                case ChangedRoomstate.SubsOnly:
                    SubsOnly = args.SubsOnly;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
