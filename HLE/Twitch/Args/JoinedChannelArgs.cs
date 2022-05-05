using System;

namespace HLE.Twitch.Args;

public class JoinedChannelArgs : EventArgs
{
    public string Username { get; }

    public string Channel { get; }

    public JoinedChannelArgs(string ircMessage)
    {
        string[] split = ircMessage.Split();
        Username = split[0].TakeBetween(':', '!');
        Channel = split[^1][1..];
    }
}
