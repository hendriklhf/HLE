using System;
using HLE.Twitch.Args;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// A class that handles incoming IRC messages.
/// </summary>
public class IrcHandler
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
    public void Handle(string ircMessage)
    {
        string[] split = ircMessage.Split();
        switch (split.Length)
        {
            case >= 3:
            {
                if (split[2] == _ircCmds[1])
                {
                    OnRoomstateReceived?.Invoke(this, new(ircMessage));
                }
                else if (split[2] == _ircCmds[2])
                {
                    OnChatMessageReceived?.Invoke(this, new(ircMessage));
                }
                else if (split[1] == _ircCmds[0])
                {
                    OnJoinedChannel?.Invoke(this, new(ircMessage));
                }
                else if (split[1] == _ircCmds[4])
                {
                    OnLeftChannel?.Invoke(this, new(ircMessage));
                }

                break;
            }
            case >= 1 when split[0] == _ircCmds[3]:
            {
                OnPingReceived?.Invoke(this, new(ircMessage[6..]));
                break;
            }
        }
    }
}
