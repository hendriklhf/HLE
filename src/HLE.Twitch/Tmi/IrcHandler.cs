using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Threading;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

/// <summary>
/// A class that handles incoming IRC messages and invokes events for the associated message type.
/// </summary>
/// <param name="parsingMode">The parsing mode of all internal messages parsers.</param>
public sealed class IrcHandler(ParsingMode parsingMode) : IEquatable<IrcHandler>
{
    /// <summary>
    /// Is invoked if a JOIN command has been received.
    /// </summary>
    public event AsyncEventHandler<IrcHandler, JoinChannelMessage>? OnJoinReceived;

    /// <summary>
    /// Is invoked if a PART command has been received.
    /// </summary>
    public event AsyncEventHandler<IrcHandler, LeftChannelMessage>? OnPartReceived;

    /// <summary>
    /// Is invoked if a ROOMSTATE command has been received.
    /// </summary>
    public event AsyncEventHandler<IrcHandler, Roomstate>? OnRoomstateReceived;

    /// <summary>
    /// Is invoked if a PRIVMSG command has been received.
    /// </summary>
    public event AsyncEventHandler<IrcHandler, IChatMessage>? OnChatMessageReceived;

    /// <summary>
    /// Is invoked if a RECONNECT command has been received.
    /// </summary>
    public event AsyncEventHandler<IrcHandler>? OnReconnectReceived;

    /// <summary>
    /// Is invoked if a PING command has been received.
    /// </summary>
    public event AsyncEventHandler<IrcHandler, Bytes>? OnPingReceived;

    /// <summary>
    /// Is invoked if a NOTICE command has been received.
    /// </summary>
    public event AsyncEventHandler<IrcHandler, Notice>? OnNoticeReceived;

    internal bool IsOnJoinReceivedSubscribed => OnJoinReceived is not null;

    internal bool IsOnPartReceivedSubscribed => OnPartReceived is not null;

    internal bool IsOnNoticeReceivedSubscribed => OnNoticeReceived is not null;

    internal bool IsOnChatMessageReceivedSubscribed => OnChatMessageReceived is not null;

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

    private static ReadOnlySpan<byte> JoinCommand => "JOIN"u8;

    private static ReadOnlySpan<byte> RoomstateCommand => "ROOMSTATE"u8;

    private static ReadOnlySpan<byte> PrivmsgCommand => "PRIVMSG"u8;

    private static ReadOnlySpan<byte> PingCommand => "PING"u8;

    private static ReadOnlySpan<byte> PartCommand => "PART"u8;

    private static ReadOnlySpan<byte> ReconnectCommand => "RECONNECT"u8;

    private static ReadOnlySpan<byte> NoticeCommand => "NOTICE"u8;

    // TODO: WHISPER, CLEARMSG, CLEARCHAT, USERSTATE, USERNOTICE

    private const int MaximumWhitespacesNeededToHandle = 5;

    /// <summary>
    /// Handles a messages and invokes an event, if the message could be handled.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <returns>True, if an event has been invoked, otherwise false.</returns>
    public bool Handle(ReadOnlySpan<byte> ircMessage)
    {
        Span<int> indicesOfWhitespaces = stackalloc int[MaximumWhitespacesNeededToHandle];
        int whitespaceCount = ParsingHelpers.IndicesOf(ircMessage, (byte)' ', indicesOfWhitespaces, MaximumWhitespacesNeededToHandle);
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

    private bool HandleMoreThanThreeWhitespaces(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> thirdWord = ircMessage[(indicesOfWhitespaces[1] + 1)..indicesOfWhitespaces[2]];
        if (HandlePrivMsgCommand(ircMessage, indicesOfWhitespaces, thirdWord) ||
            HandleNoticeCommandWithTag(ircMessage, indicesOfWhitespaces, thirdWord))
        {
            return true;
        }

        ReadOnlySpan<byte> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..indicesOfWhitespaces[1]];
        return HandleNoticeCommandWithoutTag(ircMessage, indicesOfWhitespaces, secondWord);
    }

    private bool HandleNoticeCommandWithTag(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> thirdWord)
    {
        if (ircMessage[0] != '@')
        {
            return false;
        }

        if (OnNoticeReceived is null || !thirdWord.SequenceEqual(NoticeCommand))
        {
            return false;
        }

        Notice notice = _noticeParser.Parse(ircMessage, indicesOfWhitespaces);
        EventInvoker.InvokeAsync(OnNoticeReceived, this, notice).Ignore();
        return true;
    }

    private bool HandleMoreThanZeroWhitespaces(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        if (HandlePingCommand(ircMessage, firstWord))
        {
            return true;
        }

        ReadOnlySpan<byte> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..];
        return HandleReconnectCommand(secondWord);
    }

    private bool HandleReconnectCommand(ReadOnlySpan<byte> secondWord)
    {
        if (OnReconnectReceived is null || !secondWord.SequenceEqual(ReconnectCommand))
        {
            return false;
        }

        EventInvoker.InvokeAsync(OnReconnectReceived, this).Ignore();
        return true;
    }

    private bool HandlePingCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<byte> firstWord)
    {
        if (OnPingReceived is null || !firstWord.SequenceEqual(PingCommand))
        {
            return false;
        }

        Bytes pingMessage = new(ircMessage[6..]);
        EventInvoker.InvokeAsync(OnPingReceived, this, pingMessage).Ignore();
        return true;
    }

    private bool HandleMoreThanOneWhitespace(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..indicesOfWhitespaces[1]];
        return HandleJoinCommand(ircMessage, indicesOfWhitespaces, secondWord) || HandlePartCommand(ircMessage, indicesOfWhitespaces, secondWord);
    }

    private bool HandlePartCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> secondWord)
    {
        if (OnPartReceived is null || !secondWord.SequenceEqual(PartCommand))
        {
            return false;
        }

        LeftChannelMessage leftChannelMessage = _membershipMessageParser.ParseLeftChannelMessage(ircMessage, indicesOfWhitespaces);
        EventInvoker.InvokeAsync(OnPartReceived, this, leftChannelMessage).Ignore();
        return true;
    }

    private bool HandleJoinCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> secondWord)
    {
        if (OnJoinReceived is null || !secondWord.SequenceEqual(JoinCommand))
        {
            return false;
        }

        JoinChannelMessage joinChannelMessage = _membershipMessageParser.ParseJoinChannelMessage(ircMessage, indicesOfWhitespaces);
        EventInvoker.InvokeAsync(OnJoinReceived, this, joinChannelMessage).Ignore();
        return true;
    }

    private bool HandleMoreThanTwoWhitespaces(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> thirdWord = ircMessage[(indicesOfWhitespaces[1] + 1)..indicesOfWhitespaces[2]];
        if (HandleRoomstateCommand(ircMessage, indicesOfWhitespaces, thirdWord))
        {
            return true;
        }

        ReadOnlySpan<byte> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..indicesOfWhitespaces[1]];
        return HandleNoticeCommandWithoutTag(ircMessage, indicesOfWhitespaces, secondWord);
    }

    private bool HandleNoticeCommandWithoutTag(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> secondWord)
    {
        if (OnNoticeReceived is null || !secondWord.SequenceEqual(NoticeCommand))
        {
            return false;
        }

        Notice notice = _noticeParser.Parse(ircMessage, indicesOfWhitespaces);
        EventInvoker.InvokeAsync(OnNoticeReceived, this, notice).Ignore();
        return true;
    }

    private bool HandleRoomstateCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> thirdWord)
    {
        if (OnRoomstateReceived is null || !thirdWord.SequenceEqual(RoomstateCommand))
        {
            return false;
        }

        _roomstateParser.Parse(ircMessage, indicesOfWhitespaces, out Roomstate roomstate);
        EventInvoker.InvokeAsync(OnRoomstateReceived, this, roomstate).Ignore();
        return true;
    }

    private bool HandlePrivMsgCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> thirdWord)
    {
        if (OnChatMessageReceived is null || !thirdWord.SequenceEqual(PrivmsgCommand))
        {
            return false;
        }

        // ReSharper disable once NotDisposedResource
        IChatMessage chatMessage = _chatMessageParser.Parse(ircMessage, indicesOfWhitespaces);
        EventInvoker.InvokeAsync(OnChatMessageReceived, this, chatMessage).Ignore();
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
