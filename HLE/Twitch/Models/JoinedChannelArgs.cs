using System;

namespace HLE.Twitch.Models;

/// <summary>
/// <see cref="EventArgs"/> used when a user joined a channel.
/// </summary>
public sealed class JoinedChannelArgs : EventArgs
{
    /// <summary>
    /// The username of the user that joined the channel. All lower case.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The channel the user joined. All lower case.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The default constructor of <see cref="JoinedChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <param name="ircRanges">Ranges that represent the message split on whitespaces.</param>
    public JoinedChannelArgs(ReadOnlySpan<char> ircMessage, Range[]? ircRanges = null)
    {
        ircRanges ??= ircMessage.GetRangesOfSplit();
        ReadOnlySpan<char> split0 = ircMessage[ircRanges[0]];
        int idxExcl = split0.IndexOf('!');
        Username = new(split0[1..idxExcl]);
        Channel = new(ircMessage[ircRanges[^1]][1..]);
    }
}
