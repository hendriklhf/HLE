using System;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user left a channel.
/// </summary>
public readonly struct LeftChannelMessage : IMembershipMessage<LeftChannelMessage>, IEquatable<LeftChannelMessage>
{
    /// <summary>
    /// The username of the user that left the channel. All lower case.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The channel the user left. All lower case, without '#'.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The default constructor of <see cref="LeftChannelMessage"/>.
    /// </summary>
    public LeftChannelMessage(string username, string channel)
    {
        Username = username;
        Channel = channel;
    }

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
