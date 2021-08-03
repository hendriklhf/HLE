using System;
using System.Collections.ObjectModel;

namespace HLETwitchClient.ChatClient
{
    public class TwitchClient
    {
        public string Username { get; }

        public string OAuthToken { get; }

        public ReadOnlyCollection<string> Channels { get; }


        public IrcClient IrcClient { get; }

        public bool IsConnected => IrcClient.IsConnected;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler OnReconnected;

        private readonly ClientOptions _options;

        public TwitchClient(ClientOptions options)
        {
            _options = options;
            IrcClient = new(_options);
            Username = _options.Username;
            OAuthToken = _options.OAuthToken;
            Channels = new(_options.Channels);
        }

        public void Connect()
        {
            if (!IsConnected)
            {
                IrcClient.Connect();
                OnConnected.Invoke(this, new());
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                OnDisconnected.Invoke(this, new());
            }
        }

        public void Reconnect()
        {
            if (IsConnected)
            {
                OnReconnected.Invoke(this, new());
            }
        }
    }
}
