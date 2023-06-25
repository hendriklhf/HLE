using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
public partial struct PoolBufferStringBuilder : IDisposable, ICollection<char>, IEquatable<PoolBufferStringBuilder>, ICopyable<char>, ICountable, IRefIndexAccessible<char>
{
    public readonly ref char this[int index] => ref WrittenSpan.AsMutableSpan()[index];

    public readonly ref char this[Index index] => ref WrittenSpan.AsMutableSpan()[index];

    public readonly Span<char> this[Range range] => WrittenSpan.AsMutableSpan()[range];

    public int Length { get; private set; }

    readonly int ICollection<char>.Count => Length;

    readonly int ICountable.Count => Length;

    public readonly int Capacity => _buffer.Length;

    public readonly Span<char> BufferSpan => _buffer;

    public readonly Memory<char> BufferMemory => _buffer;

    public readonly ReadOnlySpan<char> WrittenSpan => BufferSpan[..Length];

    public readonly ReadOnlyMemory<char> WrittenMemory => BufferMemory[..Length];

    public readonly Span<char> FreeBufferSpan => BufferSpan[Length..];

    public readonly Memory<char> FreeBufferMemory => BufferMemory[Length..];

    public readonly int FreeBufferSize => Capacity - Length;

    readonly bool ICollection<char>.IsReadOnly => false;

    private RentedArray<char> _buffer = RentedArray<char>.Empty;

    public const int DefaultBufferSize = 32;

    public static PoolBufferStringBuilder Empty => new(RentedArray<char>.Empty);

    private PoolBufferStringBuilder(RentedArray<char> buffer)
    {
        _buffer = buffer;
    }

    public PoolBufferStringBuilder()
    {
        _buffer = new(DefaultBufferSize);
    }

    public PoolBufferStringBuilder(int initialBufferSize)
    {
        _buffer = new(initialBufferSize);
    }

    public readonly void Dispose()
    {
        _buffer.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowBuffer()
    {
        RentedArray<char> newBuffer = new(_buffer.Length << 1);
        Debug.Assert(newBuffer.Length > _buffer.Length);
        MemoryHelper.CopyUnsafe(_buffer.Span, newBuffer.Span);
        _buffer.Dispose();
        _buffer = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int length)
    {
        Length += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        if (FreeBufferSize < span.Length)
        {
            GrowBuffer();
        }

        span.CopyTo(FreeBufferSpan);
        Advance(span.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        if (FreeBufferSize <= 0)
        {
            GrowBuffer();
        }

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
    public void Append(Int128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<Int128, IFormatProvider>(value, format, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(UInt128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        Append<UInt128, IFormatProvider>(value, format, formatProvider);
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

    public void Append<TSpanFormattable, TFormatProvider>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default, TFormatProvider? formatProvider = default)
        where TSpanFormattable : ISpanFormattable where TFormatProvider : IFormatProvider
    {
        const int maximumFormattingTries = 10;
        int countOfFailedTries = 0;
        while (true)
        {
            if (countOfFailedTries == maximumFormattingTries)
            {
                throw new InvalidOperationException($"Trying to format the {typeof(TSpanFormattable)} failed {countOfFailedTries} times. The method aborted.");
            }

            if (!spanFormattable.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
            {
                countOfFailedTries++;
                GrowBuffer();
                continue;
            }

            Advance(charsWritten);
            break;
        }
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

    public readonly void CopyTo(List<char> destination, int offset = 0)
    {
        DefaultCopyableCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination, offset);
    }

    public readonly void CopyTo(char[] destination, int offset = 0)
    {
        DefaultCopyableCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination, offset);
    }

    public readonly void CopyTo(Memory<char> destination)
    {
        DefaultCopyableCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    public readonly void CopyTo(Span<char> destination)
    {
        DefaultCopyableCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    public readonly void CopyTo(ref char destination)
    {
        DefaultCopyableCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(ref destination);
    }

    public readonly unsafe void CopyTo(char* destination)
    {
        DefaultCopyableCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    void ICollection<char>.Add(char c)
    {
        Append(c);
    }

    readonly bool ICollection<char>.Contains(char c)
    {
        return WrittenSpan.Contains(c);
    }

    readonly bool ICollection<char>.Remove(char c)
    {
        throw new NotSupportedException();
    }

    [Pure]
    public readonly IEnumerator<char> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return WrittenSpan[i];
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    public readonly bool Equals(PoolBufferStringBuilder builder, StringComparison comparisonType)
    {
        return Equals(builder.WrittenSpan, comparisonType);
    }

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType)
    {
        return WrittenSpan.Equals(str, comparisonType);
    }

    public readonly bool Equals(PoolBufferStringBuilder other)
    {
        return Length == other.Length && _buffer.Equals(other._buffer);
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj is PoolBufferStringBuilder other && Equals(other);
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_buffer, Length);
    }

    [Pure]
    public readonly int GetHashCode(StringComparison comparisonType)
    {
        return string.GetHashCode(WrittenSpan, comparisonType);
    }

    public static bool operator ==(PoolBufferStringBuilder left, PoolBufferStringBuilder right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PoolBufferStringBuilder left, PoolBufferStringBuilder right)
    {
        return !(left == right);
    }
}
