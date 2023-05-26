using System;

namespace HLE.Twitch;

public sealed class AnonymousClientException : Exception
{
    public AnonymousClientException() : base("The client is connected anonymously, therefore cannot send messages.")
    {
    }
}
