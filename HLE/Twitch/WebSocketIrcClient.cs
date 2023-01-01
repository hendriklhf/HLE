using System;
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

    /// <summary>
    /// The default constructor of <see cref="WebSocketIrcClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    public WebSocketIrcClient(string username, string? oAuthToken = null) : base(username, oAuthToken)
    {
    }

    private protected override async ValueTask Send(string message)
    {
        byte[] rentedArray = _byteArrayPool.Rent(1024);
        try
        {
            Memory<byte> bytes = rentedArray;
            int byteCount = Encoding.UTF8.GetBytes(message, bytes.Span);
            bytes = bytes[..byteCount];
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _token);
            InvokeDataSent(this, message);
        }
        finally
        {
            _byteArrayPool.Return(rentedArray);
        }
    }

    private protected override async ValueTask Send(ReadOnlyMemory<char> message)
    {
        byte[] rentedArray = _byteArrayPool.Rent(1024);
        try
        {
            Memory<byte> bytes = rentedArray;
            int byteCount = Encoding.UTF8.GetBytes(message.Span, bytes.Span);
            bytes = bytes[..byteCount];
            await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, _token);
            InvokeDataSent(this, message.Span);
        }
        finally
        {
            _byteArrayPool.Return(rentedArray);
        }
    }

    private protected override void StartListening()
    {
        async ValueTask StartListeningLocal()
        {
            Memory<byte> buffer = new byte[1024];
            Memory<char> charBuffer = new char[1024];
            Memory<Range> rangeBuffer = new Range[256];
            while (IsConnected && !_token.IsCancellationRequested)
            {
                ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, _token);
                if (result.Count == 0)
                {
                    continue;
                }

                ReadOnlyMemory<byte> bytes = buffer[..(result.Count - 2)];
                int count = Encoding.UTF8.GetChars(bytes.Span, charBuffer.Span);
                ReadOnlyMemory<char> chars = charBuffer[..count];
                int rangesLength = chars.Span.GetRangesOfSplit(_newLine, rangeBuffer.Span);
                ReadOnlyMemory<Range> ranges = rangeBuffer[..rangesLength];
                for (int i = 0; i < rangesLength; i++)
                {
                    InvokeDataReceived(this, chars[ranges.Span[i]]);
                }
            }
        }

        Task.Run(StartListeningLocal, _token);
    }

    private protected override async ValueTask ConnectClient()
    {
        await _webSocket.ConnectAsync(new($"{_url.Url}:{_url.Port}"), _token);
    }

    private protected override async ValueTask DisconnectClient(string closeMessage)
    {
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeMessage, _token);
    }

    private protected override (string Url, int Port) GetUrl()
    {
        return UseSSL ? ("wss://irc-ws.chat.twitch.tv", 443) : ("ws://irc-ws.chat.twitch.tv", 80);
    }
}
