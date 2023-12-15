using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Tmi.Models;

/// <summary>
/// Arguments used when a user left a channel.
/// </summary>
public readonly struct LeftChannelMessage(string username, string channel)
    : IMembershipMessage<LeftChannelMessage>, IEquatable<LeftChannelMessage>
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
    public static LeftChannelMessage Create(string username, string channel) => new(username, channel);

    [Pure]
    public bool Equals(LeftChannelMessage other) => Username == other.Username && Channel == other.Channel;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is LeftChannelMessage other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Username, Channel);

    public static bool operator ==(LeftChannelMessage left, LeftChannelMessage right) => left.Equals(right);

    public static bool operator !=(LeftChannelMessage left, LeftChannelMessage right) => !(left == right);
}
