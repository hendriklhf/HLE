using HLE.Strings;
using HLETwitchClient.Extensions;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace HLETwitchClient.Clients
{
    public class IrcClient
    {
        public ClientWebSocket ClientWebSocket { get; private set; } = new();

        public bool WebSocketIsOpen => ClientWebSocket.State == WebSocketState.Open;

        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler OnReconnected;
        public event EventHandler OnMessageSent;
        public event EventHandler OnMessageReceived;

        private readonly ClientOptions _options;
        private const string _url = "wss://irc-ws.chat.twitch.tv:443";
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public IrcClient(ClientOptions options)
        {
            _options = options;
        }

        public void Connect()
        {
            if (!WebSocketIsOpen)
            {
                OpenWebSocket();
                ConnectToIRC();
                StartReceiving();
                OnConnected?.Invoke(this, new());
            }
        }

        public void Disconnect()
        {
            if (WebSocketIsOpen)
            {
                Task.Run(async () => await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Manually closed.", _cancellationTokenSource.Token));
                OnDisconnected?.Invoke(this, new());
            }
        }

        public void Reconnect()
        {
            if (WebSocketIsOpen)
            {
                Disconnect();
                Connect();
                OnReconnected?.Invoke(this, new());
            }
        }

        public void Send(string message)
        {
            Task.Run(async () => await SendAsync(message));
        }

        private async Task SendAsync(string message)
        {
            if (WebSocketIsOpen)
            {
                await ClientWebSocket.SendAsync(message.Encode(), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                OnMessageSent?.Invoke(this, new());
            }
        }

        private void OpenWebSocket()
        {
            if (!WebSocketIsOpen)
            {
                ClientWebSocket.ConnectAsync(new(_url), _cancellationTokenSource.Token).Wait();
            }
        }

        private void ConnectToIRC()
        {
            if (WebSocketIsOpen)
            {
                this.SendOAuthToken(_options.OAuthToken);
                this.SendUsername(_options.Username);
            }
        }

        private void StartReceiving()
        {
            Task.Run(async () =>
            {
                while (WebSocketIsOpen)
                {
                    byte[] buffer = new byte[1024];
                    WebSocketReceiveResult result = await ClientWebSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine(buffer.Decode());
                        OnMessageReceived?.Invoke(this, new());
                    }
                }
            });
        }
    }
}
