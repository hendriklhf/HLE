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

    private readonly string[] _ircCmds =
    {
        "JOIN",
        "ROOMSTATE",
        "PRIVMSG",
        "PING",
        "PART"
    };

    /// <summary>
    /// Handles the incoming messages.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    public void Handle(ReadOnlySpan<char> ircMessage)
    {
        Range[] ircRanges = ircMessage.GetRangesOfSplit();
        switch (ircRanges.Length)
        {
            case >= 3:
            {
                if (ircMessage[ircRanges[2]].SequenceEqual(_ircCmds[2]))
                {
                    OnChatMessageReceived?.Invoke(this, new(ircMessage, ircRanges));
                }
                else if (ircMessage[ircRanges[1]].SequenceEqual(_ircCmds[0]))
                {
                    OnJoinedChannel?.Invoke(this, new(ircMessage, ircRanges));
                }
                else if (ircMessage[ircRanges[1]].SequenceEqual(_ircCmds[4]))
                {
                    OnLeftChannel?.Invoke(this, new(ircMessage, ircRanges));
                }
                else if (ircMessage[ircRanges[2]].SequenceEqual(_ircCmds[1]))
                {
                    OnRoomstateReceived?.Invoke(this, new(ircMessage, ircRanges));
                }

                break;
            }
            case >= 1 when ircMessage[ircRanges[0]].SequenceEqual(_ircCmds[3]):
            {
                //OnPingReceived?.Invoke(this, new(ircMessage[6..]));
                OnPingReceived?.Invoke(this, new(""));
                break;
            }
        }
    }
}
