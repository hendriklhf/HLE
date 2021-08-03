using System.Collections.Generic;

namespace HLETwitchClient.ChatClient
{
    public class ClientOptions
    {
        public string Username { get; }

        public string OAuthToken { get; }

        public List<string> Channels { get; }

        public ClientOptions(string username, string token, List<string> channels)
        {
            Username = username;
            OAuthToken = token;
            Channels = channels;
        }

        public ClientOptions(string username, string token, string channel) : this(username, token, new List<string>() { channel })
        {
        }
    }
}
