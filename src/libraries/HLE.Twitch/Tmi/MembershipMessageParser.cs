using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Collections;
using HLE.Memory;
using HLE.Text;

namespace HLE.Twitch.Tmi;

public sealed class MembershipMessageParser : IMembershipMessageParser, IEquatable<MembershipMessageParser>
{
    [Pure]
    [SkipLocalsInit]
    public LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<byte> ircMessage)
    {
        int whitespaceCount;
        if (!MemoryHelpers.UseStackalloc<int>(ircMessage.Length))
        {
            int[] indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.Rent(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespacesBuffer.AsSpan());
            LeftChannelMessage result = ParseLeftChannelMessage(ircMessage, indicesOfWhitespacesBuffer.AsSpanUnsafe(..whitespaceCount));
            ArrayPool<int>.Shared.Return(indicesOfWhitespacesBuffer);
            return result;
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespaces);
        return ParseLeftChannelMessage(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    public LeftChannelMessage ParseLeftChannelMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => Parse<LeftChannelMessage>(ircMessage, indicesOfWhitespaces);

    [Pure]
    [SkipLocalsInit]
    public JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<byte> ircMessage)
    {
        int whitespaceCount;
        if (!MemoryHelpers.UseStackalloc<int>(ircMessage.Length))
        {
            int[] indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.Rent(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespacesBuffer.AsSpan());
            JoinChannelMessage result = ParseJoinChannelMessage(ircMessage, indicesOfWhitespacesBuffer.AsSpanUnsafe(..whitespaceCount));
            ArrayPool<int>.Shared.Return(indicesOfWhitespacesBuffer);
            return result;
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespaces);
        return ParseJoinChannelMessage(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    public JoinChannelMessage ParseJoinChannelMessage(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
        => Parse<JoinChannelMessage>(ircMessage, indicesOfWhitespaces);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T Parse<T>(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces) where T : IMembershipMessage<T>
    {
        ReadOnlySpan<byte> firstWord = ircMessage[..indicesOfWhitespaces[0]];
        int indexOfExclamationMark = firstWord.IndexOf((byte)'!');
        Encoding utf8 = Encoding.UTF8;
        string username = utf8.GetString(firstWord[1..indexOfExclamationMark]);
        string channel = StringPool.Shared.GetOrAdd(ircMessage[(indicesOfWhitespaces[^1] + 2)..], utf8);
        return T.Create(username, channel);
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] MembershipMessageParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(MembershipMessageParser? left, MembershipMessageParser? right) => Equals(left, right);

    public static bool operator !=(MembershipMessageParser? left, MembershipMessageParser? right) => !(left == right);
}
