using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="ClientWebSocket"/>.
/// </summary>
public sealed class WebSocketIrcClient : IrcClient
{
    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public override bool IsConnected => _webSocket.State is WebSocketState.Open;

    private readonly ClientWebSocket _webSocket = new();

    /// <summary>
    /// The default constructor of <see cref="WebSocketIrcClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    public WebSocketIrcClient(string username, string? oAuthToken = null, ClientOptions options = default) : base(username, oAuthToken, options)
    {
    }

    private protected override async ValueTask Send(ReadOnlyMemory<char> message)
    {
        byte[] rentedArray = ArrayPool<byte>.Shared.Rent(message.Length << 1);
        try
        {
            Memory<byte> bytes = rentedArray;
            int byteCount = Encoding.UTF8.GetBytes(message.Span, bytes.Span);
            await _webSocket.SendAsync(bytes[..byteCount], WebSocketMessageType.Text, true, default);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }

    private protected override void StartListening()
    {
        async ValueTask StartListeningAsync()
        {
            Memory<byte> byteBuffer = new byte[2048];
            Memory<char> charBuffer = new char[4096];
            int bufferLength = 0;
            Memory<Range> rangeBuffer = new Range[512];
            while (IsConnected)
            {
                ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(byteBuffer, default);
                if (result.Count == 0)
                {
                    continue;
                }

                int count = Encoding.UTF8.GetChars(byteBuffer.Span[..result.Count], charBuffer.Span[bufferLength..]);
                ReadOnlyMemory<char> chars = charBuffer[..(count + bufferLength)];
                int rangesLength = chars.Span.GetRangesOfSplit(_newLine, rangeBuffer.Span);
                bool isEndOfMessage = chars.Span[^2] == _newLine[0] && chars.Span[^1] == _newLine[1];
                if (isEndOfMessage)
                {
                    for (int i = 0; i < rangesLength; i++)
                    {
                        ReadOnlyMemory<char> charSpan = chars[rangeBuffer.Span[i]];
                        if (charSpan.Length > 0)
                        {
                            InvokeDataReceived(this, ReceivedData.Create(charSpan.Span));
                        }
                    }

                    bufferLength = 0;
                    continue;
                }

                rangesLength--;
                for (int i = 0; i < rangesLength; i++)
                {
                    ReadOnlyMemory<char> charSpan = chars[rangeBuffer.Span[i]];
                    if (charSpan.Length > 0)
                    {
                        InvokeDataReceived(this, ReceivedData.Create(charSpan.Span));
                    }
                }

                ReadOnlyMemory<char> lastPart = chars[rangeBuffer.Span[rangesLength]];
                if (lastPart.Length < 3)
                {
                    continue;
                }

                lastPart.Span.CopyTo(charBuffer.Span);
                bufferLength = lastPart.Length;
            }
        }

        Task.Run(StartListeningAsync);
    }

    private protected override async ValueTask ConnectClient()
    {
        await _webSocket.ConnectAsync(new($"{_url.Url}:{_url.Port}"), default);
    }

    private protected override async ValueTask DisconnectClient(string closeMessage)
    {
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeMessage, default);
    }

    private protected override (string Url, int Port) GetUrl()
    {
        return UseSSL ? ("wss://irc-ws.chat.twitch.tv", 443) : ("ws://irc-ws.chat.twitch.tv", 80);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public override void Dispose()
    {
        _webSocket.Dispose();
    }
}
