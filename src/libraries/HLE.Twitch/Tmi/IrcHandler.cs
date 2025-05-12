using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using HLE.Threading;

namespace HLE.Twitch.Tmi;

/// <summary>
/// A class that handles incoming IRC messages and invokes events for the associated message type.
/// </summary>
public sealed class IrcHandler : IEquatable<IrcHandler>
{
    public event AsyncEventHandler<IrcHandler>? OnReconnectReceived;

    private readonly ChannelWriter<ChatMessage>? _messageWriter;

    private readonly RoomstateParser? _roomstateParser;
    private readonly ChannelWriter<Roomstate>? _roomstateWriter;

    private readonly MembershipMessageParser? _membershipMessageParser;
    private readonly ChannelWriter<JoinChannelMessage>? _joinWriter;
    private readonly ChannelWriter<PartChannelMessage>? _partWriter;

    private readonly NoticeParser? _noticeParser;
    private readonly ChannelWriter<Notice>? _noticeWriter;

    private readonly ChannelWriter<Bytes> _pingWriter;

    private static ReadOnlySpan<byte> JoinCommand => "JOIN"u8;

    private static ReadOnlySpan<byte> RoomstateCommand => "ROOMSTATE"u8;

    private static ReadOnlySpan<byte> PrivmsgCommand => "PRIVMSG"u8;

    private static ReadOnlySpan<byte> PingCommand => "PING"u8;

    private static ReadOnlySpan<byte> PartCommand => "PART"u8;

    private static ReadOnlySpan<byte> ReconnectCommand => "RECONNECT"u8;

    private static ReadOnlySpan<byte> NoticeCommand => "NOTICE"u8;

    // TODO: WHISPER, CLEARMSG, CLEARCHAT, USERSTATE, USERNOTICE

    private const int MaximumWhitespacesNeededToHandle = 5;

    [SuppressMessage("Style", "IDE0290:Use primary constructor")]
    public IrcHandler(
        ChannelWriter<Bytes> pingWriter,
        ChannelWriter<ChatMessage>? messageWriter,
        ChannelWriter<Roomstate>? roomstateWriter,
        ChannelWriter<JoinChannelMessage>? joinWriter,
        ChannelWriter<PartChannelMessage>? partWriter,
        ChannelWriter<Notice>? noticeWriter
    )
    {
        _messageWriter = messageWriter;

        if (roomstateWriter is not null)
        {
            _roomstateParser = new();
            _roomstateWriter = roomstateWriter;
        }

        if (joinWriter is not null || partWriter is not null)
        {
            _membershipMessageParser = new();
        }

        _joinWriter = joinWriter;
        _partWriter = partWriter;

        if (noticeWriter is not null)
        {
            _noticeParser = new();
            _noticeWriter = noticeWriter;
        }

        _pingWriter = pingWriter;
    }

    /// <summary>
    /// Handles a messages.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <returns>True, if the message was handled, otherwise false.</returns>
    [SkipLocalsInit]
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

        if (_noticeParser is null || _noticeWriter is null ||
            !thirdWord.SequenceEqual(NoticeCommand))
        {
            return false;
        }

        Notice notice = _noticeParser.Parse(ircMessage, indicesOfWhitespaces);
        bool success = _noticeWriter.TryWrite(notice);
        Debug.Assert(success);
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
        if (!firstWord.SequenceEqual(PingCommand))
        {
            return false;
        }

        Bytes pingMessage = new(ircMessage[6..]);
        bool success = _pingWriter.TryWrite(pingMessage);
        Debug.Assert(success);
        return true;
    }

    private bool HandleMoreThanOneWhitespace(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        ReadOnlySpan<byte> secondWord = ircMessage[(indicesOfWhitespaces[0] + 1)..indicesOfWhitespaces[1]];
        return HandleJoinCommand(ircMessage, indicesOfWhitespaces, secondWord) || HandlePartCommand(ircMessage, indicesOfWhitespaces, secondWord);
    }

    private bool HandlePartCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> secondWord)
    {
        if (_membershipMessageParser is null || _partWriter is null ||
            !secondWord.SequenceEqual(PartCommand))
        {
            return false;
        }

        PartChannelMessage partChannelMessage = _membershipMessageParser.ParsePartChannelMessage(ircMessage, indicesOfWhitespaces);
        bool success = _partWriter.TryWrite(partChannelMessage);
        Debug.Assert(success);
        return true;
    }

    private bool HandleJoinCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> secondWord)
    {
        if (_membershipMessageParser is null || _joinWriter is null ||
            !secondWord.SequenceEqual(JoinCommand))
        {
            return false;
        }

        JoinChannelMessage joinChannelMessage = _membershipMessageParser.ParseJoinChannelMessage(ircMessage, indicesOfWhitespaces);
        bool success = _joinWriter.TryWrite(joinChannelMessage);
        Debug.Assert(success);
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
        if (_noticeParser is null || _noticeWriter is null ||
            !secondWord.SequenceEqual(NoticeCommand))
        {
            return false;
        }

        Notice notice = _noticeParser.Parse(ircMessage, indicesOfWhitespaces);
        bool success = _noticeWriter.TryWrite(notice);
        Debug.Assert(success);
        return true;
    }

    private bool HandleRoomstateCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> thirdWord)
    {
        if (_roomstateParser is null || _roomstateWriter is null ||
            !thirdWord.SequenceEqual(RoomstateCommand))
        {
            return false;
        }

        _roomstateParser.Parse(ircMessage, indicesOfWhitespaces, out Roomstate roomstate);
        bool success = _roomstateWriter.TryWrite(roomstate);
        Debug.Assert(success);
        return true;
    }

    private bool HandlePrivMsgCommand(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces, ReadOnlySpan<byte> thirdWord)
    {
        if (_messageWriter is null || !thirdWord.SequenceEqual(PrivmsgCommand))
        {
            return false;
        }

#pragma warning disable CA2000
        ChatMessage chatMessage = ChatMessageParser.Parse(ircMessage, indicesOfWhitespaces);
#pragma warning restore CA2000
        bool success = _messageWriter.TryWrite(chatMessage);
        Debug.Assert(success);
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
