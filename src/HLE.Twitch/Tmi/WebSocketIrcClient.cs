using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="ClientWebSocket"/>.
/// </summary>
public sealed class WebSocketIrcClient : IEquatable<WebSocketIrcClient>, IDisposable
{
    /// <summary>
    /// The username of the client.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Indicates whether the connection uses SSL or not.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool UseSSL { get; }

    /// <summary>
    /// Is invoked if the client connects to the server.
    /// </summary>
    public event EventHandler? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event EventHandler? OnDisconnected;

    /// <summary>
    /// Is invoked if the client receives data. If this event is subscribed to, the <see cref="ReceivedData"/> instance has to be manually disposed.
    /// </summary>
    public event EventHandler<ReceivedData>? OnDataReceived;

    internal event EventHandler? OnConnectionException;

    /// <summary>
    /// Gets the state of the websocket connection.
    /// </summary>
    public WebSocketState State => _webSocket.State;

    private ClientWebSocket _webSocket = new();
    private readonly bool _isVerifiedBot;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly OAuthToken _oAuthToken;
    private readonly Uri _connectionUri;

    private static readonly Uri s_sslConnectionUri = new("wss://irc-ws.chat.twitch.tv:443");
    private static readonly Uri s_nonSslConnectionUri = new("ws://irc-ws.chat.twitch.tv:80");

    private const string NewLine = "\r\n";
    private const string PassPrefix = "PASS ";
    private const string NickPrefix = "NICK ";
    private const string CapReqMessage = "CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership";
    private const string PrivMsgPrefix = "PRIVMSG ";
    private const string JoinPrefix = "JOIN ";
    private const string PartPrefix = "PART ";
    private const byte MaximumChannelNameLength = 26; // 25 for the name + 1 for the '#'
    private const ushort MaximumMessageLength = 500;

    /// <summary>
    /// The default constructor of <see cref="WebSocketIrcClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    public WebSocketIrcClient(string username, OAuthToken oAuthToken, ClientOptions options)
    {
        Username = username;
        _oAuthToken = oAuthToken;
        UseSSL = options.UseSSL;
        _isVerifiedBot = options.IsVerifiedBot;
        _connectionUri = UseSSL ? s_sslConnectionUri : s_nonSslConnectionUri;
    }

    private async ValueTask SendAsync(ReadOnlyMemory<char> message)
    {
        using RentedArray<byte> bytes = ArrayPool<byte>.Shared.RentAsRentedArray(message.Length << 1);
        int byteCount = Encoding.UTF8.GetBytes(message.Span, bytes.AsSpan());
        await SendAsync(bytes.AsMemory(..byteCount));
    }

    private async ValueTask SendAsync(ReadOnlyMemory<byte> message)
    {
        try
        {
            await _webSocket.SendAsync(message, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (ex is not (WebSocketException or InvalidOperationException))
            {
                throw;
            }

            HandleConnectionException();
        }
    }

    private void StartListeningThread()
    {
        Thread listeningThread = new(StartListeningAsync)
        {
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal
        };
        listeningThread.Start();
    }

    // ReSharper disable once AsyncVoidMethod
    private async void StartListeningAsync()
    {
        try
        {
            CancellationTokenSource cancellationTokenSource = _cancellationTokenSource;
            ClientWebSocket webSocket = _webSocket;

            Memory<byte> byteBuffer = GC.AllocateUninitializedArray<byte>(4096, true);
            Memory<char> charBuffer = GC.AllocateUninitializedArray<char>(4096, true);
            int bufferLength = 0;
            while (!cancellationTokenSource.IsCancellationRequested && State is WebSocketState.Open)
            {
                ValueWebSocketReceiveResult webSocketReceiveResult = await webSocket.ReceiveAsync(byteBuffer, cancellationTokenSource.Token);
                if (webSocketReceiveResult.Count == 0)
                {
                    continue;
                }

                ReadOnlyMemory<byte> receivedBytes = byteBuffer[..webSocketReceiveResult.Count];
                int charCount = Encoding.UTF8.GetChars(receivedBytes.Span, charBuffer.Span[bufferLength..]);
                ReadOnlyMemory<char> receivedChars = charBuffer[..(bufferLength + charCount)];

                bool isEndOfMessage = receivedChars.Span is [.., '\r', '\n'];
                if (isEndOfMessage)
                {
                    PassAllLines(receivedChars.Span);
                    bufferLength = 0;
                    continue;
                }

                PassAllLinesExceptLast(ref receivedChars);

                // receivedChars now only contains left-over chars, because the last received message didn't end with an new line
                // left-over chars are copied into the charBuffer and will be handled next loop iteration when the new line has been received
                receivedChars.Span.CopyTo(charBuffer.Span);
                bufferLength = receivedChars.Length;
            }
        }
        catch (Exception ex)
        {
            if (ex is not (WebSocketException or InvalidOperationException))
            {
                throw;
            }

            HandleConnectionException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PassAllLinesExceptLast(ref ReadOnlyMemory<char> receivedChars)
    {
        int indexOfLineEnding = receivedChars.Span.IndexOf(NewLine);
        while (indexOfLineEnding >= 0)
        {
            ReadOnlyMemory<char> lineOfData = receivedChars[..indexOfLineEnding];
            ReceivedData receivedData = new(lineOfData.Span);
            InvokeDataReceived(this, in receivedData);
            receivedChars = receivedChars[(indexOfLineEnding + NewLine.Length)..];
            indexOfLineEnding = receivedChars.Span.IndexOf(NewLine);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PassAllLines(ReadOnlySpan<char> receivedChars)
    {
        while (receivedChars.Length > 2)
        {
            int indexOfLineEnding = receivedChars.IndexOf(NewLine);
            ReadOnlySpan<char> lineOfData = receivedChars[..indexOfLineEnding];
            ReceivedData receivedData = new(lineOfData);
            InvokeDataReceived(this, in receivedData);
            receivedChars = receivedChars[(indexOfLineEnding + NewLine.Length)..];
        }
    }

    private async Task ConnectClientAsync()
    {
        try
        {
            await _webSocket.ConnectAsync(_connectionUri, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (ex is not (WebSocketException or InvalidOperationException))
            {
                throw;
            }

            HandleConnectionException();
        }
    }

    private async Task DisconnectClientAsync(string closeMessage)
    {
        try
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeMessage, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (ex is not (WebSocketException or InvalidOperationException))
            {
                throw;
            }

            HandleConnectionException();
        }
    }

    private void HandleConnectionException()
    {
        _webSocket.Dispose();
        _webSocket = new();
        OnConnectionException?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    public async Task ConnectAsync(IEnumerable<string> channels)
    {
        if (channels.TryGetReadOnlyMemory<string>(out ReadOnlyMemory<string> channelsAsMemory))
        {
            await ConnectAsync(channelsAsMemory);
            return;
        }

        // TODO: try other ways before using .ToArray()
        await ConnectAsync(channels.ToArray());
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    // ReSharper disable once InconsistentNaming
    public Task ConnectAsync(string[] channels) => ConnectAsync(channels.AsMemory());

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{string})"/>
    // ReSharper disable once InconsistentNaming
    public Task ConnectAsync(List<string> channels)
        => ConnectAsync(SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(channels)));

    /// <summary>
    /// Asynchronously connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public async Task ConnectAsync(ReadOnlyMemory<string> channels)
    {
        await ConnectClientAsync();
        StartListeningThread();
        OnConnected?.Invoke(this, EventArgs.Empty);

        using PooledStringBuilder messageBuilder = new(CapReqMessage.Length);
        if (_oAuthToken != OAuthToken.Empty)
        {
            messageBuilder.Append(PassPrefix, _oAuthToken.AsSpan());
            await SendAsync(messageBuilder.WrittenMemory);
        }

        messageBuilder.Clear();
        messageBuilder.Append(NickPrefix, Username);
        await SendAsync(messageBuilder.WrittenMemory);

        messageBuilder.Clear();
        messageBuilder.Append(CapReqMessage);
        await SendAsync(messageBuilder.WrittenMemory);

        await JoinChannelsThrottledAsync(channels);
    }

    /// <summary>
    /// Asynchronously disconnects the client.
    /// </summary>
    /// <param name="closeMessage">A close message or reason.</param>
    public async Task DisconnectAsync(string closeMessage = "Manually closed")
    {
        await DisconnectClientAsync(closeMessage);
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }

    internal async Task ReconnectAsync(ReadOnlyMemory<string> channels)
    {
        await DisconnectAsync();
        await ConnectAsync(channels);
    }

    /// <inheritdoc cref="SendRawAsync(ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendRawAsync(string rawMessage) => SendAsync(rawMessage.AsMemory());

    /// <summary>
    /// Asynchronously sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="rawMessage">The IRC message.</param>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendRawAsync(ReadOnlyMemory<char> rawMessage) => SendAsync(rawMessage);

    /// <inheritdoc cref="SendMessageAsync(ReadOnlyMemory{char},ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendMessageAsync(string channel, string message)
        => SendMessageAsync(channel.AsMemory(), message.AsMemory());

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="builder">The builder that contains the message that will be sent.</param>
    // ReSharper disable once InconsistentNaming
    public ValueTask SendMessageAsync(ReadOnlyMemory<char> channel, PooledStringBuilder builder)
        => SendMessageAsync(channel, builder.WrittenMemory);

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public async ValueTask SendMessageAsync(ReadOnlyMemory<char> channel, ReadOnlyMemory<char> message)
    {
        using PooledStringBuilder messageBuilder = new(PrivMsgPrefix.Length + MaximumChannelNameLength + 2 + MaximumMessageLength);
        messageBuilder.Append(PrivMsgPrefix, channel.Span, " :", message.Span);
        await SendAsync(messageBuilder.WrittenMemory);
    }

    /// <inheritdoc cref="JoinChannelAsync(ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask JoinChannelAsync(string channel) => JoinChannelAsync(channel.AsMemory());

    /// <summary>
    /// Asynchronously joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public async ValueTask JoinChannelAsync(ReadOnlyMemory<char> channel)
    {
        using PooledStringBuilder messageBuilder = new(JoinPrefix.Length + MaximumChannelNameLength);
        messageBuilder.Append(JoinPrefix, channel.Span);
        await SendAsync(messageBuilder.WrittenMemory);
    }

    /// <inheritdoc cref="LeaveChannelAsync(ReadOnlyMemory{char})"/>
    // ReSharper disable once InconsistentNaming
    public ValueTask LeaveChannelAsync(string channel) => LeaveChannelAsync(channel.AsMemory());

    /// <summary>
    /// Asynchronously leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public async ValueTask LeaveChannelAsync(ReadOnlyMemory<char> channel)
    {
        using PooledStringBuilder messageBuilder = new(PartPrefix.Length + MaximumChannelNameLength);
        messageBuilder.Append(PartPrefix, channel.Span);
        await SendAsync(messageBuilder.WrittenMemory);
    }

    private async Task JoinChannelsThrottledAsync(ReadOnlyMemory<string> channels)
    {
        if (channels.Length == 0)
        {
            return;
        }

        int isVerifiedBotFactor = _isVerifiedBot ? 1 : 0;
        int maximumJoinsInPeriod = 20 + 180 * isVerifiedBotFactor;
        TimeSpan period = TimeSpan.FromSeconds(10);

        using PooledStringBuilder messageBuilder = new(JoinPrefix.Length + MaximumChannelNameLength);
        DateTimeOffset start = DateTimeOffset.UtcNow;
        for (int i = 0; i < channels.Length && !_cancellationTokenSource.IsCancellationRequested; i++)
        {
            if (i != 0 && i % maximumJoinsInPeriod == 0)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                TimeSpan waitTime = period - (now - start);
                if (waitTime.TotalMilliseconds > 0)
                {
                    await Task.Delay(waitTime, _cancellationTokenSource.Token);
                }

                start = now + waitTime;
            }

            messageBuilder.Append(JoinPrefix, channels.Span[i]);
            await SendAsync(messageBuilder.WrittenMemory);
            messageBuilder.Clear();
        }
    }

    private void InvokeDataReceived(WebSocketIrcClient sender, in ReceivedData data)
    {
        if (OnDataReceived is null)
        {
            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            // doesnt matter, because lifetime of the struct ends here
            data.Dispose();
            return;
        }

        OnDataReceived.Invoke(sender, data);
    }

    internal void CancelTasks()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new();
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        _webSocket.Dispose();
        _cancellationTokenSource.Dispose();
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] WebSocketIrcClient? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(WebSocketIrcClient? left, WebSocketIrcClient? right) => Equals(left, right);

    public static bool operator !=(WebSocketIrcClient? left, WebSocketIrcClient? right) => !(left == right);
}
