using System.Diagnostics.Contracts;
using HLE.Twitch.Tmi;

namespace HLE.Twitch.Marshalling;

public static class TwitchClientMarshal
{
    [Pure]
    public static IrcHandler GetIrcHandler(TwitchClient client) => client._ircHandler;

    [Pure]
    public static WebSocketIrcClient GetWebSocketIrcClient(TwitchClient client) => client._client;
}
