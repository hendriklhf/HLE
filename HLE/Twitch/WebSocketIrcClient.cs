using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="ClientWebSocket"/>.
/// </summary>
public sealed class WebSocketIrcClient : IrcClient
{
    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public override bool IsConnected => _webSocket.State is WebSocketState.Open && !_token.IsCancellationRequested;

    private readonly ClientWebSocket _webSocket = new();

    private const string _newLine = "\r\n";

    /// <summary>
    /// The default constructor of <see cref="WebSocketIrcClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    public WebSocketIrcClient(string username, string? oAuthToken = null) : base(username, oAuthToken)
    {
    }

    ~WebSocketIrcClient()
    {
        _webSocket.Dispose();
    }

    private protected override async Task Send(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _token);
        InvokeDataSent(this, message);
    }

    private protected override async Task Send(ReadOnlyMemory<char> message)
    {
        ReadOnlySequence<char> sequence = new(message);
        byte[] bytes = Encoding.UTF8.GetBytes(sequence);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _token);
        InvokeDataSent(this, message.Span);
    }

    private protected override void StartListening()
    {
        async Task StartListeningLocal()
        {
            Memory<byte> buffer = new byte[1024];
            Memory<char> chars = new char[1024];
            while (!_token.IsCancellationRequested && IsConnected)
            {
                ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, _token);
                if (result.Count == 0)
                {
                    continue;
                }

                ReadOnlyMemory<byte> bytes = buffer[..(result.Count - 2)];
                int count = Encoding.UTF8.GetChars(bytes.Span, chars.Span);
                ReadOnlyMemory<Range> charsRanges = ((ReadOnlySpan<char>)chars.Span[..count]).GetRangesOfSplit(_newLine);
                for (int i = 0; i < charsRanges.Length; i++)
                {
                    InvokeDataReceived(this, chars[charsRanges.Span[i]]);
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
