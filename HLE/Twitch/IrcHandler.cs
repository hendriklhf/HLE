using System;
using HLE.Twitch.Args;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public class IrcHandler
{
    #region Events

    public event EventHandler<JoinedChannelArgs>? OnJoinedChannel;
    public event EventHandler<LeftChannelArgs>? OnLeftChannel;
    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;
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

    public void Handle(string ircMessage)
    {
        string[] split = ircMessage.Split();
        switch (split.Length)
        {
            case >= 3:
            {
                if (string.Equals(split[2], _ircCmds[1], StringComparison.Ordinal))
                {
                    OnRoomstateReceived?.Invoke(this, new(ircMessage));
                }
                else if (string.Equals(split[2], _ircCmds[2], StringComparison.Ordinal))
                {
                    OnChatMessageReceived?.Invoke(this, new(ircMessage));
                }

                break;
            }
            case >= 2:
            {
                if (string.Equals(split[1], _ircCmds[0], StringComparison.Ordinal))
                {
                    OnJoinedChannel?.Invoke(this, new(ircMessage));
                }
                else if (string.Equals(split[1], _ircCmds[4], StringComparison.Ordinal))
                {
                    OnLeftChannel?.Invoke(this, new(ircMessage));
                }

                break;
            }
            case >= 1 when string.Equals(split[0], _ircCmds[3], StringComparison.Ordinal):
            {
                OnPingReceived?.Invoke(this, new(ircMessage[6..]));
                break;
            }
        }
    }
}
