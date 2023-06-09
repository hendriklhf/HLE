using System;
using HLE.Memory;
using HLE.Twitch.Chatterino;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public sealed class IrcParser : IEquatable<IrcParser>
{
    private readonly IChatMessageParser _chatMessageParser;

    public IrcParser(ParsingMode parsingMode = ParsingMode.Balanced)
    {
        _chatMessageParser = parsingMode switch
        {
            ParsingMode.TimeEfficient => new TimeEfficientChatMessageParser(),
            ParsingMode.Balanced => new BalancedChatMessageParser(),
            ParsingMode.MemoryEfficient => new MemoryEfficientChatMessageParser(),
            _ => throw new ArgumentOutOfRangeException(nameof(parsingMode), parsingMode, null)
        };
    }

    public IChatMessage ParseChatMessage(ReadOnlySpan<char> ircMessage)
    {
        return _chatMessageParser.Parse(ircMessage);
    }

    public IChatMessage ParseChatMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        return _chatMessageParser.Parse(ircMessage, indicesOfWhitespaces);
    }

    public bool Equals(IrcParser? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is IrcParser other && Equals(other);
    }

    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }

    public static bool operator ==(IrcParser? left, IrcParser? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(IrcParser? left, IrcParser? right)
    {
        return !(left == right);
    }
}
