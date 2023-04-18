using System;
using HLE.Strings;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user left a channel.
/// </summary>
public readonly struct LeftChannelArgs : IEquatable<LeftChannelArgs>
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
    /// The default constructor of <see cref="LeftChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="indicesOfWhitespaces">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public LeftChannelArgs(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        int indexOfExclamationMark = firstWord.IndexOf('!');
        Username = StringPool.Shared.GetOrAdd(firstWord[1..indexOfExclamationMark]);
        Channel = StringPool.Shared.GetOrAdd(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
    }

    public bool Equals(LeftChannelArgs other)
    {
        return Username == other.Username && Channel == other.Channel;
    }

    public override bool Equals(object? obj)
    {
        return obj is LeftChannelArgs other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Username, Channel);
    }

    public static bool operator ==(LeftChannelArgs left, LeftChannelArgs right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LeftChannelArgs left, LeftChannelArgs right)
    {
        return !(left == right);
    }
}
