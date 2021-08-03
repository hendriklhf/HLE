using HLE.Strings;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace HLETwitchClient.ChatClient
{
    public class IrcClient
    {
        public ClientWebSocket ClientWebSocket { get; private set; } = new();

        public bool IsConnected => ClientWebSocket.State == WebSocketState.Open;

        private readonly ClientOptions _options;
        private const string _url = "wss://irc-ws.chat.twitch.tv:443";
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public IrcClient(ClientOptions options)
        {
            _options = options;
        }

        public void Connect()
        {
            OpenWebSocket();
            ConnectToIRC();
            StartListening();
        }

        private async Task SendAsync(string message)
        {
            if (IsConnected)
            {
                await ClientWebSocket.SendAsync(message.Encode(), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
        }

        private void OpenWebSocket()
        {
            ClientWebSocket.ConnectAsync(new(_url), _cancellationTokenSource.Token).Wait();
        }

        private void ConnectToIRC()
        {
            Task.Run(async () => await SendAsync($"PASS {_options.OAuthToken}"));
            Task.Run(async () => await SendAsync($"NICK {_options.Username}"));
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
            });
        }
    }
}
