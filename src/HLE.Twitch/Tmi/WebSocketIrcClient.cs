using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
using HLE.Threading;
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
    public bool UseSsl { get; }

    /// <summary>
    /// Is invoked if the client connects to the server.
    /// </summary>
    public event AsyncEventHandler<WebSocketIrcClient>? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event AsyncEventHandler<WebSocketIrcClient>? OnDisconnected;

    /// <summary>
    /// Is invoked if the client receives data. If this event is subscribed to, the <see cref="Bytes"/> instance has to be manually disposed.
    /// </summary>
    public event AsyncEventHandler<WebSocketIrcClient, Bytes>? OnBytesReceived;

    internal event AsyncEventHandler<WebSocketIrcClient, EventArgs>? OnConnectionException;

    /// <summary>
    /// Gets the state of the websocket connection.
    /// </summary>
    public WebSocketState State => _webSocket.State;

    private ClientWebSocket _webSocket = new();
    private readonly bool _isVerifiedBot;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly OAuthToken _oAuthToken;
    private readonly Uri _connectionUri;
    private readonly ImmutableArray<byte> _usernameUtf8;

    private static ReadOnlySpan<byte> PassPrefix => "PASS "u8;

    private static ReadOnlySpan<byte> NickPrefix => "NICK "u8;

    private static ReadOnlySpan<byte> CapReqMessage => "CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership"u8;

    private static ReadOnlySpan<byte> PrivMsgPrefix => "PRIVMSG "u8;

    private static ReadOnlySpan<byte> JoinPrefix => "JOIN "u8;

    private static ReadOnlySpan<byte> PartPrefix => "PART "u8;

    private static ReadOnlySpan<byte> NewLine => "\r\n"u8;

    private static readonly Uri s_sslConnectionUri = new("wss://irc-ws.chat.twitch.tv:443");
    private static readonly Uri s_nonSslConnectionUri = new("ws://irc-ws.chat.twitch.tv:80");

    private const int MaximumChannelNameLength = 26; // 25 for the name + 1 for the '#'
    private const int MaximumMessageLength = 500;

    /// <summary>
    /// The default constructor of <see cref="WebSocketIrcClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    [SuppressMessage("Style", "IDE0290:Use primary constructor")] // TODO: change later
    public WebSocketIrcClient(string username, OAuthToken oAuthToken, ClientOptions options)
    {
        Username = username;
        _usernameUtf8 = ImmutableCollectionsMarshal.AsImmutableArray(Encoding.UTF8.GetBytes(username));
        _oAuthToken = oAuthToken;
        UseSsl = options.UseSsl;
        _isVerifiedBot = options.IsVerifiedBot;
        _connectionUri = options.UseSsl ? s_sslConnectionUri : s_nonSslConnectionUri;
    }

    private async ValueTask SendAsync(ReadOnlyMemory<byte> message)
    {
        try
        {
            await _webSocket.SendAsync(message, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }
        catch (Exception ex) when (ex is WebSocketException or InvalidOperationException)
        {
            await HandleConnectionExceptionAsync();
        }
    }

    private void StartListeningBackgroundTask() => StartListeningAsync().Ignore();

    private async Task StartListeningAsync()
    {
        try
        {
            CancellationToken token = _cancellationTokenSource.Token;
            ClientWebSocket webSocket = _webSocket;

            int writtenBufferCount = 0;
            Memory<byte> buffer = new byte[4096];
            while (webSocket.State is WebSocketState.Open && !token.IsCancellationRequested)
            {
                ValueWebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(buffer[writtenBufferCount..], token);
                if (receiveResult.Count == 0 || OnBytesReceived is null)
                {
                    continue;
                }

                ReadOnlyMemory<byte> bytes = buffer[..(writtenBufferCount + receiveResult.Count)];
                if (receiveResult.EndOfMessage)
                {
                    PassAllLines(bytes.Span);
                    writtenBufferCount = 0;
                    continue;
                }

                PassAllLinesExceptLast(ref bytes);

                // "bytes" now only contains left-over bytes, because the last received message didn't end with a new line.
                // left-over bytes will be handled in the next loop iteration when a new line has been received.
                bytes.Span.CopyTo(buffer.Span);
                writtenBufferCount = bytes.Length;
            }
        }
        catch (Exception ex) when (ex is WebSocketException or InvalidOperationException)
        {
            await HandleConnectionExceptionAsync();
        }
    }

    private void PassAllLinesExceptLast(ref ReadOnlyMemory<byte> receivedBytes)
    {
        int indexOfLineEnding = receivedBytes.Span.IndexOf(NewLine);
        while (indexOfLineEnding >= 0)
        {
            ReadOnlyMemory<byte> line = receivedBytes[..indexOfLineEnding];
            Bytes bytes = new(line.Span);
            InvokeBytesReceived(ref bytes);
            receivedBytes = receivedBytes[(indexOfLineEnding + NewLine.Length)..];
            indexOfLineEnding = receivedBytes.Span.IndexOf(NewLine);
        }
    }

    private void PassAllLines(ReadOnlySpan<byte> receivedBytes)
    {
        do
        {
            int indexOfLineEnding = receivedBytes.IndexOf(NewLine);
            ReadOnlySpan<byte> line = receivedBytes[..indexOfLineEnding];
            Bytes data = new(line);
            InvokeBytesReceived(ref data);
            receivedBytes = receivedBytes[(indexOfLineEnding + NewLine.Length)..];
        }
        while (receivedBytes.Length != 0);
    }

    private async Task ConnectClientAsync()
    {
        try
        {
            await _webSocket.ConnectAsync(_connectionUri, _cancellationTokenSource.Token);
        }
        catch (Exception ex) when (ex is WebSocketException or InvalidOperationException)
        {
            await HandleConnectionExceptionAsync();
        }
    }

    private async Task DisconnectClientAsync()
    {
        try
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Manually closed", _cancellationTokenSource.Token);
        }
        catch (Exception ex) when (ex is WebSocketException or InvalidOperationException)
        {
            await HandleConnectionExceptionAsync();
        }
    }

    private Task HandleConnectionExceptionAsync()
    {
        _webSocket.Dispose();
        _webSocket = new();
        return EventInvoker.InvokeAsync(OnConnectionException, this, EventArgs.Empty);
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{ReadOnlyMemory{byte}})"/>
    public async Task ConnectAsync(IEnumerable<ReadOnlyMemory<byte>> channels)
    {
        if (channels.TryGetReadOnlyMemory(out ReadOnlyMemory<ReadOnlyMemory<byte>> channelsAsMemory))
        {
            await ConnectAsync(channelsAsMemory);
            return;
        }

        await ConnectAsync(channels.ToArray());
    }

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{ReadOnlyMemory{byte}})"/>
    public Task ConnectAsync(ReadOnlyMemory<byte>[] channels) => ConnectAsync(channels.AsMemory());

    /// <inheritdoc cref="ConnectAsync(ReadOnlyMemory{ReadOnlyMemory{byte}})"/>
    public Task ConnectAsync(List<ReadOnlyMemory<byte>> channels) => ConnectAsync(ListMarshal.AsReadOnlyMemory(channels));

    /// <summary>
    /// Asynchronously connects the client to the Twitch IRC server.
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public async Task ConnectAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> channels)
    {
        await ConnectClientAsync();
        StartListeningBackgroundTask();
        await EventInvoker.InvokeAsync(OnConnected, this);

        using PooledBufferWriter<byte> messageBuilder = new(CapReqMessage.Length);
        if (_oAuthToken != OAuthToken.Empty)
        {
            messageBuilder.Write(PassPrefix);
            messageBuilder.WriteUtf8(_oAuthToken.AsSpan());
            await SendAsync(messageBuilder.WrittenMemory);
        }

        messageBuilder.Clear();
        messageBuilder.Write(NickPrefix);
        messageBuilder.Write(_usernameUtf8.AsSpan());
        await SendAsync(messageBuilder.WrittenMemory);

        messageBuilder.Clear();
        messageBuilder.Write(CapReqMessage);
        await SendAsync(messageBuilder.WrittenMemory);

        await JoinChannelsThrottledAsync(channels);
    }

    /// <summary>
    /// Asynchronously disconnects the client.
    /// </summary>
    public async Task DisconnectAsync()
    {
        await DisconnectClientAsync();
        await EventInvoker.InvokeAsync(OnDisconnected, this);
    }

    internal async Task ReconnectAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> channels)
    {
        await DisconnectAsync();
        await ConnectAsync(channels);
    }

    /// <summary>
    /// Asynchronously sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="rawMessage">The IRC message.</param>
    public ValueTask SendRawAsync(ReadOnlyMemory<byte> rawMessage) => SendAsync(rawMessage);

    /// <summary>
    /// Asynchronously sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
    public async ValueTask SendMessageAsync(ReadOnlyMemory<byte> channel, ReadOnlyMemory<byte> message)
    {
        using PooledBufferWriter<byte> messageBuilder = new(PrivMsgPrefix.Length + MaximumChannelNameLength + " :"u8.Length + MaximumMessageLength);
        messageBuilder.Write(PrivMsgPrefix);
        messageBuilder.Write(channel.Span);
        messageBuilder.Write(" :"u8);
        messageBuilder.Write(message.Span);

        await SendAsync(messageBuilder.WrittenMemory);
    }

    /// <summary>
    /// Asynchronously joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public async ValueTask JoinChannelAsync(ReadOnlyMemory<byte> channel)
    {
        using PooledBufferWriter<byte> messageBuilder = new(JoinPrefix.Length + MaximumChannelNameLength);
        messageBuilder.Write(JoinPrefix);
        messageBuilder.Write(channel.Span);

        await SendAsync(messageBuilder.WrittenMemory);
    }

    /// <summary>
    /// Asynchronously leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public async ValueTask LeaveChannelAsync(ReadOnlyMemory<byte> channel)
    {
        using PooledBufferWriter<byte> messageBuilder = new(PartPrefix.Length + MaximumChannelNameLength);
        messageBuilder.Write(PartPrefix);
        messageBuilder.Write(channel.Span);

        await SendAsync(messageBuilder.WrittenMemory);
    }

    private async Task JoinChannelsThrottledAsync(ReadOnlyMemory<ReadOnlyMemory<byte>> channels)
    {
        if (channels.Length == 0)
        {
            return;
        }

        int maximumJoinsInPeriod = _isVerifiedBot ? 200 : 20;
        TimeSpan period = TimeSpan.FromSeconds(10);

        CancellationTokenSource cancellationTokenSource = _cancellationTokenSource;
        using PooledBufferWriter<byte> messageBuilder = new(JoinPrefix.Length + MaximumChannelNameLength);

        DateTimeOffset start = DateTimeOffset.UtcNow;
        for (int i = 0; i < channels.Length; i++)
        {
            if (i != 0 && i % maximumJoinsInPeriod == 0)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                TimeSpan waitTime = period - (now - start);
                if (waitTime.TotalMilliseconds > 0)
                {
                    await Task.Delay(waitTime, cancellationTokenSource.Token);
                }

                start = now + waitTime;
            }

            if (cancellationTokenSource.IsCancellationRequested)
            {
                ThrowHelper.ThrowTaskCancelledException();
            }

            messageBuilder.Write(JoinPrefix);
            messageBuilder.Write(channels.Span[i].Span);
            await SendAsync(messageBuilder.WrittenMemory);
            messageBuilder.Clear();
        }
    }

    private void InvokeBytesReceived(ref Bytes bytes)
    {
        Debug.Assert(OnBytesReceived is not null);
        EventInvoker.InvokeAsync(OnBytesReceived, this, bytes).Ignore();
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
