using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;

namespace HLE.Twitch.Tmi;

/// <summary>
/// A class that represents a channel with all its states.
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

    internal ImmutableArray<byte> PrefixedNameUtf8 { get; }

    internal Channel(ref readonly Roomstate args)
    {
        Name = args.Channel;
        PrefixedNameUtf8 = ImmutableCollectionsMarshal.AsImmutableArray(Encoding.UTF8.GetBytes($"#{args.Channel}"));
        Id = args.ChannelId;
        EmoteOnly = args.EmoteOnly;
        FollowersOnly = args.FollowersOnly;
        R9K = args.R9K;
        SlowMode = args.SlowMode;
        SubsOnly = args.SubsOnly;
    }

    internal void Update(ref readonly Roomstate args)
    {
        ReadOnlySpan<ChangedRoomStates> allChangedRoomStatesValues = EnumValues<ChangedRoomStates>.AsSpan();
        for (int i = 0; i < allChangedRoomStatesValues.Length; i++)
        {
            ChangedRoomStates roomState = allChangedRoomStatesValues[i];
            bool roomstateChanged = (args.ChangedStates & roomState) != 0;
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
                    ThrowHelper.ThrowInvalidEnumValue(roomState);
                    break;
            }
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] Channel? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Channel other && Equals(other);

    [Pure]
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Channel? left, Channel? right) => Equals(left, right);

    public static bool operator !=(Channel? left, Channel? right) => !(left == right);
}
