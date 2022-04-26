using System;

namespace HLE.Twitch.Args;

public class JoinedChannelArgs : EventArgs
{
    public string Username { get; }

    public string Channel { get; }

    public JoinedChannelArgs(string username, string channel)
    {
        Username = username;
        Channel = channel;
    }
}
