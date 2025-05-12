using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using HLE.Threading;

namespace HLE.Twitch.Tmi;

internal sealed class BackgroundWebSocketClient : IAsyncDisposable
{
    public WebSocketState State => _websocket.State;

    private ClientWebSocket _websocket;
    private readonly ChannelWriter<Bytes> _outgoingWriter;
    private readonly ChannelReader<Bytes> _outgoingReader;
    private readonly ChannelWriter<Bytes> _ingoingWriter;
    private readonly ChannelReader<Bytes> _ingoingReader;
    private readonly Uri _uri;
    private readonly SemaphoreSlim _exceptionHandlingLock;
    private readonly ManualResetEventSlim _maintenanceSignal;
    private Task _senderTask;
    private Task _listenerTask;
    private CancellationTokenSource _cts;

    private Func<object, Task>? _afterReconnectionAction;
    private object? _afterReconnectionState;

    private static ReadOnlySpan<byte> NewLine => "\r\n"u8;

    public BackgroundWebSocketClient(Uri uri)
    {
        _websocket = new();
        _uri = uri;
        _cts = new();
        _exceptionHandlingLock = new(1);
        _maintenanceSignal = new(true);

        Channel<Bytes> outgoingChannel = System.Threading.Channels.Channel.CreateUnbounded<Bytes>(new()
        {
            SingleReader = true
        });

        _outgoingWriter = outgoingChannel.Writer;
        _outgoingReader = outgoingChannel.Reader;

        Channel<Bytes> ingoingChannel = System.Threading.Channels.Channel.CreateUnbounded<Bytes>(new()
        {
            SingleWriter = true
        });

        _ingoingWriter = ingoingChannel.Writer;
        _ingoingReader = ingoingChannel.Reader;

        _listenerTask = Task.CompletedTask;
        _senderTask = Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await CancelTasksAsync().ConfigureAwait(false);

        _cts.Dispose();
        _exceptionHandlingLock.Dispose();
        _maintenanceSignal.Dispose();
        _websocket.Dispose();
    }

    public async Task ConnectAsync()
    {
        if (_websocket.State == WebSocketState.Open)
        {
            return;
        }

        if (_websocket.State != WebSocketState.None)
        {
            _websocket.Dispose();
            _websocket = new();
        }

        await _websocket.ConnectAsync(_uri, CancellationToken.None).ConfigureAwait(false);

        if (_cts.IsCancellationRequested)
        {
            _cts.Dispose();
            _cts = new();
        }

        _listenerTask = BackgroundListeningAsync(this, _cts.Token);
        _senderTask = BackgroundSendingAsync(this, _cts.Token);
    }

    public async Task DisconnectAsync()
    {
        await CancelTasksAsync().ConfigureAwait(false);
        await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Manual close.", CancellationToken.None).ConfigureAwait(false);
    }

    public async Task ReconnectAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        await ConnectAsync().ConfigureAwait(false);
    }

    public ValueTask SendAsync(Bytes bytes)
    {
        WaitForMaintenanceCompletion(_cts.Token);
        return _outgoingWriter.WriteAsync(bytes);
    }

    public ValueTask SendEvenWhenInMaintenanceAsync(ReadOnlyMemory<byte> message)
    {
        // TODO: with this approach, when there is a connection exception while connecting and joining channels,
        // the client will be able to write message even when in maintenance mode
        Bytes bytes = new(message.Span);
        return _outgoingWriter.WriteAsync(bytes);
    }

    public ValueTask<Bytes> ReceiveAsync(CancellationToken stoppingToken = default) => _ingoingReader.ReadAsync(stoppingToken);

    public async Task WaitForPendingOutgoingMessagesAsync()
    {
        SpinWait spinWait = new();
        while (DoPendingMessagesExist(_outgoingReader))
        {
            if (spinWait.NextSpinWillYield)
            {
                spinWait.Reset();
                await Task.Yield();
            }
            else
            {
                spinWait.SpinOnce();
            }
        }

        static bool DoPendingMessagesExist(ChannelReader<Bytes> reader)
        {
            try
            {
                if (reader.CanCount)
                {
                    return reader.Count != 0;
                }

                if (reader.CanPeek)
                {
                    return reader.TryPeek(out _);
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }
    }

    public void RegisterAfterAutomaticReconnectionEvent<T>(Func<object, Task> action, T state) where T : class
    {
        _afterReconnectionAction = action;
        _afterReconnectionState = state;
    }

    [SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks")]
    private async Task CancelTasksAsync()
    {
        Task cancellation = _cts.CancelAsync();
        await Task.WhenAll(cancellation, _senderTask, _listenerTask).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    private async Task HandleConnectionExceptionAsync()
    {
        await _exceptionHandlingLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_websocket.State is WebSocketState.Open or WebSocketState.Connecting)
            {
                return;
            }

            _maintenanceSignal.Reset();

            // TODO: maybe don't clear the messages, rather stash them and send them after reconnecting
            await CancelTasksAsync().ConfigureAwait(false);
            while (_outgoingReader.TryRead(out Bytes bytes))
            {
                bytes.Dispose();
            }

            TimeSpan sleep = TimeSpan.FromSeconds(5);

            while (Volatile.Read(ref _websocket).State != WebSocketState.Open)
            {
                await CancelTasksAsync().ConfigureAwait(false);

                try
                {
                    await ConnectAsync().ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is WebSocketException or InvalidOperationException)
                {
                    await Task.Delay(sleep).ConfigureAwait(false);
                    IncreaseSleep(ref sleep);
                }
            }

            Task? action = _afterReconnectionAction?.Invoke(_afterReconnectionState!);
            if (action is not null)
            {
                await action.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
            }
        }
        finally
        {
            _maintenanceSignal.Set();
            _exceptionHandlingLock.Release();
        }

        static void IncreaseSleep(ref TimeSpan sleep)
        {
            sleep *= 1.25;
            if (sleep > TimeSpan.FromMinutes(2))
            {
                sleep = TimeSpan.FromMinutes(2);
            }
        }
    }

    private void WaitForMaintenanceCompletion(CancellationToken stoppingToken) => _maintenanceSignal.Wait(stoppingToken);

    private static async Task BackgroundSendingAsync(BackgroundWebSocketClient client, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using Bytes bytes = await client._outgoingReader.ReadAsync(stoppingToken).ConfigureAwait(false);

            client.WaitForMaintenanceCompletion(stoppingToken);

            try
            {
                await client._websocket.SendAsync(bytes.AsMemory(), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is WebSocketException or InvalidOperationException)
            {
                client.HandleConnectionExceptionAsync().Ignore();
                return;
            }
        }
    }

    private static async Task BackgroundListeningAsync(BackgroundWebSocketClient client, CancellationToken stoppingToken)
    {
        int writtenBufferCount = 0;
        Memory<byte> buffer = new byte[4096];
        while (!stoppingToken.IsCancellationRequested)
        {
            ValueWebSocketReceiveResult receiveResult;
            try
            {
                receiveResult = await client._websocket.ReceiveAsync(buffer[writtenBufferCount..], stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is WebSocketException or InvalidOperationException)
            {
                client.HandleConnectionExceptionAsync().Ignore();
                return;
            }

            if (receiveResult.Count == 0)
            {
                continue;
            }

            ReadOnlyMemory<byte> bytes = buffer[..(writtenBufferCount + receiveResult.Count)];
            if (receiveResult.EndOfMessage)
            {
                await PassAllLinesAsync(client._ingoingWriter, bytes).ConfigureAwait(false);
                writtenBufferCount = 0;
                continue;
            }

            bytes = await PassAllLinesExceptLastAsync(client._ingoingWriter, bytes).ConfigureAwait(false);

            // "bytes" now only contains left-over bytes, because the last received message didn't end with a new line.
            // left-over bytes will be handled in the next loop iteration when a new line has been received.
            bytes.Span.CopyTo(buffer.Span);
            writtenBufferCount = bytes.Length;
        }
    }

    private static async ValueTask<ReadOnlyMemory<byte>> PassAllLinesExceptLastAsync(ChannelWriter<Bytes> writer, ReadOnlyMemory<byte> receivedBytes)
    {
        // TODO: improve slicing
        int indexOfLineEnding = receivedBytes.Span.IndexOf(NewLine);
        while (indexOfLineEnding >= 0)
        {
            ReadOnlyMemory<byte> line = receivedBytes[..indexOfLineEnding];
            Bytes bytes = new(line.Span);
            await writer.WriteAsync(bytes).ConfigureAwait(false);
            receivedBytes = receivedBytes[(indexOfLineEnding + NewLine.Length)..];
            indexOfLineEnding = receivedBytes.Span.IndexOf(NewLine);
        }

        return receivedBytes;
    }

    private static async ValueTask PassAllLinesAsync(ChannelWriter<Bytes> writer, ReadOnlyMemory<byte> receivedBytes)
    {
        // TODO: improve slicing
        do
        {
            int indexOfLineEnding = receivedBytes.Span.IndexOf(NewLine);
            ReadOnlySpan<byte> line = receivedBytes.Span[..indexOfLineEnding];
            Bytes data = new(line);
            await writer.WriteAsync(data).ConfigureAwait(false);
            receivedBytes = receivedBytes[(indexOfLineEnding + NewLine.Length)..];
        }
        while (receivedBytes.Length != 0);
    }
}
