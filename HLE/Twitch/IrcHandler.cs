using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// A class that handles incoming IRC messages.
/// </summary>
public sealed class IrcHandler(ParsingMode parsingMode) : IEquatable<IrcHandler>
{
    /// <summary>
    /// Is invoked if a JOIN command has been received.
    /// </summary>
    public event EventHandler<JoinChannelMessage>? OnJoinReceived;

    /// <summary>
    /// Is invoked if a PART command has been received.
    /// </summary>
    public event EventHandler<LeftChannelMessage>? OnPartReceived;

    /// <summary>
    /// Is invoked if a ROOMSTATE command has been received.
    /// </summary>
    public event EventHandler<Roomstate>? OnRoomstateReceived;

    /// <summary>
    /// Is invoked if a PRIVMSG command has been received.
    /// </summary>
    public event EventHandler<IChatMessage>? OnChatMessageReceived;

    /// <summary>
    /// Is invoked if a RECONNECT command has been received.
    /// </summary>
    public event EventHandler? OnReconnectReceived;

    /// <summary>
    /// Is invoked if a PING command has been received.
    /// </summary>
    public event EventHandler<ReceivedData>? OnPingReceived;

    /// <summary>
    /// Is invoked if a NOTICE command has been received.
    /// </summary>
    public event EventHandler<Notice>? OnNoticeReceived;

    private readonly ChatMessageParser _chatMessageParser = parsingMode switch
    {
        ParsingMode.TimeEfficient => new TimeEfficientChatMessageParser(),
        ParsingMode.Balanced => new BalancedChatMessageParser(),
        ParsingMode.MemoryEfficient => new MemoryEfficientChatMessageParser(),
        _ => throw new ArgumentOutOfRangeException(nameof(parsingMode), parsingMode, null)
    };

    private readonly RoomstateParser _roomstateParser = new();
    private readonly MembershipMessageParser _membershipMessageParser = new();
    private readonly NoticeParser _noticeParser = new();

    private const string _joinCommand = "JOIN";
    private const string _roomstateCommand = "ROOMSTATE";
    private const string _privmsgCommand = "PRIVMSG";
    private const string _pingCommand = "PING";
    private const string _partCommand = "PART";
    private const string _reconnectCommand = "RECONNECT";
    private const string _noticeCommand = "NOTICE";
    // TODO: WHISPER, CLEARMSG, CLEARCHAT, USERSTATE, USERNOTICE

    private const int _maximumWhitespacesNeededToHandle = 5;

    /// <summary>
    /// Handles a messages and invokes an event, if the message could be handled.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <returns>True, if an event has been invoked, otherwise false.</returns>
    public bool Handle(ReadOnlySpan<char> ircMessage)
    {
        Span<int> indicesOfWhitespaces = stackalloc int[_maximumWhitespacesNeededToHandle];
        int whitespaceCount = ParsingHelper.GetIndicesOfWhitespaces(ircMessage, ref MemoryMarshal.GetReference(indicesOfWhitespaces), _maximumWhitespacesNeededToHandle);
        indicesOfWhitespaces = indicesOfWhitespaces[..whitespaceCount];

        return whitespaceCount switch
        {
            > 3 => HandleMoreThanThreeWhitespaces(ircMessage, indicesOfWhitespaces),
            > 2 => HandleMoreThanTwoWhitespaces(ircMessage, indicesOfWhitespaces),
            > 1 => HandleMoreThanOneWhitespace(ircMessage, indicesOfWhitespaces),
            > 0 => HandleMoreThanZeroWhitespaces(ircMessage, indicesOfWhitespaces),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleMoreThanThreeWhitespaces(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> thirdWord = ircMessage[(indicesOfWhitespaces[1] + 1)..indicesOfWhitespaces[2]];
        if (HandlePrivMsgCommand(ircMessage, indicesOfWhitespaces, thirdWord) || HandleNoticeCommandWithTag(ircMessage, indicesOfWhitespaces, thirdWord))
        {
            return true;
        }

        ReadOnlySpan<char> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..indicesOfWhitespaces[1]];
        return HandleNoticeCommandWithoutTag(ircMessage, indicesOfWhitespaces, secondWord);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleNoticeCommandWithTag(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> thirdWord)
    {
        if (ircMessage[0] != '@')
        {
            return false;
        }

        if (OnNoticeReceived is null || !thirdWord.SequenceEqual(_noticeCommand))
        {
            return false;
        }

        OnNoticeReceived.Invoke(this, _noticeParser.Parse(ircMessage, indicesOfWhitespaces));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleMoreThanZeroWhitespaces(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        if (HandlePingCommand(ircMessage, firstWord))
        {
            return true;
        }

        ReadOnlySpan<char> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..];
        return HandleReconnectCommand(secondWord);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleReconnectCommand(ReadOnlySpan<char> secondWord)
    {
        if (OnReconnectReceived is null || !secondWord.SequenceEqual(_reconnectCommand))
        {
            return false;
        }

        OnReconnectReceived.Invoke(this, EventArgs.Empty);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandlePingCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<char> firstWord)
    {
        if (OnPingReceived is null || !firstWord.SequenceEqual(_pingCommand))
        {
            return false;
        }

        OnPingReceived.Invoke(this, new(ircMessage[6..]));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleMoreThanOneWhitespace(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..indicesOfWhitespaces[1]];
        return HandleJoinCommand(ircMessage, indicesOfWhitespaces, secondWord) || HandlePartCommand(ircMessage, indicesOfWhitespaces, secondWord);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandlePartCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> secondWord)
    {
        if (OnPartReceived is null || !secondWord.SequenceEqual(_partCommand))
        {
            return false;
        }

        OnPartReceived.Invoke(this, _membershipMessageParser.ParseLeftChannelMessage(ircMessage, indicesOfWhitespaces));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleJoinCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> secondWord)
    {
        if (OnJoinReceived is null || !secondWord.SequenceEqual(_joinCommand))
        {
            return false;
        }

        OnJoinReceived.Invoke(this, _membershipMessageParser.ParseJoinChannelMessage(ircMessage, indicesOfWhitespaces));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleMoreThanTwoWhitespaces(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<char> thirdWord = ircMessage[(indicesOfWhitespaces[1] + 1)..indicesOfWhitespaces[2]];
        if (HandleRoomstateCommand(ircMessage, indicesOfWhitespaces, thirdWord))
        {
            return true;
        }

        ReadOnlySpan<char> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..indicesOfWhitespaces[1]];
        return HandleNoticeCommandWithoutTag(ircMessage, indicesOfWhitespaces, secondWord);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleNoticeCommandWithoutTag(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> secondWord)
    {
        if (OnNoticeReceived is null || !secondWord.SequenceEqual(_noticeCommand))
        {
            return false;
        }

        OnNoticeReceived.Invoke(this, _noticeParser.Parse(ircMessage, indicesOfWhitespaces));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleRoomstateCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> thirdWord)
    {
        if (OnRoomstateReceived is null || !thirdWord.SequenceEqual(_roomstateCommand))
        {
            return false;
        }

        _roomstateParser.Parse(ircMessage, indicesOfWhitespaces, out Roomstate roomstate);
        OnRoomstateReceived.Invoke(this, roomstate);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandlePrivMsgCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> thirdWord)
    {
        if (OnChatMessageReceived is null || !thirdWord.SequenceEqual(_privmsgCommand))
        {
            return false;
        }

        OnChatMessageReceived.Invoke(this, _chatMessageParser.Parse(ircMessage, indicesOfWhitespaces));
        return true;
    }

    public bool Equals(IrcHandler? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is IrcHandler other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }
}
