using HLETwitchClient.Clients;

namespace HLETwitchClient.Extensions
{
    public static class IrcCommands
    {
        public static void SendOAuthToken(this IrcClient ircClient, string oauth)
        {
            ircClient.Send($"PASS {oauth}");
        }

        public static void SendUsername(this IrcClient ircClient, string username)
        {
            ircClient.Send($"NICK {username}");
        }

        public static void SendJoinChannel(this IrcClient ircClient, string channel)
        {
        }

        public static void SendChatMessage(this IrcClient ircClient, string channel, string message)
        {
        }
    }
}
