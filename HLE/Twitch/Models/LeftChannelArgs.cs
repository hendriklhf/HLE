using System;

namespace HLE.Twitch.Models;

/// <summary>
/// <see cref="EventArgs"/> used whe a user left a channel.
/// </summary>
public sealed class LeftChannelArgs : EventArgs
{
    /// <summary>
    /// The username of the user that left the channel. All lower case.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The chanel the user left. All lower case.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// The default constructor of <see cref="LeftChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// /// <param name="split">The IRC message split on whitespaces. Optional if a split has been done prior to calling this method.</param>
    public LeftChannelArgs(string ircMessage, string[]? split = null)
    {
        split ??= ircMessage.Split();
        int idxExcl = split[0].IndexOf('!');
        Username = split[0][1..idxExcl];
        Channel = split[2][1..];
    }
}
