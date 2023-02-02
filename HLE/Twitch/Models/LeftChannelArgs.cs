using System;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user left a channel.
/// </summary>
public readonly struct LeftChannelArgs
{
    /// <summary>
    /// The username of the user that left the channel. All lower case.
    /// </summary>
    public ReadOnlyMemory<char> Username { get; }

    /// <summary>
    /// The chanel the user left. All lower case.
    /// </summary>
    public ReadOnlyMemory<char> Channel { get; }

    /// <summary>
    /// The default constructor of <see cref="LeftChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="ircRanges">Ranges that represent the message split on whitespaces.</param>
    public LeftChannelArgs(ReadOnlyMemory<char> ircMessage, Span<Range> ircRanges)
    {
        ReadOnlyMemory<char> split0 = ircMessage[ircRanges[0]];
        int idxExcl = split0.Span.IndexOf('!');
        Username = split0[1..idxExcl];
        Channel = ircMessage[ircRanges[^1]][1..];
    }
}
