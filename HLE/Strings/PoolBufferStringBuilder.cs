using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
public partial struct PoolBufferStringBuilder : IDisposable, IEquatable<PoolBufferStringBuilder>, ICopyable<char>
{
    public readonly ref char this[int index] => ref WrittenSpan.AsMutableSpan()[index];

    public readonly ref char this[Index index] => ref WrittenSpan.AsMutableSpan()[index];

    public readonly Span<char> this[Range range] => WrittenSpan.AsMutableSpan()[range];

    public int Length { get; private set; }

    public readonly int Capacity => _buffer.Length;

    public readonly Span<char> BufferSpan => _buffer;

    public readonly Memory<char> BufferMemory => _buffer;

    public readonly ReadOnlySpan<char> WrittenSpan => BufferSpan[..Length];

    public readonly ReadOnlyMemory<char> WrittenMemory => BufferMemory[..Length];

    public readonly Span<char> FreeBufferSpan => BufferSpan[Length..];

    public readonly Memory<char> FreeBufferMemory => BufferMemory[Length..];

    public readonly int FreeBufferSize => Capacity - Length;

    private RentedArray<char> _buffer = RentedArray<char>.Empty;

    public const int DefaultBufferSize = 64;
    private const int _minimumGrowth = 128;

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
    private void GrowBuffer(int size = _minimumGrowth)
    {
        if (size < _minimumGrowth)
        {
            size = _minimumGrowth;
        }

        char[] newBuffer = ArrayPool<char>.Shared.Rent(_buffer.Length + size);
        WrittenSpan.CopyTo(newBuffer);
        ArrayPool<char>.Shared.Return(_buffer);
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
            GrowBuffer(span.Length);
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
    public void Append<TSpanFormattable, TFormatProvider>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default, TFormatProvider? formatProvider = default)
        where TSpanFormattable : ISpanFormattable where TFormatProvider : IFormatProvider
    {
        while (true)
        {
            if (!spanFormattable.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
            {
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

    public readonly void CopyTo(char[] destination, int offset = 0)
    {
        CopyTo(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(destination), offset));
    }

    public readonly void CopyTo(Memory<char> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination.Span));
    }

    public readonly void CopyTo(Span<char> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination));
    }

    public readonly unsafe void CopyTo(ref char destination)
    {
        CopyTo((char*)Unsafe.AsPointer(ref destination));
    }

    public readonly unsafe void CopyTo(char* destination)
    {
        char* source = (char*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(_buffer));
        Unsafe.CopyBlock(destination, source, (uint)(Length * sizeof(char)));
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
