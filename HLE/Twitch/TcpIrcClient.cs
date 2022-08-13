using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HLE.Twitch;

/// <summary>
/// Provides a Twitch IRC server connection via a <see cref="TcpClient"/>.
/// </summary>
public class TcpIrcClient : IrcClient
{
    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public override bool IsConnected => _tcpClient.Connected && !_token.IsCancellationRequested;

    private readonly TcpClient _tcpClient = new();

    /// <summary>
    /// The default constructor of <see cref="TcpIrcClient"/>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    public TcpIrcClient(string username, string? oAuthToken = null) : base(username, oAuthToken)
    {
    }

    private protected override async Task Send(string message)
    {
        NetworkStream stream = _tcpClient.GetStream();
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(bytes, _token);
        InvokeDataSent(this, bytes);
    }

    private protected override void StartListening()
    {
        async Task StartListeningLocal()
        {
            NetworkStream stream = _tcpClient.GetStream();
            while (!_tokenSource.IsCancellationRequested && IsConnected)
            {
                Memory<byte> buffer = new(new byte[2048]);
                int count = await stream.ReadAsync(buffer, _token);
                InvokeDataReceived(this, buffer[..(count - 1)]);
            }
        }

        Task.Run(StartListeningLocal, _token);
    }

    private protected override async Task ConnectClient()
    {
        await _tcpClient.ConnectAsync(_url.Url, _url.Port, _token);
    }

    private protected override Task DisconnectClient(string closeMessage)
    {
        _tcpClient.Close();
        return Task.CompletedTask;
    }

    private protected override (string Url, int Port) GetUrl()
    {
        return UseSSL ? ("irc.chat.twitch.tv", 6697) : ("irc.chat.twitch.tv", 6667);
    }
}
