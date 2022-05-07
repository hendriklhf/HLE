using System;

namespace HLE.Twitch.Args;

public class LeftChannelArgs : EventArgs
{
    public string Username { get; }

    public string Channel { get; }

    public LeftChannelArgs(string ircMessage)
    {
        Username = ircMessage.TakeBetween(':', '!');
        Channel = ircMessage.Split()[2][1..];
    }
}
