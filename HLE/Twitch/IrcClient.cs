using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using HLE.Time;

namespace HLE.Twitch;

public class IrcClient
{
    public string Username { get; }

    public string? OAuthToken { get; }

    public bool IsConnected => _webSocket.State is WebSocketState.Open;

    #region Events

    public event EventHandler<Memory<byte>>? OnDataReceived;
    public event EventHandler<Memory<byte>>? OnDataSent;

    #endregion Events

    private readonly bool _isVerifiedBot;

    private readonly ClientWebSocket _webSocket = new();
    private readonly CancellationToken _cancellationToken;

    public IrcClient(string username, string? oAuthToken, bool isVerifiedBot = false)
    {
        Username = username;
        OAuthToken = oAuthToken;
        _isVerifiedBot = isVerifiedBot;

        using CancellationTokenSource tokenCreator = new();
        _cancellationToken = tokenCreator.Token;
    }

    private void StartListening()
    {
        async Task StartListeningLocal()
        {
            while (!_cancellationToken.IsCancellationRequested && IsConnected)
            {
                Memory<byte> buffer = new(new byte[1024]);
                ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, _cancellationToken);
                OnDataReceived?.Invoke(this, buffer[..(result.Count - 1)]);
            }
        }

        Task.Run(StartListeningLocal, _cancellationToken);
    }

    private async Task Send(string message)
    {
        byte[] bytes = message.Encode();
        Memory<byte> msg = new(bytes);
        await _webSocket.SendAsync(new(bytes), WebSocketMessageType.Text, true, _cancellationToken);
        OnDataSent?.Invoke(this, msg);
    }

    public void Connect(IEnumerable<string> channels)
    {
        async Task ConnectLocal()
        {
            await _webSocket.ConnectAsync(new("wss://irc-ws.chat.twitch.tv:443"), _cancellationToken);
            StartListening();
            if (OAuthToken is not null)
            {
                await Send($"PASS {OAuthToken}");
            }

            await Send($"NICK {Username}");
            await Send("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            await JoinChannels(channels);
        }

        Task.Run(ConnectLocal, _cancellationToken).Wait(_cancellationToken);
    }

    public void SendMessage(string channel, string message)
    {
        async Task SendLocal()
        {
            await Send($"PRIVMSG {channel} :{message}");
        }

        Task.Run(SendLocal, _cancellationToken).Wait(_cancellationToken);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private async Task JoinChannels(IEnumerable<string> channels)
    {
        int maxChannels = _isVerifiedBot ? 200 : 20;
        const short period = 10000;
        string[] joins = channels.Select(c => $"JOIN {c}").ToArray();
        long start = TimeHelper.Now();
        for (int i = 0; i < joins.Length; i++)
        {
            if (i % maxChannels == 0 && i != default)
            {
                int waitTime = (int)(period - (TimeHelper.Now() - start));
                if (waitTime > 0)
                {
                    await Task.Delay(waitTime, _cancellationToken);
                }

                start = TimeHelper.Now();
            }

            await Send(joins[i]);
        }
    }

    public void JoinChannel(string channel)
    {
        async Task JoinChannelLocal()
        {
            await Send($"JOIN {channel}");
        }

        Task.Run(JoinChannelLocal, _cancellationToken).Wait(_cancellationToken);
    }

    public void LeaveChannel(string channel)
    {
        async Task LeaveChannelLocal()
        {
            await Send($"PART {channel}");
        }

        Task.Run(LeaveChannelLocal, _cancellationToken).Wait(_cancellationToken);
    }

    public void LeaveChannels(IEnumerable<string> channels)
    {
        async Task LeaveChannelsLocal()
        {
            IEnumerable<string> parts = channels.Select(c => $"PART {c}");
            foreach (string part in parts)
            {
                await Send(part);
            }
        }

        Task.Run(LeaveChannelsLocal, _cancellationToken).Wait(_cancellationToken);
    }

    public void Disconnect(string closeMessage = "Manually closed")
    {
        async Task DisconnectLocal()
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeMessage, _cancellationToken);
        }

        Task.Run(DisconnectLocal, _cancellationToken).Wait(_cancellationToken);
    }
}
