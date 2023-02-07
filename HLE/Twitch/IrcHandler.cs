using System;
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

    private const string _joinCommand = "JOIN";
    private const string _roomstateCommand = "ROOMSTATE";
    private const string _privmsgCommand = "PRIVMSG";
    private const string _pingCommand = "PING";
    private const string _partCommand = "PART";
    private const string _reconnectCommand = "RECONNECT";

    /// <summary>
    /// Handles the incoming messages.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <returns>True, if an event has been invoked, otherwise false.</returns>
    public bool Handle(ReadOnlySpan<char> ircMessage)
    {
        Span<Range> ircRanges = MemoryHelper.UseStackAlloc<Range>(ircMessage.Length) ? stackalloc Range[ircMessage.Length] : new Range[ircMessage.Length];
        int ircRangesLength = ircMessage.GetRangesOfSplit(' ', ircRanges);
        ircRanges = ircRanges[..ircRangesLength];

        switch (ircRanges.Length)
        {
            case > 2:
                if (ircMessage[ircRanges[2]].Equals(_privmsgCommand, StringComparison.Ordinal))
                {
                    OnChatMessageReceived?.Invoke(this, new(ircMessage, ircRanges));
                    return true;
                }

                if (ircMessage[ircRanges[2]].Equals(_roomstateCommand, StringComparison.Ordinal))
                {
                    OnRoomstateReceived?.Invoke(this, new(ircMessage, ircRanges));
                    return true;
                }

                if (ircMessage[ircRanges[1]].Equals(_joinCommand, StringComparison.Ordinal))
                {
                    OnJoinedChannel?.Invoke(this, new(ircMessage, ircRanges));
                    return true;
                }

                if (ircMessage[ircRanges[1]].Equals(_partCommand, StringComparison.Ordinal))
                {
                    OnLeftChannel?.Invoke(this, new(ircMessage, ircRanges));
                    return true;
                }

                break;
            case > 0:
                if (ircMessage[ircRanges[0]].Equals(_pingCommand, StringComparison.Ordinal))
                {
                    OnPingReceived?.Invoke(this, ReceivedData.Create(ircMessage[6..]));
                    return true;
                }

                if (ircMessage[ircRanges[1]].Equals(_reconnectCommand, StringComparison.Ordinal))
                {
                    OnReconnectReceived?.Invoke(this, EventArgs.Empty);
                    return true;
                }

                break;
        }

        return false;
    }
}
