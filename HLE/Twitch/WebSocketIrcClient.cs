using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="ClientWebSocket"/>.
/// This class does not validate input, the overlying <see cref="TwitchClient"/> does that.
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
        using RentedArray<byte> byteBuffer = ArrayPool<byte>.Shared.Rent(message.Length << 1);
        int byteCount = Encoding.UTF8.GetBytes(message.Span, byteBuffer);
        await Send(byteBuffer.Memory[..byteCount]);
    }

    private protected override async ValueTask Send(ReadOnlyMemory<byte> message)
    {
        await _webSocket.SendAsync(message, WebSocketMessageType.Text, true, default);
    }

    private protected override void StartListening()
    {
        StartListeningAsync();
    }

    private async ValueTask StartListeningAsync()
    {
        Memory<byte> byteBuffer = new byte[2048];
        int bufferLength = 0;
        while (IsConnected)
        {
            ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(byteBuffer[bufferLength..], default);
            if (result.Count == 0)
            {
                continue;
            }

            ReadOnlyMemory<byte> receivedBytes = byteBuffer[..(result.Count + bufferLength)];
            bool isEndOfMessage = receivedBytes.Span[^2] == _newLine[0] && receivedBytes.Span[^1] == _newLine[1];

            if (isEndOfMessage)
            {
                PassAllLines(receivedBytes);
                continue;
            }

            PassAllLinesExceptLast(ref receivedBytes);
            receivedBytes.Span.CopyTo(byteBuffer.Span);
            bufferLength = receivedBytes.Length;
        }
    }

    private void PassAllLinesExceptLast(ref ReadOnlyMemory<byte> receivedBytes)
    {
        int indexOfLineEnding = receivedBytes.Span.IndexOf(_newLine);
        while (indexOfLineEnding > -1)
        {
            ReadOnlyMemory<byte> lineOfData = receivedBytes[..indexOfLineEnding];
            InvokeDataReceived(this, ReceivedData.Create(lineOfData.Span));
            receivedBytes = receivedBytes[(indexOfLineEnding + _newLine.Length)..];
            indexOfLineEnding = receivedBytes.Span.IndexOf(_newLine);
        }
    }

    private void PassAllLines(ReadOnlyMemory<byte> receivedBytes)
    {
        while (receivedBytes.Length > 2)
        {
            int indexOfLineEnding = receivedBytes.Span.IndexOf(_newLine);
            receivedBytes = receivedBytes[(indexOfLineEnding + 2)..];
            ReadOnlyMemory<byte> lineOfData = receivedBytes[..indexOfLineEnding];
            InvokeDataReceived(this, ReceivedData.Create(lineOfData.Span));
        }
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
