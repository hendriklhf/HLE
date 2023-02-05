using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Twitch;

[DebuggerDisplay("{ToString()}")]
public struct MessageBuilder : IDisposable
{
    public readonly ref char this[int index] => ref Span[index];

    public readonly ref char this[Index index] => ref Span[index];

    public readonly Span<char> this[Range range] => Span[range];

    public readonly int Length => _length;

    public readonly int Capacity => _buffer.Length;

    public readonly Span<char> Span => _buffer;

    public readonly Memory<char> Memory => _buffer;

    public readonly ReadOnlyMemory<char> Message => Memory[.._length];

    private readonly char[] _buffer;
    private ushort _length;

    private const ushort _maxMessageLength = 500;

    public MessageBuilder()
    {
        _buffer = ArrayPool<char>.Shared.Rent(_maxMessageLength);
    }

    public MessageBuilder(int bufferLength)
    {
        _buffer = ArrayPool<char>.Shared.Rent(bufferLength);
    }

    public readonly void Dispose()
    {
        ArrayPool<char>.Shared.Return(_buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        span.CopyTo(Span[_length..]);
        _length += (ushort)span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2)
    {
        Append(span);
        span2.CopyTo(Span[_length..]);
        _length += (ushort)span2.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3)
    {
        Append(span, span2);
        span3.CopyTo(Span[_length..]);
        _length += (ushort)span3.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4)
    {
        Append(span, span2, span3);
        span4.CopyTo(Span[_length..]);
        _length += (ushort)span4.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5)
    {
        Append(span, span2, span3, span4);
        span5.CopyTo(Span[_length..]);
        _length += (ushort)span5.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6)
    {
        Append(span, span2, span3, span4, span5);
        span6.CopyTo(Span[_length..]);
        _length += (ushort)span6.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7)
    {
        Append(span, span2, span3, span4, span5, span6);
        span7.CopyTo(Span[_length..]);
        _length += (ushort)span7.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8)
    {
        Append(span, span2, span3, span4, span5, span6, span7);
        span8.CopyTo(Span[_length..]);
        _length += (ushort)span8.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8, scoped ReadOnlySpan<char> span9)
    {
        Append(span, span2, span3, span4, span5, span6, span7, span8);
        span9.CopyTo(Span[_length..]);
        _length += (ushort)span9.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8, scoped ReadOnlySpan<char> span9, scoped ReadOnlySpan<char> span10)
    {
        Append(span, span2, span3, span4, span5, span6, span7, span8, span9);
        span10.CopyTo(Span[_length..]);
        _length += (ushort)span10.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c) => Span[_length++] = c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2)
    {
        Append(c);
        Span[_length++] = c2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3)
    {
        Append(c, c2);
        Span[_length++] = c3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4)
    {
        Append(c, c2, c3);
        Span[_length++] = c4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5)
    {
        Append(c, c2, c3, c4);
        Span[_length++] = c5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6)
    {
        Append(c, c2, c3, c4, c5);
        Span[_length++] = c6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7)
    {
        Append(c, c2, c3, c4, c5, c6);
        Span[_length++] = c7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8)
    {
        Append(c, c2, c3, c4, c5, c6, c7);
        Span[_length++] = c8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8, char c9)
    {
        Append(c, c2, c3, c4, c5, c6, c7, c8);
        Span[_length++] = c9;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8, char c9, char c10)
    {
        Append(c, c2, c3, c4, c5, c6, c7, c8, c9);
        Span[_length++] = c10;
    }

    public void Remove(int index)
    {
        Span[(index + 1).._length].CopyTo(Span[index..]);
        _length--;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
    {
        return new(Span[.._length]);
    }

    [Pure]
    public readonly bool Equals(MessageBuilder builder, StringComparison comparisonType = default)
    {
        return ((ReadOnlySpan<char>)Span[.._length]).Equals(builder.Span[..builder._length], comparisonType);
    }

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType = default)
    {
        return ((ReadOnlySpan<char>)Span[.._length]).Equals(str, comparisonType);
    }
}
