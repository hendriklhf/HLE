using System;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user joined a channel.
/// </summary>
public readonly struct JoinChannelMessage(string username, string channel)
    : IMembershipMessage<JoinChannelMessage>, IEquatable<JoinChannelMessage>
{
    /// <summary>
    /// The username of the user that joined the channel. All lower case.
    /// </summary>
    public string Username { get; } = username;

    /// <summary>
    /// The channel the user joined. All lower case, without '#'.
    /// </summary>
    public string Channel { get; } = channel;

    [Pure]
    public static JoinChannelMessage Create(string username, string channel)
    {
        return new(username, channel);
    }

    public bool Equals(JoinChannelMessage other)
    {
        return Username == other.Username && Channel == other.Channel;
    }

    public override bool Equals(object? obj)
    {
        return obj is JoinChannelMessage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Username, Channel);
    }

    public static bool operator ==(JoinChannelMessage left, JoinChannelMessage right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(JoinChannelMessage left, JoinChannelMessage right)
    {
        return !(left == right);
    }
}
