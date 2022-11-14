using System;

namespace HLE.Twitch.Models;

internal sealed class PingArgs : EventArgs
{
    internal string Message { get; }

    internal PingArgs(string message)
    {
        Message = message;
    }
}
