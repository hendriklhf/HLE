using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using HLE.Memory;
using HLE.Strings;
using HLE.Twitch.Tmi.Models;

namespace HLE.Twitch.Tmi;

public sealed class NoticeParser : INoticeParser, IEquatable<NoticeParser>
{
    [Pure]
    [SkipLocalsInit]
    public Notice Parse(ReadOnlySpan<byte> ircMessage)
    {
        int whitespaceCount;
        if (!MemoryHelpers.UseStackalloc<int>(ircMessage.Length))
        {
            using RentedArray<int> indicesOfWhitespacesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(ircMessage.Length);
            whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespacesBuffer.AsSpan());
            return Parse(ircMessage, indicesOfWhitespacesBuffer[..whitespaceCount]);
        }

        Span<int> indicesOfWhitespaces = stackalloc int[ircMessage.Length];
        whitespaceCount = ircMessage.IndicesOf((byte)' ', indicesOfWhitespaces);
        return Parse(ircMessage, indicesOfWhitespaces[..whitespaceCount]);
    }

    [Pure]
    [SkipLocalsInit]
    public Notice Parse(ReadOnlySpan<byte> ircMessage, ReadOnlySpan<int> indicesOfWhitespaces)
    {
        NoticeType type = NoticeType.Unknown;
        bool hasTag = ircMessage[0] == '@';
        if (hasTag)
        {
            ReadOnlySpan<byte> msgId = ircMessage[8..indicesOfWhitespaces[0]];

            Span<byte> msgIdWithoutUnderscores = stackalloc byte[msgId.Length];
            msgId.CopyTo(msgIdWithoutUnderscores);
            RemoveChar(ref msgIdWithoutUnderscores, (byte)'_');

            type = ParseNoticeType(msgIdWithoutUnderscores);
        }

        byte hasTagAsByte = Unsafe.As<bool, byte>(ref hasTag);
        ReadOnlySpan<byte> message = ircMessage[(indicesOfWhitespaces[2 + hasTagAsByte] + 2)..];

        ReadOnlySpan<byte> channel = ircMessage[(indicesOfWhitespaces[1 + hasTagAsByte] + 1)..indicesOfWhitespaces[2 + hasTagAsByte]];
        if (channel[0] == '#')
        {
            channel = channel[1..];
        }

        Encoding utf8 = Encoding.UTF8;
        return new(type, StringPool.Shared.GetOrAdd(message, utf8), StringPool.Shared.GetOrAdd(channel, utf8));
    }

    private static void RemoveChar(ref Span<byte> span, byte charToRemove)
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

    private static NoticeType ParseNoticeType(ReadOnlySpan<byte> bytes)
    {
        Span<char> chars = stackalloc char[bytes.Length];
        int charCount = Encoding.UTF8.GetChars(bytes, chars);
        return Enum.Parse<NoticeType>(chars[..charCount], true);
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
