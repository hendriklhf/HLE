using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using HLE.Time;

namespace HLE.Twitch;

/// <summary>
/// A class that represents a IRC client for Twitch chats. Connects to "wss://irc-ws.chat.twitch.tv:443".<br/>
/// Only provides the connection. Received message are not handled here. Use this class if you want to handle the messages by yourself.
/// </summary>
public class IrcClient
{
    /// <summary>
    /// The username of the client.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// The OAuth token of the user.
    /// </summary>
    public string? OAuthToken { get; }

    /// <summary>
    /// Indicates whether the client is connected or not.
    /// </summary>
    public bool IsConnected => _webSocket.State is WebSocketState.Open;

    #region Events

    /// <summary>
    /// Is invoked if the client connects.
    /// </summary>
    public event EventHandler? OnConnected;

    /// <summary>
    /// Is invoked if the client disconnects.
    /// </summary>
    public event EventHandler? OnDisconnected;

    /// <summary>
    /// Is invoked if the client receives data.
    /// </summary>
    public event EventHandler<Memory<byte>>? OnDataReceived;

    /// <summary>
    /// Is invoked if the client sends data.
    /// </summary>
    public event EventHandler<Memory<byte>>? OnDataSent;

    #endregion Events

    private readonly bool _isVerifiedBot;

    private readonly ClientWebSocket _webSocket = new();
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// The basic constructor of <see cref="IrcClient"/>. An OAuth token for example can be obtained here: <a href="https://twitchapps.com/tmi">twitchapps.com/tmi</a>.
    /// </summary>
    /// <param name="username">The username of the client.</param>
    /// <param name="oAuthToken">The OAuth token of the client.</param>
    /// <param name="isVerifiedBot">If the client user is a verified bot, pass true, otherwise false.</param>
    public IrcClient(string username, string? oAuthToken, bool isVerifiedBot = false)
    {
        Username = username;
        OAuthToken = oAuthToken;
        _isVerifiedBot = isVerifiedBot;

        using CancellationTokenSource tokenCreator = new();
        _cancellationToken = tokenCreator.Token;
    }

    /// <summary>
    /// Sends a raw message to the Twitch IRC server.
    /// </summary>
    /// <param name="message">The IRC message.</param>
    public void SendRaw(string message)
    {
        Send(message).Wait(_cancellationToken);
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

    /// <summary>
    /// Connects the client to the Twitch IRC server on "wss://irc-ws.chat.twitch.tv:443".
    /// </summary>
    /// <param name="channels">The collection of channels the client will join on connect.</param>
    public void Connect(IEnumerable<string> channels)
    {
        async Task ConnectLocal()
        {
            await _webSocket.ConnectAsync(new("wss://irc-ws.chat.twitch.tv:443"), _cancellationToken);
            OnConnected?.Invoke(this, EventArgs.Empty);
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

    /// <summary>
    /// Sends a chat message to a channel.
    /// </summary>
    /// <param name="channel">The channel the message will be sent to.</param>
    /// <param name="message">The message that will be sent to the channel.</param>
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
        string[] channelArr = channels.ToArray();
        if (channelArr.Length == 0)
        {
            return;
        }

        int maxChannels = _isVerifiedBot ? 200 : 20;
        const short period = 10000;
        string[] joins = channelArr.Select(c => $"JOIN {c}").ToArray();
        long start = TimeHelper.Now();
        for (int i = 0; i < joins.Length; i++)
        {
            if (i > 0 && i % maxChannels == 0)
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

    /// <summary>
    /// Joins one channel.
    /// </summary>
    /// <param name="channel">The channel the client will join.</param>
    public void JoinChannel(string channel)
    {
        async Task JoinChannelLocal()
        {
            await Send($"JOIN {channel}");
        }

        Task.Run(JoinChannelLocal, _cancellationToken).Wait(_cancellationToken);
    }

    /// <summary>
    /// Leaves one channel.
    /// </summary>
    /// <param name="channel">The channel the client will leave.</param>
    public void LeaveChannel(string channel)
    {
        async Task LeaveChannelLocal()
        {
            await Send($"PART {channel}");
        }

        Task.Run(LeaveChannelLocal, _cancellationToken).Wait(_cancellationToken);
    }

    /// <summary>
    /// Disconnects the client.
    /// </summary>
    /// <param name="closeMessage">A close message or reason.</param>
    public void Disconnect(string closeMessage = "Manually closed")
    {
        async Task DisconnectLocal()
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeMessage, _cancellationToken);
        }

        Task.Run(DisconnectLocal, _cancellationToken).Wait(_cancellationToken);
        OnDisconnected?.Invoke(this, EventArgs.Empty);
    }
}
