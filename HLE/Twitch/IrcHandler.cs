using System;
using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Chatterino;
using HLE.Twitch.Models;

namespace HLE.Twitch;

/// <summary>
/// A class that handles incoming IRC messages.
/// </summary>
public sealed class IrcHandler : IEquatable<IrcHandler>
{
    /// <summary>
    /// Is invoked if a JOIN message has been received.
    /// </summary>
    public event EventHandler<JoinedChannelArgs>? OnJoinedReceived;

    /// <summary>
    /// Is invoked if a PART message has been received.
    /// </summary>
    public event EventHandler<LeftChannelArgs>? OnLeftReceived;

    /// <summary>
    /// Is invoked if a ROOMSTATE message has been received.
    /// </summary>
    public event EventHandler<RoomstateArgs>? OnRoomstateReceived;

    /// <summary>
    /// Is invoked if a PRIVMSG message has been received.
    /// </summary>
    public event EventHandler<IChatMessage>? OnChatMessageReceived;

    /// <summary>
    /// Is invoked if a RECONNECT message has been received.
    /// </summary>
    public event EventHandler? OnReconnectReceived;

    /// <summary>
    /// Is invoked if a PING message has been received.
    /// </summary>
    public event EventHandler<ReceivedData>? OnPingReceived;

    private readonly IrcParser _ircParser;

    private const string _joinCommand = "JOIN";
    private const string _roomstateCommand = "ROOMSTATE";
    private const string _privmsgCommand = "PRIVMSG";
    private const string _pingCommand = "PING";
    private const string _partCommand = "PART";
    private const string _reconnectCommand = "RECONNECT";

    public IrcHandler(ParsingMode parsingMode)
    {
        _ircParser = new(parsingMode);
    }

    /// <summary>
    /// Handles a messages and invokes an event, if the message could be handled.
    /// </summary>
    /// <param name="ircMessage">The IRC message.</param>
    /// <returns>True, if an event has been invoked, otherwise false.</returns>
    public bool Handle(ReadOnlySpan<char> ircMessage)
    {
        Span<int> indicesOfWhitespace = stackalloc int[ircMessage.Length];
        int whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespace);

        return whitespaceCount switch
        {
            > 2 => HandleMoreThanTwoWhitespaces(ircMessage, indicesOfWhitespace, whitespaceCount),
            > 1 => HandleMoreThanOneWhitespace(ircMessage, indicesOfWhitespace, whitespaceCount),
            > 0 => HandleMoreThanZeroWhitespaces(ircMessage, indicesOfWhitespace),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleMoreThanZeroWhitespaces(ReadOnlySpan<char> ircMessage, Span<int> indicesOfWhitespace)
    {
        ReadOnlySpan<char> firstWord = ircMessage[..indicesOfWhitespace[0]];
        if (firstWord.SequenceEqual(_pingCommand))
        {
            OnPingReceived?.Invoke(this, ReceivedData.Create(ircMessage[6..]));
            return true;
        }

        ReadOnlySpan<char> secondWord = ircMessage[(indicesOfWhitespace[0] + 1)..];
        if (!secondWord.SequenceEqual(_reconnectCommand))
        {
            return false;
        }

        OnReconnectReceived?.Invoke(this, EventArgs.Empty);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleMoreThanOneWhitespace(ReadOnlySpan<char> ircMessage, Span<int> indicesOfWhitespace, int whitespaceCount)
    {
        ReadOnlySpan<char> secondWord = ircMessage[(indicesOfWhitespace[0] + 1)..indicesOfWhitespace[1]];
        if (secondWord.SequenceEqual(_joinCommand))
        {
            OnJoinedReceived?.Invoke(this, new(ircMessage, indicesOfWhitespace[..whitespaceCount]));
            return true;
        }

        if (!secondWord.Equals(_partCommand, StringComparison.Ordinal))
        {
            return false;
        }

        OnLeftReceived?.Invoke(this, new(ircMessage, indicesOfWhitespace[..whitespaceCount]));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleMoreThanTwoWhitespaces(ReadOnlySpan<char> ircMessage, Span<int> indicesOfWhitespace, int whitespaceCount)
    {
        ReadOnlySpan<char> thirdWord = ircMessage[(indicesOfWhitespace[1] + 1)..indicesOfWhitespace[2]];
        if (thirdWord.SequenceEqual(_privmsgCommand))
        {
            OnChatMessageReceived?.Invoke(this, _ircParser.ParseChatMessage(ircMessage, indicesOfWhitespace[..whitespaceCount]));
            return true;
        }

        if (!thirdWord.SequenceEqual(_roomstateCommand))
        {
            return false;
        }

        OnRoomstateReceived?.Invoke(this, new(ircMessage, indicesOfWhitespace[..whitespaceCount]));
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
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }
}
