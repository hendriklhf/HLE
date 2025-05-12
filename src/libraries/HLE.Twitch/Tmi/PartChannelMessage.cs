using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Arguments used when a user left a channel.
/// </summary>
/// /// <param name="username">The name of the user that left the <paramref name="channel"/>.</param>
/// <param name="channel">The channel that the user left.</param>
public readonly struct PartChannelMessage(string username, string channel)
    : IMembershipMessage<PartChannelMessage>, IEquatable<PartChannelMessage>
{
    /// <summary>
    /// The username of the user that left the channel. All lower case.
    /// </summary>
    public string Username { get; } = username;

    /// <summary>
    /// The channel the user left. All lower case, without '#'.
    /// </summary>
    public string Channel { get; } = channel;

    [Pure]
    public static PartChannelMessage Create(string username, string channel) => new(username, channel);

    [Pure]
    public bool Equals(PartChannelMessage other) => Username == other.Username && Channel == other.Channel;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is PartChannelMessage other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Username, Channel);

    public static bool operator ==(PartChannelMessage left, PartChannelMessage right) => left.Equals(right);

    public static bool operator !=(PartChannelMessage left, PartChannelMessage right) => !(left == right);
}
