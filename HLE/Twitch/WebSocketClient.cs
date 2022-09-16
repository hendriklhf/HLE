using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="ClientWebSocket"/>.
/// </summary>
public sealed class WebSocketClient : IrcClient
{
    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public override bool IsConnected => _webSocket.State is WebSocketState.Open && !_token.IsCancellationRequested;

    private readonly ClientWebSocket _webSocket = new();

    /// <summary>
    /// The default constructor of <see cref="WebSocketClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    public WebSocketClient(string username, string? oAuthToken = null) : base(username, oAuthToken)
    {
    }

    private protected override async Task Send(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _token);
        InvokeDataSent(this, message);
    }

    private protected override void StartListening()
    {
        async Task StartListeningLocal()
        {
            while (!_token.IsCancellationRequested && IsConnected)
            {
                Memory<byte> buffer = new(new byte[2048]);
                ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, _token);
                if (result.Count == 0)
                {
                    continue;
                }

                string message = Encoding.UTF8.GetString(buffer[..result.Count].ToArray());
                string[] messages = message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
                foreach (string m in messages)
                {
                    InvokeDataReceived(this, m);
                }
            }
        }

        Task.Run(StartListeningLocal, _token);
    }

    private protected override async Task ConnectClient()
    {
        await _webSocket.ConnectAsync(new($"{_url.Url}:{_url.Port}"), _token);
    }

    private protected override async Task DisconnectClient(string closeMessage)
    {
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeMessage, _token);
    }

    private protected override (string Url, int Port) GetUrl()
    {
        return UseSSL ? ("wss://irc-ws.chat.twitch.tv", 443) : ("ws://irc-ws.chat.twitch.tv", 80);
    }
}
