using System;

namespace HLE.Twitch;

public sealed class ClientNotConnectedException : Exception
{
    public ClientNotConnectedException() : base("The client is not connected.")
    {
    }
}
