using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE;

[DebuggerDisplay("\"{ToString()}\"")]
public ref partial struct StringBuilder
{
    public readonly ref char this[int index] => ref _buffer[index];

    public readonly ref char this[Index index] => ref _buffer[index];

    public readonly Span<char> this[Range range] => _buffer[range];

    public readonly ReadOnlySpan<char> WrittenSpan => _buffer[.._length];

    public readonly Span<char> FreeBuffer => _buffer[_length..];

    public readonly int Length => _length;

    public readonly int Capacity => _buffer.Length;

    public readonly int FreeBufferSize => _buffer.Length - _length;

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

    public void Advance(int length)
    {
        if (length < 0)
        {
            throw new ArgumentException($"Parameter {nameof(length)} must be a positive number.", nameof(length));
        }

        _length += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        span.CopyTo(FreeBuffer);
        _length += span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        _buffer[_length++] = c;
    }

    public void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        dateTime.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        timeSpan.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Append(ISpanFormattable spanFormattable, ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        spanFormattable.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider);
        Advance(charsWritten);
    }

    public void Remove(int index, int length = 1)
    {
        _buffer[(index + length).._length].CopyTo(_buffer[index..]);
        _length -= length;
    }

#if NET8_0_OR_GREATER
    public readonly void Replace(char oldChar, char newChar)
    {
        _buffer[.._length].Replace(oldChar, newChar);
    }
#endif

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

    public readonly char[] ToCharArray()
    {
        return _buffer[.._length].ToArray();
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
