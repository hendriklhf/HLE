using System;
using HLE.Collections;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// A class that handles incoming IRC messages.
/// </summary>
public sealed class IrcHandler
{
    #region Events

    /// <summary>
    /// Is invoked if a JOIN message has been received.
    /// </summary>
    public event EventHandler<JoinedChannelArgs>? OnJoinedChannel;

    /// <summary>
    /// Is invoked if a PART message has been received.
    /// </summary>
    public event EventHandler<LeftChannelArgs>? OnLeftChannel;

    /// <summary>
    /// Is invoked if a ROOMSTATE message has been received.
    /// </summary>
    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;

    /// <summary>
    /// Is invoked if a PRIVMSG message has been received.
    /// </summary>
    public event EventHandler<ChatMessage>? OnChatMessageReceived;

    internal event EventHandler? OnReconnectReceived;

    internal event EventHandler<ReceivedData>? OnPingReceived;

    #endregion Events

    private const byte _whitespace = (byte)' ';
    private readonly byte[] _joinCommand = "JOIN"u8.ToArray();
    private readonly byte[] _roomstateCommand = "ROOMSTATE"u8.ToArray();
    private readonly byte[] _privmsgCommand = "PRIVMSG"u8.ToArray();
    private readonly byte[] _pingCommand = "PING"u8.ToArray();
    private readonly byte[] _partCommand = "PART"u8.ToArray();
    private readonly byte[] _reconnectCommand = "RECONNECT"u8.ToArray();

    /// <summary>
    /// Handles the incoming messages.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <returns>True, if an event has been invoked, otherwise false.</returns>
    public bool Handle(ReadOnlySpan<byte> ircMessage)
    {
        Span<int> indicesOfWhitespace = stackalloc int[ircMessage.Length];
        int whitespaceCount = ircMessage.IndicesOf(_whitespace, indicesOfWhitespace);

        switch (whitespaceCount)
        {
            case > 2:
                ReadOnlySpan<byte> thirdWord = ircMessage[(indicesOfWhitespace[1] + 1)..indicesOfWhitespace[2]];
                if (thirdWord.SequenceEqual(_privmsgCommand))
                {
                    OnChatMessageReceived?.Invoke(this, new(ircMessage, indicesOfWhitespace[..whitespaceCount]));
                    return true;
                }

                if (thirdWord.SequenceEqual(_roomstateCommand))
                {
                    OnRoomstateReceived?.Invoke(this, new(ircMessage, indicesOfWhitespace[..whitespaceCount]));
                    return true;
                }

                break;
            case > 1:
                ReadOnlySpan<byte> secondWord = ircMessage[(indicesOfWhitespace[0] + 1)..indicesOfWhitespace[1]];
                if (secondWord.SequenceEqual(_joinCommand))
                {
                    OnJoinedChannel?.Invoke(this, new(ircMessage, indicesOfWhitespace[..whitespaceCount]));
                    return true;
                }

                if (secondWord.SequenceEqual(_partCommand))
                {
                    OnLeftChannel?.Invoke(this, new(ircMessage, indicesOfWhitespace[..whitespaceCount]));
                    return true;
                }

                break;
            case > 0:
                ReadOnlySpan<byte> firstWord = ircMessage[..indicesOfWhitespace[0]];
                if (firstWord.SequenceEqual(_pingCommand))
                {
                    OnPingReceived?.Invoke(this, ReceivedData.Create(ircMessage[6..]));
                    return true;
                }

                secondWord = ircMessage[(indicesOfWhitespace[0] + 1)..];
                if (secondWord.SequenceEqual(_reconnectCommand))
                {
                    OnReconnectReceived?.Invoke(this, EventArgs.Empty);
                    return true;
                }

                break;
        }

        return false;
    }
}
