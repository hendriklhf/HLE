using System;

namespace HLE.Twitch;

public sealed class NotConnectedToTheChannelException : Exception
{
    public NotConnectedToTheChannelException(string channel) : base($"The client is not connected to the channel \"{channel}\".")
    {
    }

    public NotConnectedToTheChannelException(long channelId) : base($"The client is not connected to the channel with id \"{channelId}\".")
    {
    }
}
