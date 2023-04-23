using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
public ref partial struct ValueStringBuilder
{
    public readonly ref char this[int index] => ref _buffer[index];

    public readonly ref char this[Index index] => ref _buffer[index];

    public readonly Span<char> this[Range range] => _buffer[range];

    public readonly Span<char> Buffer => _buffer;

    public readonly ReadOnlySpan<char> WrittenSpan => _buffer[.._length];

    public readonly Span<char> FreeBuffer => _buffer[_length..];

    public readonly int Length => _length;

    public readonly int Capacity => _buffer.Length;

    public readonly int FreeBufferSize => _buffer.Length - _length;

    private readonly Span<char> _buffer = Span<char>.Empty;
    private int _length = 0;

    public static ValueStringBuilder Empty => new();

    public ValueStringBuilder()
    {
    }

    public ValueStringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
    }

    public unsafe ValueStringBuilder(char* pointer, int length)
    {
        _buffer = new(pointer, length);
    }

    public ValueStringBuilder(ref char reference, int length)
    {
        _buffer = MemoryMarshal.CreateSpan(ref reference, length);
    }

    public void Advance(int length)
    {
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
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(value));
        }

        Advance(charsWritten);
    }

    public void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!dateTime.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(dateTime));
        }

        Advance(charsWritten);
    }

    public void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!timeSpan.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(timeSpan));
        }

        Advance(charsWritten);
    }

    public void Append<TSpanFormattable, TFormatProvider>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default, TFormatProvider? formatProvider = default) where TSpanFormattable : ISpanFormattable where TFormatProvider : IFormatProvider
    {
        if (!spanFormattable.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(spanFormattable));
        }

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
        WrittenSpan.Replace(oldChar, newChar);
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

    [Pure]
    public readonly char[] ToCharArray()
    {
        return _buffer[.._length].ToArray();
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return false;
    }

    [Pure]
    public readonly bool Equals(ValueStringBuilder other)
    {
        return Unsafe.AreSame(ref MemoryMarshal.GetReference(_buffer), ref MemoryMarshal.GetReference(other._buffer)) && _length == other._length;
    }

    [Pure]
    public readonly bool Equals(ValueStringBuilder other, StringComparison comparisonType)
    {
        return WrittenSpan.Equals(other.WrittenSpan, comparisonType);
    }

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> span, StringComparison comparisonType)
    {
        return WrittenSpan.Equals(span, comparisonType);
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly unsafe int GetHashCode()
    {
        return HashCode.Combine((nuint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(_buffer)), _length);
    }

    [Pure]
    public readonly int GetHashCode(StringComparison comparisonType)
    {
        return string.GetHashCode(WrittenSpan, comparisonType);
    }

    private static ArgumentException NotEnoughSpaceException(string paramName)
    {
        return new("There was not enough space left in the buffer to write the provided value to the buffer.", paramName);
    }

    public static implicit operator ValueStringBuilder(Span<char> buffer) => new(buffer);

    public static implicit operator ValueStringBuilder(char[] buffer) => new(buffer);

    public static bool operator ==(ValueStringBuilder left, ValueStringBuilder right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueStringBuilder left, ValueStringBuilder right)
    {
        return !(left == right);
    }
}
