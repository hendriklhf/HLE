using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Models;

namespace HLE.Twitch;

public sealed class MembershipMessageParser : IMembershipMessageParser, IEquatable<MembershipMessageParser>
{
    [Pure]
    [SkipLocalsInit]
    public LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<char> ircMessage)
    {
        int whitespaceCount;
        if (!MemoryHelper.UseStackAlloc<int>(ircMessage.Length))
        {
            using RentedArray<int> indicesOfWhitespacesBuffer = new(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespacesBuffer.AsSpan());
            return ParseLeftChannelMessage(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespaces);
        return ParseLeftChannelMessage(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    public LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        return Parse<LeftChannelMessage>(ircMessage, indicesOfWhitespaces);
    }

    [Pure]
    [SkipLocalsInit]
    public JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage)
    {
        int whitespaceCount;
        if (!MemoryHelper.UseStackAlloc<int>(ircMessage.Length))
        {
            using RentedArray<int> indicesOfWhitespacesBuffer = new(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespacesBuffer.AsSpan());
            return ParseJoinChannelMessage(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespaces);
        return ParseJoinChannelMessage(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    public JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        return Parse<JoinChannelMessage>(ircMessage, indicesOfWhitespaces);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Parse<T>(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces) where T : IMembershipMessage<T>
    {
        ReadOnlySpan<char> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        int indexOfExclamationMark = firstWord.IndexOf('!');
        string username = new(firstWord[1..indexOfExclamationMark]);
        string channel = StringPool.Shared.GetOrAdd(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
        return T.Create(username, channel);
    }

    public bool Equals(MembershipMessageParser? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is MembershipMessageParser other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(MembershipMessageParser? left, MembershipMessageParser? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(MembershipMessageParser? left, MembershipMessageParser? right)
    {
        return !(left == right);
    }
}
