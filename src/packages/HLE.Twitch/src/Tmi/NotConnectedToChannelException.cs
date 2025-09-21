using System;

namespace HLE.Twitch.Tmi;

public sealed class NotConnectedToChannelException : Exception
{
    public NotConnectedToChannelException(string channel) : base($"The client is not connected to the channel \"{channel}\".")
    {
    }

    public NotConnectedToChannelException(long channelId) : base($"The client is not connected to the channel with id \"{channelId}\".")
    {
    }
}
