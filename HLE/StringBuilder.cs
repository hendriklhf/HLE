using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE;

[DebuggerDisplay("\"{ToString()}\"")]
public ref struct StringBuilder
{
    public readonly ref char this[int index] => ref _buffer[index];

    public readonly ref char this[Index index] => ref _buffer[index];

    public readonly Span<char> this[Range range] => _buffer[range];

    public readonly ReadOnlySpan<char> WrittenSpan => _buffer[.._length];

    public readonly int Length => _length;

    public readonly int Capacity => _buffer.Length;

    private readonly Span<char> _buffer = Span<char>.Empty;
    private int _length = 0;

    public static StringBuilder Empty => new();

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

    public StringBuilder(ref char reference, int length)
    {
        _buffer = MemoryMarshal.CreateSpan(ref reference, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        span.CopyTo(_buffer[_length..]);
        _length += span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2)
    {
        Append(span1);
        span2.CopyTo(_buffer[_length..]);
        _length += span2.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3)
    {
        Append(span1, span2);
        span3.CopyTo(_buffer[_length..]);
        _length += span3.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4)
    {
        Append(span1, span2, span3);
        span4.CopyTo(_buffer[_length..]);
        _length += span4.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5)
    {
        Append(span1, span2, span3, span4);
        span5.CopyTo(_buffer[_length..]);
        _length += span5.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6)
    {
        Append(span1, span2, span3, span4, span5);
        span6.CopyTo(_buffer[_length..]);
        _length += span6.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7)
    {
        Append(span1, span2, span3, span4, span5, span6);
        span7.CopyTo(_buffer[_length..]);
        _length += span7.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8)
    {
        Append(span1, span2, span3, span4, span5, span6, span7);
        span8.CopyTo(_buffer[_length..]);
        _length += span8.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8, scoped ReadOnlySpan<char> span9)
    {
        Append(span1, span2, span3, span4, span5, span6, span7, span8);
        span9.CopyTo(_buffer[_length..]);
        _length += span9.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span1, scoped ReadOnlySpan<char> span2, scoped ReadOnlySpan<char> span3, scoped ReadOnlySpan<char> span4, scoped ReadOnlySpan<char> span5, scoped ReadOnlySpan<char> span6,
        scoped ReadOnlySpan<char> span7, scoped ReadOnlySpan<char> span8, scoped ReadOnlySpan<char> span9, scoped ReadOnlySpan<char> span10)
    {
        Append(span1, span2, span3, span4, span5, span6, span7, span8, span9);
        span10.CopyTo(_buffer[_length..]);
        _length += span10.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c) => _buffer[_length++] = c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2)
    {
        Append(char1);
        _buffer[_length++] = char2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3)
    {
        Append(char1, char2);
        _buffer[_length++] = char3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3, char char4)
    {
        Append(char1, char2, char3);
        _buffer[_length++] = char4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3, char char4, char char5)
    {
        Append(char1, char2, char3, char4);
        _buffer[_length++] = char5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3, char char4, char char5, char char6)
    {
        Append(char1, char2, char3, char4, char5);
        _buffer[_length++] = char6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3, char char4, char char5, char char6, char char7)
    {
        Append(char1, char2, char3, char4, char5, char6);
        _buffer[_length++] = char7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3, char char4, char char5, char char6, char char7, char char8)
    {
        Append(char1, char2, char3, char4, char5, char6, char7);
        _buffer[_length++] = char8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3, char char4, char char5, char char6, char char7, char char8, char char9)
    {
        Append(char1, char2, char3, char4, char5, char6, char7, char8);
        _buffer[_length++] = char9;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char char1, char char2, char char3, char char4, char char5, char char6, char char7, char char8, char char9, char char10)
    {
        Append(char1, char2, char3, char4, char5, char6, char7, char8, char9);
        _buffer[_length++] = char10;
    }

    public void Append(byte value)
    {
        Span<char> chars = stackalloc char[3];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(sbyte value)
    {
        Span<char> chars = stackalloc char[4];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(short value)
    {
        Span<char> chars = stackalloc char[6];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(ushort value)
    {
        Span<char> chars = stackalloc char[5];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(int value)
    {
        Span<char> chars = stackalloc char[11];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(uint value)
    {
        Span<char> chars = stackalloc char[10];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(long value)
    {
        Span<char> chars = stackalloc char[20];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(ulong value)
    {
        Span<char> chars = stackalloc char[20];
        value.TryFormat(chars, out int charsWritten);
        Append(chars[..charsWritten]);
    }

    public void Append(ISpanFormattable spanFormattable, ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        spanFormattable.TryFormat(_buffer[_length..], out int charsWritten, format, formatProvider);
        _length += charsWritten;
    }

    public void Remove(int index, int length = 1)
    {
        _buffer[(index + length).._length].CopyTo(_buffer[index..]);
        _length -= length;
    }

    public readonly void Replace(char oldChar, char newChar)
    {
        _buffer[.._length].Replace(oldChar, newChar);
    }

    public void Clear()
    {
        _length = 0;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
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
