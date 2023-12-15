using System;
using System.Diagnostics.Contracts;
using HLE.Twitch.Tmi;

namespace HLE.Twitch.Marshalling;

public static class TwitchClientMarshal
{
    [Pure]
    public static IrcHandler GetIrcHandler(TwitchClient client) => client._ircHandler;

    [Pure]
    public static WebSocketIrcClient GetWebSocketIrcClient(TwitchClient client) => client._client;

    [Pure]
    public static ReadOnlySpan<string> GetConnectedChannels(TwitchClient client) => client._ircChannels.AsSpan();
}
