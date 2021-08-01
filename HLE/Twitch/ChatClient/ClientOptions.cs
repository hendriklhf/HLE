using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLE.Twitch.ChatClient
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
