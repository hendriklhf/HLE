using System;
using System.Diagnostics.Contracts;
using HLE.Twitch;

namespace HLE.Marshalling;

public static class TwitchClientMarshal
{
    [Pure]
    public static IrcHandler GetIrcHandler(TwitchClient client)
    {
        return client._ircHandler;
    }

    [Pure]
    public static WebSocketIrcClient GetWebSocketIrcClient(TwitchClient client)
    {
        return client._client;
    }

    [Pure]
    public static ReadOnlySpan<string> GetConnectedChannels(TwitchClient client)
    {
        return client._ircChannels.AsSpan();
    }
}
