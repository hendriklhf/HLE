using System;

namespace HLE.Twitch.Args;

internal class PingArgs : EventArgs
{
    internal string Message { get; }

    internal PingArgs(string message)
    {
        Message = message;
    }
}
