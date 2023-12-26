using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Twitch.Tmi.Models;

/// <summary>
/// Arguments used when a user joined a channel.
/// </summary>
/// <param name="username">The name of the user that joined the <paramref name="channel"/>.</param>
/// <param name="channel">The channel that the user joined.</param>
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
    public static JoinChannelMessage Create(string username, string channel) => new(username, channel);

    [Pure]
    public bool Equals(JoinChannelMessage other) => Username == other.Username && Channel == other.Channel;

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is JoinChannelMessage other && Equals(other);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(Username, Channel);

    public static bool operator ==(JoinChannelMessage left, JoinChannelMessage right) => left.Equals(right);

    public static bool operator !=(JoinChannelMessage left, JoinChannelMessage right) => !(left == right);
}
