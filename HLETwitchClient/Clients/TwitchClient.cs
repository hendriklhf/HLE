using HLETwitchClient.Extensions;
using System;
using System.Collections.ObjectModel;

namespace HLETwitchClient.Clients
{
    public class TwitchClient
    {
        public string Username { get; }

        public string OAuthToken { get; }

        public ReadOnlyCollection<string> Channels { get; }

        public IrcClient IrcClient { get; }

        public bool IsConnected => IrcClient.WebSocketIsOpen;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler OnReconnected;
        public event EventHandler OnChatMessageReceived;
        public event EventHandler OnChatMessageSent;

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
                OnConnected?.Invoke(this, new());
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                IrcClient.Disconnect();
                OnDisconnected?.Invoke(this, new());
            }
        }

        public void Reconnect()
        {
            if (IsConnected)
            {
                IrcClient.Reconnect();
                OnReconnected?.Invoke(this, new());
            }
        }

        public void SendMessage(string channel, string message)
        {
            if (IsConnected)
            {
                IrcClient.SendChatMessage(channel, message);
                OnChatMessageSent?.Invoke(this, new());
            }
        }

        public void JoinChannel(string channel)
        {
            if (IsConnected)
            {

            }
        }
    }
}
