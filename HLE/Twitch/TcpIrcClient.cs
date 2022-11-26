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

    ~TcpIrcClient()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _tcpClient.Dispose();
    }

    private protected override async Task Send(string message)
    {
        if (_writer is null)
        {
            throw new ArgumentNullException(nameof(_writer));
        }

        ReadOnlyMemory<char> chars = message.ToCharArray();
        await Send(chars);
    }

    private protected override async Task Send(ReadOnlyMemory<char> message)
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
        async Task StartListeningLocal()
        {
            Memory<char> buffer = new char[1024];
            while (!_tokenSource.IsCancellationRequested && IsConnected)
            {
                int count = await _reader.ReadAsync(buffer, _token);
                ReadOnlyMemory<char> message = buffer[..(count - 2)];
                InvokeDataReceived(this, message);
            }
        }

        if (_reader is null)
        {
            throw new ArgumentNullException(nameof(_reader));
        }

        Task.Run(StartListeningLocal, _token);
    }

    private protected override async Task ConnectClient()
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

    private protected override Task DisconnectClient(string closeMessage)
    {
        _tcpClient.Close();
        return Task.CompletedTask;
    }

    private protected override (string Url, int Port) GetUrl()
    {
        return UseSSL ? ("irc.chat.twitch.tv", 443) : ("irc.chat.twitch.tv", 80);
    }
}
