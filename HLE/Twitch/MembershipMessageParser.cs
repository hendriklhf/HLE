using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
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
        if (!MemoryHelpers.UseStackAlloc<int>(ircMessage.Length))
        {
            using RentedArray<int> indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespacesBuffer.AsSpan());
            return ParseLeftChannelMessage(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespaces);
        return ParseLeftChannelMessage(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    public LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => Parse<LeftChannelMessage>(ircMessage, indicesOfWhitespaces);

    [Pure]
    [SkipLocalsInit]
    public JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage)
    {
        int whitespaceCount;
        if (!MemoryHelpers.UseStackAlloc<int>(ircMessage.Length))
        {
            using RentedArray<int> indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespacesBuffer.AsSpan());
            return ParseJoinChannelMessage(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespaces);
        return ParseJoinChannelMessage(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    public JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => Parse<JoinChannelMessage>(ircMessage, indicesOfWhitespaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Parse<T>(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces) where T : IMembershipMessage<T>
    {
        ReadOnlySpan<char> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        int indexOfExclamationMark = firstWord.IndexOf('!');
        string username = new(firstWord[1..indexOfExclamationMark]);
        string channel = StringPool.Shared.GetOrAdd(ircMessage[(indicesOfWhitespaces[^1] + 2)..]);
        return T.Create(username, channel);
    }

    [Pure]
    public bool Equals(MembershipMessageParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(MembershipMessageParser? left, MembershipMessageParser? right) => Equals(left, right);

    public static bool operator !=(MembershipMessageParser? left, MembershipMessageParser? right) => !(left == right);
}
