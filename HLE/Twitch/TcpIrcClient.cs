using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="TcpClient"/>.
/// </summary>
public sealed class TcpIrcClient : IrcClient
{
    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public override bool IsConnected => _tcpClient.Connected && !_token.IsCancellationRequested;

    private readonly TcpClient _tcpClient = new();
    private StreamReader? _reader;
    private StreamWriter? _writer;

    /// <summary>
    /// The default constructor of <see cref="TcpIrcClient"/>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    public TcpIrcClient(string username, string? oAuthToken = null) : base(username, oAuthToken)
    {
    }

    private protected override async ValueTask Send(string message)
    {
        if (_writer is null)
        {
            throw new ArgumentNullException(nameof(_writer));
        }

        char[] rentedArray = _charArrayPool.Rent(1024);
        try
        {
            Memory<char> chars = rentedArray;
            message.CopyTo(chars.Span);
            chars = chars[..message.Length];
            await Send(chars);
        }
        finally
        {
            _charArrayPool.Return(rentedArray);
        }
    }

    private protected override async ValueTask Send(ReadOnlyMemory<char> message)
    {
        if (_writer is null)
        {
            throw new ArgumentNullException(nameof(_writer));
        }

        await _writer.WriteLineAsync(message, _token);
        await _writer.FlushAsync();
        InvokeDataSent(this, message.Span);
    }

    private protected override void StartListening()
    {
        async ValueTask StartListeningLocal()
        {
            Memory<char> buffer = new char[2048];
            Memory<Range> rangeBuffer = new Range[256];
            while (!_tokenSource.IsCancellationRequested && IsConnected)
            {
                // TODO: this doesnt work :)
                int charCount = await _reader.ReadAsync(buffer, _token);
                if (charCount == 0)
                {
                    continue;
                }

                ReadOnlyMemory<char> message = buffer[..charCount];
                int rangesLength = message.Span.GetRangesOfSplit(_newLine, rangeBuffer.Span);
                ReadOnlyMemory<Range> ranges = rangeBuffer[..rangesLength];
                for (int i = 0; i < rangesLength; i++)
                {
                    InvokeDataReceived(this, message[ranges.Span[i]]);
                }
            }
        }

        if (_reader is null)
        {
            throw new ArgumentNullException(nameof(_reader));
        }

        Task.Run(StartListeningLocal, _token);
    }

    private protected override async ValueTask ConnectClient()
    {
        await _tcpClient.ConnectAsync(_url.Url, _url.Port, _token);
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
}
