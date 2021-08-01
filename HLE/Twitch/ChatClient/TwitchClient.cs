using HLE.Strings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HLE.Twitch.ChatClient
{
    public class TwitchClient
    {
        public string Username { get; }

        public string OAuthToken { get; }

        public ReadOnlyCollection<string> Channels { get; }

        public ClientOptions ClientOptions { get; }

        public ClientWebSocket ClientWebSocket { get; private set; } = new();

        public bool IsConnected => ClientWebSocket.State == WebSocketState.Open;

        public event EventHandler OnConnected;
        public event EventHandler OnException;
        public event EventHandler OnMessageReceived;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly List<Task> _socketTasks = new();
        private const string _url = "wss://irc-ws.chat.twitch.tv:443";

        public TwitchClient(ClientOptions options)
        {
            ClientOptions = options;
            Username = ClientOptions.Username;
            OAuthToken = ClientOptions.OAuthToken;
            Channels = new(ClientOptions.Channels);
        }

        public void Connect()
        {
            if (!IsConnected)
            {
                ClientWebSocket.ConnectAsync(new(_url), _cancellationTokenSource.Token).Wait();
                OnConnected.Invoke(this, new());
                ConnectToIRC();
                StartListening();
            }
        }

        private async Task SendAsync(byte[] message)
        {
            if (IsConnected)
            {
                await ClientWebSocket.SendAsync(message, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
        }

        private void ConnectToIRC()
        {
            Task.Run(async () => await SendAsync($"PASS {OAuthToken}".Encode()));
            Task.Run(async () => await SendAsync($"NICK {Username}".Encode()));
        }

        private void StartListening()
        {
            Task.Run(async () =>
            {
                while (IsConnected)
                {
                    byte[] buffer = new byte[1024];
                    WebSocketReceiveResult result = await ClientWebSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine(buffer.Decode());
                    }
                }
            }).Wait();
        }
    }
}
