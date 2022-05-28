using System;

namespace HLE.Twitch.Args;

/// <summary>
/// <see cref="EventArgs"/> used when a user joined a channel.
/// </summary>
public class JoinedChannelArgs : EventArgs
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
    /// The basic constructor for <see cref="JoinedChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    public JoinedChannelArgs(string ircMessage)
    {
        string[] split = ircMessage.Split();
        Username = split[0].TakeBetween(':', '!');
        Channel = split[^1][1..];
    }
}
