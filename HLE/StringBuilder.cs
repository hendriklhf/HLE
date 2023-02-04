using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE;

[DebuggerDisplay("\"{ToString()}\"")]
public ref struct StringBuilder
{
    public readonly ref char this[int index] => ref _buffer[index];

    public readonly ref char this[Index index] => ref _buffer[index];

    public readonly ReadOnlySpan<char> this[Range range] => _buffer[range];

    public readonly int Length => _length;

    public readonly int Capacity => _buffer.Length;

    private readonly Span<char> _buffer = Span<char>.Empty;
    private int _length;

    public StringBuilder()
    {
    }

    public StringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
    }

    public unsafe StringBuilder(char* pointer, int length)
    {
        _buffer = new(pointer, length);
    }

    public unsafe StringBuilder(ref char reference, int length)
    {
        _buffer = new((char*)Unsafe.AsPointer(ref reference), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        span.CopyTo(_buffer[_length..]);
        _length += span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2)
    {
        Append(span);
        span2.CopyTo(_buffer[_length..]);
        _length += span2.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3)
    {
        Append(span, span2);
        span3.CopyTo(_buffer[_length..]);
        _length += span3.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4)
    {
        Append(span, span2, span3);
        span4.CopyTo(_buffer[_length..]);
        _length += span4.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5)
    {
        Append(span, span2, span3, span4);
        span5.CopyTo(_buffer[_length..]);
        _length += span5.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6)
    {
        Append(span, span2, span3, span4, span5);
        span6.CopyTo(_buffer[_length..]);
        _length += span6.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7)
    {
        Append(span, span2, span3, span4, span5, span6);
        span7.CopyTo(_buffer[_length..]);
        _length += span7.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8)
    {
        Append(span, span2, span3, span4, span5, span6, span7);
        span8.CopyTo(_buffer[_length..]);
        _length += span8.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8, scoped ReadOnlySpan<char> span9)
    {
        Append(span, span2, span3, span4, span5, span6, span7, span8);
        span9.CopyTo(_buffer[_length..]);
        _length += span9.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8, scoped ReadOnlySpan<char> span9, scoped ReadOnlySpan<char> span10)
    {
        Append(span, span2, span3, span4, span5, span6, span7, span8, span9);
        span10.CopyTo(_buffer[_length..]);
        _length += span10.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c) => _buffer[_length++] = c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2)
    {
        Append(c);
        _buffer[_length++] = c2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3)
    {
        Append(c, c2);
        _buffer[_length++] = c3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4)
    {
        Append(c, c2, c3);
        _buffer[_length++] = c4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5)
    {
        Append(c, c2, c3, c4);
        _buffer[_length++] = c5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6)
    {
        Append(c, c2, c3, c4, c5);
        _buffer[_length++] = c6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7)
    {
        Append(c, c2, c3, c4, c5, c6);
        _buffer[_length++] = c7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8)
    {
        Append(c, c2, c3, c4, c5, c6, c7);
        _buffer[_length++] = c8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8, char c9)
    {
        Append(c, c2, c3, c4, c5, c6, c7, c8);
        _buffer[_length++] = c9;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c, char c2, char c3, char c4, char c5, char c6, char c7, char c8, char c9, char c10)
    {
        Append(c, c2, c3, c4, c5, c6, c7, c8, c9);
        _buffer[_length++] = c10;
    }

    public void Remove(int index)
    {
        _buffer[(index + 1).._length].CopyTo(_buffer[index..]);
        _length--;
    }

    [Pure]
    public override string ToString()
    {
        return new(_buffer[.._length]);
    }

    [Pure]
    public readonly bool Equals(StringBuilder builder, StringComparison comparisonType = default)
    {
        return ((ReadOnlySpan<char>)_buffer[.._length]).Equals(builder._buffer[..builder._length], comparisonType);
    }

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType = default)
    {
        return ((ReadOnlySpan<char>)_buffer[.._length]).Equals(str, comparisonType);
    }

    public static implicit operator StringBuilder(Span<char> buffer) => new(buffer);

    public static implicit operator StringBuilder(char[] buffer) => new(buffer);
}
