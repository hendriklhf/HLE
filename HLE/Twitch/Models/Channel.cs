using System;

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
    /// Indicates whether slow mode os turned on or off.
    /// Value is "-1" if turned off, otherwise the value indicates the number of seconds between each message a user can send.
    /// </summary>
    public int SlowMode { get; private set; }

    /// <summary>
    /// Indicates whether subs-only mode is turned on or off.
    /// </summary>
    public bool SubsOnly { get; private set; }

    private static readonly ChangedRoomstates[] _changedRoomstates = Enum.GetValues<ChangedRoomstates>();

    internal Channel(RoomstateArgs args)
    {
        Name = args.Channel;
        Id = args.ChannelId;
        EmoteOnly = args.EmoteOnly;
        FollowersOnly = args.FollowersOnly;
        R9K = args.R9K;
        SlowMode = args.SlowMode;
        SubsOnly = args.SubsOnly;
    }

    internal void Update(RoomstateArgs args)
    {
        foreach (ChangedRoomstates rs in _changedRoomstates)
        {
            if (!args.ChangedStates.HasFlag(rs))
            {
                continue;
            }

            switch (rs)
            {
                case ChangedRoomstates.EmoteOnly:
                    EmoteOnly = args.EmoteOnly;
                    break;
                case ChangedRoomstates.FollowersOnly:
                    FollowersOnly = args.FollowersOnly;
                    break;
                case ChangedRoomstates.R9K:
                    R9K = args.R9K;
                    break;
                case ChangedRoomstates.SlowMode:
                    SlowMode = args.SlowMode;
                    break;
                case ChangedRoomstates.SubsOnly:
                    SubsOnly = args.SubsOnly;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
