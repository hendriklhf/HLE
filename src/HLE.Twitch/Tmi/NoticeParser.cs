using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public sealed class NoticeParser : INoticeParser, IEquatable<NoticeParser>
{
    [Pure]
    [SkipLocalsInit]
    public Notice Parse(ReadOnlySpan<char> ircMessage)
    {
        int whitespaceCount;
        if (!MemoryHelpers.UseStackAlloc<int>(ircMessage.Length))
        {
            using RentedArray<int> indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespacesBuffer.AsSpan());
            return Parse(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf(' ', indicesOfWhitespaces);
        return Parse(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    [SkipLocalsInit]
    public Notice Parse(ReadOnlySpan<char> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        NoticeType type = NoticeType.Unknown;
        bool hasTag = ircMessage[0] == '@';
        if (hasTag)
        {
            ReadOnlySpan<char> msgId = ircMessage[8..indicesOfWhitespaces[0]];

            Span<char> msgIdWithoutUnderscores = stackalloc char[msgId.Length];
            msgId.CopyTo(msgIdWithoutUnderscores);
            RemoveChar(ref msgIdWithoutUnderscores, '_');

            type = Enum.Parse<NoticeType>(msgIdWithoutUnderscores, true);
        }

        byte hasTagAsByte = Unsafe.As<bool, byte>(ref hasTag);
        ReadOnlySpan<char> message = ircMessage[(indicesOfWhitespaces[2 + hasTagAsByte] + 2)..];

        ReadOnlySpan<char> channel = ircMessage[(indicesOfWhitespaces[1 + hasTagAsByte] + 1)..indicesOfWhitespaces[2 + hasTagAsByte]];
        if (channel[0] == '#')
        {
            channel = channel[1..];
        }

        return new(type, StringPool.Shared.GetOrAdd(message), StringPool.Shared.GetOrAdd(channel));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RemoveChar(ref Span<char> span, char charToRemove)
    {
        int indexOfChar = span.IndexOf(charToRemove);
        while (indexOfChar >= 0)
        {
            span[(indexOfChar + 1)..].CopyTo(span[indexOfChar..]);
            span = span[..^1];
            int lastIndex = indexOfChar;
            indexOfChar = span[indexOfChar..].IndexOf(charToRemove);
            if (indexOfChar >= 0)
            {
                indexOfChar += lastIndex;
            }
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] NoticeParser? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(NoticeParser? left, NoticeParser? right) => Equals(left, right);

    public static bool operator !=(NoticeParser? left, NoticeParser? right) => !(left == right);
}
