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
        await Send(message.AsMemory());
    }

    private protected override async ValueTask Send(ReadOnlyMemory<char> message)
    {
        byte[] rentedArray = _byteArrayPool.Rent(2048);
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
            Memory<byte> buffer = new byte[2048];
            Memory<char> charBuffer = new char[4096];
            int charBufferLength = 0;
            Memory<Range> rangeBuffer = new Range[512];
            while (IsConnected && !_token.IsCancellationRequested)
            {
                ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, _token);
                if (result.Count == 0)
                {
                    continue;
                }

                int count = Encoding.UTF8.GetChars(buffer.Span[..result.Count], charBuffer.Span[charBufferLength..]);
                ReadOnlyMemory<char> chars = charBuffer[..(count + charBufferLength)];
                int rangesLength = chars.Span.GetRangesOfSplit(_newLine, rangeBuffer.Span);
                bool isEndOfMessage = chars.Span[^2] == _newLine[0] && chars.Span[^1] == _newLine[1];
                if (isEndOfMessage)
                {
                    for (int i = 0; i < rangesLength; i++)
                    {
                        InvokeDataReceived(this, chars[rangeBuffer.Span[i]]);
                    }

                    charBufferLength = 0;
                    continue;
                }

                rangesLength--;
                for (int i = 0; i < rangesLength; i++)
                {
                    InvokeDataReceived(this, chars[rangeBuffer.Span[i]]);
                }

                ReadOnlyMemory<char> lastPart = chars[rangeBuffer.Span[rangesLength]];
                if (lastPart.Length < 3)
                {
                    continue;
                }

                lastPart.CopyTo(charBuffer);
                charBufferLength = lastPart.Length;
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
