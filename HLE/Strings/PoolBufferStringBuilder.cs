using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Strings;

[DebuggerDisplay("{ToString()}")]
public partial struct PoolBufferStringBuilder : IDisposable, IEquatable<PoolBufferStringBuilder>, ICopyable<char>
{
    public readonly ref char this[int index] => ref BufferSpan[index];

    public readonly ref char this[Index index] => ref BufferSpan[index];

    public readonly Span<char> this[Range range] => BufferSpan[range];

    public readonly int Length => _length;

    public readonly int Capacity => _buffer.Length;

    public readonly Span<char> BufferSpan => _buffer;

    public readonly Memory<char> BufferMemory => _buffer;

    public readonly ReadOnlySpan<char> WrittenSpan => BufferSpan[.._length];

    public readonly ReadOnlyMemory<char> WrittenMemory => BufferMemory[.._length];

    public readonly Span<char> FreeBufferSpan => BufferSpan[_length..];

    public readonly Memory<char> FreeBufferMemory => BufferMemory[_length..];

    public readonly int FreeBufferSize => _buffer.Length - _length;

    private char[] _buffer = Array.Empty<char>();
    private int _length;

    private const int _minimumGrowth = 100;

    public PoolBufferStringBuilder(int initialBufferSize = 100)
    {
        _buffer = ArrayPool<char>.Shared.Rent(initialBufferSize);
    }

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

    public readonly void Dispose()
    {
        ArrayPool<char>.Shared.Return(_buffer);
    }

    public void Advance(int length)
    {
        _length += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> span)
    {
        if (FreeBufferSize < span.Length)
        {
            GrowBuffer(span.Length);
        }

        span.CopyTo(FreeBufferSpan);
        _length += span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        if (FreeBufferSize <= 0)
        {
            GrowBuffer();
        }

        _buffer[_length++] = c;
    }

    public void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!value.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(value, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append<TSpanFormattable, TFormatProvider>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default, TFormatProvider? formatProvider = default) where TSpanFormattable : ISpanFormattable where TFormatProvider : IFormatProvider
    {
        if (!spanFormattable.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(spanFormattable, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!dateTime.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(dateTime, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] ReadOnlySpan<char> format = default, IFormatProvider? formatProvider = null)
    {
        if (!timeSpan.TryFormat(FreeBufferSpan, out int charsWritten, format, formatProvider))
        {
            GrowBuffer();
            Append(timeSpan, format, formatProvider);
        }

        Advance(charsWritten);
    }

    public void Remove(int index, int length = 1)
    {
        BufferSpan[(index + length).._length].CopyTo(BufferSpan[index..]);
        _length -= length;
    }

#if M
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
        return new(WrittenSpan);
    }

    [Pure]
    public readonly char[] ToCharArray()
    {
        return WrittenSpan.ToArray();
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
        Unsafe.CopyBlock(destination, source, (uint)(_length * sizeof(char)));
    }

    [Pure]
    public readonly bool Equals(PoolBufferStringBuilder builder, StringComparison comparisonType)
    {
        return WrittenSpan.Equals(builder.WrittenSpan, comparisonType);
    }

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType)
    {
        return WrittenSpan.Equals(str, comparisonType);
    }

    public readonly bool Equals(PoolBufferStringBuilder other)
    {
        return ReferenceEquals(_buffer, other._buffer) && _length == other._length;
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
        return HashCode.Combine(_buffer, _length);
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
