using System;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user joined a channel.
/// </summary>
public readonly struct JoinedChannelArgs : IEquatable<JoinedChannelArgs>
{
    /// <summary>
    /// The username of the user that joined the channel. All lower case.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The channel the user joined. All lower case, without '#'.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The default constructor of <see cref="JoinedChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="indicesOfWhitespaces">The indices of whitespaces (char 32) in <paramref name="ircMessage"/>.</param>
    public JoinedChannelArgs(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        int idxExcl = firstWord.IndexOf('!');
        Username = new(firstWord[1..idxExcl]);
        Channel = new(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
    }

    public bool Equals(JoinedChannelArgs other)
    {
        return Username == other.Username && Channel == other.Channel;
    }

    public override bool Equals(object? obj)
    {
        return obj is JoinedChannelArgs other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Username, Channel);
    }

    public static bool operator ==(JoinedChannelArgs left, JoinedChannelArgs right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(JoinedChannelArgs left, JoinedChannelArgs right)
    {
        return !(left == right);
    }
}
