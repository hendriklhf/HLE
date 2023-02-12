#if TCP
using System;
using System.Buffers;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="TcpClient"/>.
/// This class does not validate input, the overlying <see cref="TwitchClient"/> does that.
/// </summary>
public sealed class TcpIrcClient : IrcClient
{
    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public override bool IsConnected => _tcpClient.Connected;

    private readonly TcpClient _tcpClient = new();
    private StreamReader? _reader;
    private StreamWriter? _writer;

    /// <summary>
    /// The default constructor of <see cref="TcpIrcClient"/>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// /// <param name="options">The client options. If null, uses default options that can be found on the documentation of <see cref="ClientOptions"/>.</param>
    public TcpIrcClient(string username, string? oAuthToken = null, ClientOptions options = default) : base(username, oAuthToken, options)
    {
    }

    private protected override async ValueTask Send(ReadOnlyMemory<char> message)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException($"{nameof(_writer)} is null");
        }

        await _writer.WriteAsync(message);
        await _writer.FlushAsync();
    }

    private protected override async ValueTask Send(ReadOnlyMemory<byte> message)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException($"{nameof(_writer)} is null");
        }

        using RentedArray<char> chars = ArrayPool<char>.Shared.Rent(message.Length << 1);
        int charCount = Encoding.UTF8.GetChars(message.Span, chars.Span);
        await Send(chars.Memory[..charCount]);
    }

    private protected override void StartListening()
    {
        async ValueTask StartListeningAsync()
        {
            Memory<char> buffer = new char[4096];
            int bufferLength = 0;
            Memory<Range> rangeBuffer = new Range[512];
            while (IsConnected)
            {
                int count = await _reader.ReadAsync(buffer[bufferLength..]);
                if (count == 0)
                {
                    continue;
                }

                ReadOnlyMemory<char> message = buffer[..(count + bufferLength)];
                int rangesLength = message.Span.GetRangesOfSplit(_newLine, rangeBuffer.Span);
                bool isEndOfMessage = message.Span[^2] == _newLine[0] && message.Span[^1] == _newLine[1];
                if (isEndOfMessage)
                {
                    for (int i = 0; i < rangesLength; i++)
                    {
                        ReadOnlyMemory<char> messageSpan = message[rangeBuffer.Span[i]];
                        if (messageSpan.Length > 0)
                        {
                            InvokeDataReceived(this, ReceivedData.Create(messageSpan.Span));
                        }
                    }

                    bufferLength = 0;
                    continue;
                }

                rangesLength--;
                for (int i = 0; i < rangesLength; i++)
                {
                    ReadOnlyMemory<char> messageSpan = message[rangeBuffer.Span[i]];
                    if (messageSpan.Length > 0)
                    {
                        InvokeDataReceived(this, ReceivedData.Create(messageSpan.Span));
                    }
                }

                ReadOnlyMemory<char> lastPart = message[rangeBuffer.Span[rangesLength]];
                if (lastPart.Length < 3)
                {
                    continue;
                }

                lastPart.Span.CopyTo(buffer.Span);
                bufferLength = lastPart.Length;
            }
        }

        if (_reader is null)
        {
            throw new ArgumentNullException(nameof(_reader));
        }

        Task.Run(StartListeningAsync);
    }

    private protected override async ValueTask ConnectClient()
    {
        await _tcpClient.ConnectAsync(_url.Url, _url.Port);
        Stream stream = _tcpClient.GetStream();
        if (UseSSL)
        {
            SslStream sslStream = new(stream, false);
            await sslStream.AuthenticateAsClientAsync(_url.Url);
            stream = sslStream;
        }

        _reader = new(stream);
        _writer = new(stream);
    }

    private protected override ValueTask DisconnectClient(string closeMessage)
    {
        _tcpClient.Close();
        return ValueTask.CompletedTask;
    }

    private protected override (string Url, int Port) GetUrl()
    {
        return UseSSL ? ("irc.chat.twitch.tv", 443) : ("irc.chat.twitch.tv", 80);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public override void Dispose()
    {
        _tcpClient.Dispose();
        _reader?.Dispose();
        _writer?.Dispose();
    }
}
#endif
