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

    internal event EventHandler<PingArgs>? OnPingReceived;

    #endregion Events

    private const string _joinCommand = "JOIN";
    private const string _roomstateCommand = "ROOMSTATE";
    private const string _privmsgCommand = "PRIVMSG";
    private const string _pingCommand = "PING";
    private const string _partCommand = "PART";

    /// <summary>
    /// Handles the incoming messages.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    public void Handle(ReadOnlySpan<char> ircMessage)
    {
        Span<Range> ircRanges = stackalloc Range[ircMessage.Length];
        int ircRangesLength = ircMessage.GetRangesOfSplit(' ', ircRanges);
        ircRanges = ircRanges[..ircRangesLength];
        switch (ircRangesLength)
        {
            case >= 3:
                if (ircMessage[ircRanges[2]].Equals(_privmsgCommand, StringComparison.Ordinal))
                {
                    OnChatMessageReceived?.Invoke(this, new(ircMessage));
                }
                else if (ircMessage[ircRanges[1]].Equals(_joinCommand, StringComparison.Ordinal))
                {
                    OnJoinedChannel?.Invoke(this, new(ircMessage, ircRanges));
                }
                else if (ircMessage[ircRanges[1]].Equals(_partCommand, StringComparison.Ordinal))
                {
                    OnLeftChannel?.Invoke(this, new(ircMessage, ircRanges));
                }
                else if (ircMessage[ircRanges[2]].Equals(_roomstateCommand, StringComparison.Ordinal))
                {
                    OnRoomstateReceived?.Invoke(this, new(ircMessage, ircRanges));
                }

                break;
            case >= 1 when ircMessage[ircRanges[0]].Equals(_pingCommand, StringComparison.Ordinal):
                OnPingReceived?.Invoke(this, new(ircMessage[6..]));
                break;
        }
    }
}
