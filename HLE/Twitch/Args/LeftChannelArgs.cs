using System;

namespace HLE.Twitch.Args;

/// <summary>
/// <see cref="EventArgs"/> used whe a user left a channel.
/// </summary>
public class LeftChannelArgs : EventArgs
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
    /// The basic constructor for <see cref="LeftChannelArgs"/>.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    public LeftChannelArgs(string ircMessage)
    {
        Username = ircMessage.TakeBetween(':', '!');
        Channel = ircMessage.Split()[2][1..];
    }
}
