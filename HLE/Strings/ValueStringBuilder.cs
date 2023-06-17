using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
public ref partial struct ValueStringBuilder
{
    public readonly ref char this[int index] => ref WrittenSpan.AsMutableSpan()[index];

    public readonly ref char this[Index index] => ref WrittenSpan.AsMutableSpan()[index];

    public readonly Span<char> this[Range range] => WrittenSpan.AsMutableSpan()[range];

    public int Length { get; private set; }

    public readonly int Capacity => _buffer.Length;

    public readonly Span<char> BufferSpan => _buffer;

    public readonly ReadOnlySpan<char> WrittenSpan => BufferSpan[..Length];

    public readonly Span<char> FreeBuffer => BufferSpan[Length..];

    public readonly int FreeBufferSize => Capacity - Length;

    private readonly Span<char> _buffer = Span<char>.Empty;

    public static ValueStringBuilder Empty => new();

    public ValueStringBuilder()
    {
    }

    public ValueStringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int length)
    {
        Length += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        span.CopyTo(FreeBuffer);
        Length += span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        _buffer[Length++] = c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<byte, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<sbyte, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<short, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<ushort, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<int, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<uint, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<long, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<ulong, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<float, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<double, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<DateTime, IFormatProvider>(dateTime, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(DateTimeOffset dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<DateTimeOffset, IFormatProvider>(dateTime, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<TimeSpan, IFormatProvider>(timeSpan, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<TSpanFormattable, TFormatProvider>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default, TFormatProvider? formatProvider = default) where TSpanFormattable : ISpanFormattable where TFormatProvider : IFormatProvider
    {
        if (!spanFormattable.TryFormat(FreeBuffer, out int charsWritten, format, formatProvider))
        {
            throw NotEnoughSpaceException(nameof(spanFormattable));
        }

        Advance(charsWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Length = 0;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
    {
        return new(WrittenSpan);
    }

    [Pure]
    public readonly char[] ToCharArray()
    {
        return _buffer[..Length].ToArray();
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
        ref char bufferReference = ref MemoryMarshal.GetReference(_buffer);
        ref char otherBufferReference = ref MemoryMarshal.GetReference(other._buffer);
        return Unsafe.AreSame(ref bufferReference, ref otherBufferReference) && Length == other.Length;
    }

    [Pure]
    public readonly bool Equals(ValueStringBuilder other, StringComparison comparisonType)
    {
        return Equals(other.WrittenSpan, comparisonType);
    }

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType)
    {
        return WrittenSpan.Equals(str, comparisonType);
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly unsafe int GetHashCode()
    {
        return HashCode.Combine((nuint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(_buffer)), Length);
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
