using System;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Models;

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
    public static LeftChannelMessage Create(string username, string channel)
    {
        return new(username, channel);
    }

    public bool Equals(LeftChannelMessage other)
    {
        return Username == other.Username && Channel == other.Channel;
    }

    public override bool Equals(object? obj)
    {
        return obj is LeftChannelMessage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Username, Channel);
    }

    public static bool operator ==(LeftChannelMessage left, LeftChannelMessage right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LeftChannelMessage left, LeftChannelMessage right)
    {
        return !(left == right);
    }
}
