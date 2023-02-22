using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="ClientWebSocket"/>.
/// This class does not validate input, the overlying <see cref="TwitchClient"/> does that.
/// </summary>
public sealed class WebSocketIrcClient : IrcClient, IEquatable<WebSocketIrcClient>
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
        StartListeningAsync();
    }

    private async ValueTask StartListeningAsync()
    {
        Memory<byte> byteBuffer = new byte[4096];
        Memory<char> charBuffer = new char[4096];
        int bufferLength = 0;
        while (IsConnected)
        {
            ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(byteBuffer, default);
            if (result.Count == 0)
            {
                continue;
            }

            ReadOnlyMemory<byte> receivedBytes = byteBuffer[..result.Count];
            int charCount = Encoding.UTF8.GetChars(receivedBytes.Span, charBuffer.Span[bufferLength..]);
            ReadOnlyMemory<char> receivedChars = charBuffer[..(bufferLength + charCount)];

            bool isEndOfMessage = receivedChars.Span[^2] == _newLine[0] && receivedChars.Span[^1] == _newLine[1];
            if (isEndOfMessage)
            {
                PassAllLines(receivedChars);
                bufferLength = 0;
                continue;
            }

            PassAllLinesExceptLast(ref receivedChars);
            receivedChars.Span.CopyTo(charBuffer.Span);
            bufferLength = receivedChars.Length;
        }
    }

    private void PassAllLinesExceptLast(ref ReadOnlyMemory<char> receivedChars)
    {
        int indexOfLineEnding = receivedChars.Span.IndexOf(_newLine);
        while (indexOfLineEnding > -1)
        {
            var lineOfData = receivedChars[..indexOfLineEnding];
            InvokeDataReceived(this, ReceivedData.Create(lineOfData.Span));
            receivedChars = receivedChars[(indexOfLineEnding + _newLine.Length)..];
            indexOfLineEnding = receivedChars.Span.IndexOf(_newLine);
        }
    }

    private void PassAllLines(ReadOnlyMemory<char> receivedChars)
    {
        while (receivedChars.Length > 2)
        {
            int indexOfLineEnding = receivedChars.Span.IndexOf(_newLine);
            ReadOnlyMemory<char> lineOfData = receivedChars[..indexOfLineEnding];
            InvokeDataReceived(this, ReceivedData.Create(lineOfData.Span));
            receivedChars = receivedChars[(indexOfLineEnding + _newLine.Length)..];
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

    public bool Equals(WebSocketIrcClient? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_webSocket, Username, _url, _oAuthToken);
    }
}
