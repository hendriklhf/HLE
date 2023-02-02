using System;

namespace HLE.Twitch.Models;

/// <summary>
/// Arguments used when a user joined a channel.
/// </summary>
public readonly struct JoinedChannelArgs
{
    /// <summary>
    /// The username of the user that joined the channel. All lower case.
    /// </summary>
    public ReadOnlyMemory<char> Username { get; }

    /// <summary>
    /// The channel the user joined. All lower case.
    /// </summary>
    public ReadOnlyMemory<char> Channel { get; }

    /// <summary>
    /// The default constructor of <see cref="JoinedChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="ircRanges">Ranges that represent the message split on whitespaces.</param>
    public JoinedChannelArgs(ReadOnlyMemory<char> ircMessage, Span<Range> ircRanges)
    {
        ReadOnlyMemory<char> split0 = ircMessage[ircRanges[0]];
        int idxExcl = split0.Span.IndexOf('!');
        Username = split0[1..idxExcl];
        Channel = ircMessage[ircRanges[^1]][1..];
    }
}
