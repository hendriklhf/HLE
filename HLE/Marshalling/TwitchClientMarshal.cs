using System;
using System.Runtime.InteropServices;
using HLE.Twitch;

namespace HLE.Marshalling;

public static class TwitchClientMarshal
{
    public static IrcHandler GetIrcHandler(TwitchClient client)
    {
        return client._ircHandler;
    }

    public static WebSocketIrcClient GetWebSocketIrcClient(TwitchClient client)
    {
        return client._client;
    }

    public static ReadOnlySpan<string> GetConnectedChannels(TwitchClient client)
    {
        return CollectionsMarshal.AsSpan(client._ircChannels);
    }
}
