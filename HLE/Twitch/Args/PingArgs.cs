using System;

namespace HLE.Twitch.Args;

public class PingArgs : EventArgs
{
    public string Message { get; }

    public PingArgs(string message)
    {
        Message = message;
    }
}
