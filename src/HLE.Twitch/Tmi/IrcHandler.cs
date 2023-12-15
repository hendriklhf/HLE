using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

/// <summary>
/// A class that handles incoming IRC messages and invokes events for the associated message type.
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
        _ => throw new InvalidEnumArgumentException(nameof(parsingMode), (int)parsingMode, typeof(ParsingMode))
    };

    private readonly RoomstateParser _roomstateParser = new();
    private readonly MembershipMessageParser _membershipMessageParser = new();
    private readonly NoticeParser _noticeParser = new();

    private const string JoinCommand = "JOIN";
    private const string RoomstateCommand = "ROOMSTATE";
    private const string PrivmsgCommand = "PRIVMSG";
    private const string PingCommand = "PING";
    private const string PartCommand = "PART";
    private const string ReconnectCommand = "RECONNECT";
    private const string NoticeCommand = "NOTICE";
    // TODO: WHISPER, CLEARMSG, CLEARCHAT, USERSTATE, USERNOTICE

    private const int MaximumWhitespacesNeededToHandle = 5;

    /// <summary>
    /// Handles a messages and invokes an event, if the message could be handled.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <returns>True, if an event has been invoked, otherwise false.</returns>
    public bool Handle(ReadOnlySpan<char> ircMessage)
    {
        Span<int> indicesOfWhitespaces = stackalloc int[MaximumWhitespacesNeededToHandle];
        int whitespaceCount = ParsingHelpers.IndicesOf(ircMessage, ' ', indicesOfWhitespaces, MaximumWhitespacesNeededToHandle);
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

        if (OnNoticeReceived is null || !thirdWord.SequenceEqual(NoticeCommand))
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
        if (OnReconnectReceived is null || !secondWord.SequenceEqual(ReconnectCommand))
        {
            return false;
        }

        OnReconnectReceived.Invoke(this, EventArgs.Empty);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandlePingCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<char> firstWord)
    {
        if (OnPingReceived is null || !firstWord.SequenceEqual(PingCommand))
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
        if (OnPartReceived is null || !secondWord.SequenceEqual(PartCommand))
        {
            return false;
        }

        OnPartReceived.Invoke(this, _membershipMessageParser.ParseLeftChannelMessage(ircMessage, indicesOfWhitespaces));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleJoinCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> secondWord)
    {
        if (OnJoinReceived is null || !secondWord.SequenceEqual(JoinCommand))
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
        if (OnNoticeReceived is null || !secondWord.SequenceEqual(NoticeCommand))
        {
            return false;
        }

        OnNoticeReceived.Invoke(this, _noticeParser.Parse(ircMessage, indicesOfWhitespaces));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleRoomstateCommand(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<char> thirdWord)
    {
        if (OnRoomstateReceived is null || !thirdWord.SequenceEqual(RoomstateCommand))
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
        if (OnChatMessageReceived is null || !thirdWord.SequenceEqual(PrivmsgCommand))
        {
            return false;
        }

        OnChatMessageReceived.Invoke(this, _chatMessageParser.Parse(ircMessage, indicesOfWhitespaces));
        return true;
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] IrcHandler? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(IrcHandler? left, IrcHandler? right) => Equals(left, right);

    public static bool operator !=(IrcHandler? left, IrcHandler? right) => !(left == right);
}
