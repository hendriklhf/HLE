using System;

namespace HLE.Twitch.Args;

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
    /// /// <param name="split">The IRC message split on whitespaces. Optional if a split has been done prior to calling this method.</param>
    public JoinedChannelArgs(string ircMessage, string[]? split = null)
    {
        split ??= ircMessage.Split();
        int idxExcl = split[0].IndexOf('!');
        Username = split[0][1..idxExcl];
        Channel = split[^1][1..];
    }
}
