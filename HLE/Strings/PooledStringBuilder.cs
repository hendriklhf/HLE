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
public sealed partial class PooledStringBuilder : IDisposable, ICollection<char>, IEquatable<PooledStringBuilder>, ICopyable<char>, ICountable, IIndexAccessible<char>, IReadOnlyCollection<char>
{
    public ref char this[int index] => ref Unsafe.AsRef(in WrittenSpan[index]);

    char IIndexAccessible<char>.this[int index] => WrittenSpan[index];

    public ref char this[Index index] => ref WrittenSpan.AsMutableSpan()[index];

    public Span<char> this[Range range] => WrittenSpan.AsMutableSpan()[range];

    public int Length { get; private set; }

    int ICollection<char>.Count => Length;

    int ICountable.Count => Length;

    int IReadOnlyCollection<char>.Count => Length;

    public int Capacity => _buffer.Length;

    public ReadOnlySpan<char> WrittenSpan => _buffer.Span[..Length];

    public ReadOnlyMemory<char> WrittenMemory => _buffer.Memory[..Length];

    public Span<char> FreeBufferSpan => _buffer.Span[Length..];

    public Memory<char> FreeBufferMemory => _buffer.Memory[Length..];

    public int FreeBufferSize => Capacity - Length;

    bool ICollection<char>.IsReadOnly => false;

    internal RentedArray<char> _buffer;

    internal const int DefaultBufferSize = 32;

    public static PooledStringBuilder Empty => new(RentedArray<char>.Empty);

    private PooledStringBuilder(RentedArray<char> buffer)
    {
        _buffer = buffer;
    }

    public PooledStringBuilder() : this(DefaultBufferSize)
    {
    }

    public PooledStringBuilder(int initialBufferSize)
    {
        _buffer = new(initialBufferSize);
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowBuffer(int sizeHint = 0)
    {
        int newSize = sizeHint < 1 ? _buffer.Length << 1 : _buffer.Length + sizeHint;
        RentedArray<char> newBuffer = new(newSize);
        Debug.Assert(newBuffer.Length > _buffer.Length);
        _buffer.Span.CopyToUnsafe(newBuffer.Span);
        _buffer.Dispose();
        _buffer = newBuffer;
    }

    public void Advance(int length)
    {
        Length += length;
    }

    public void Append(scoped ReadOnlySpan<char> span)
    {
        if (FreeBufferSize < span.Length)
        {
            GrowBuffer(span.Length);
        }

        ValueStringBuilder builder = new(FreeBufferSpan);
        builder.Append(span);
        Advance(builder.Length);
    }

    public void Append(char c)
    {
        if (FreeBufferSize <= 0)
        {
            GrowBuffer();
        }

        _buffer[Length++] = c;
    }

    public void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<byte>(value, format);
    }

    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<sbyte>(value, format);
    }

    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<short>(value, format);
    }

    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<ushort>(value, format);
    }

    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<int>(value, format);
    }

    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<uint>(value, format);
    }

    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<long>(value, format);
    }

    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<ulong>(value, format);
    }

    public void Append(Int128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<Int128>(value, format);
    }

    public void Append(UInt128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<UInt128>(value, format);
    }

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<float>(value, format);
    }

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
    {
        Append<double>(value, format);
    }

    public void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default)
    {
        Append<DateTime>(dateTime, format);
    }

    public void Append(DateTimeOffset dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default)
    {
        Append<DateTimeOffset>(dateTime, format);
    }

    public void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] ReadOnlySpan<char> format = default)
    {
        Append<TimeSpan>(timeSpan, format);
    }

    public void Append(DateOnly dateOnly, [StringSyntax(StringSyntaxAttribute.DateOnlyFormat)] ReadOnlySpan<char> format = default)
    {
        Append<DateOnly>(dateOnly, format);
    }

    public void Append(TimeOnly timeOnly, [StringSyntax(StringSyntaxAttribute.TimeOnlyFormat)] ReadOnlySpan<char> format = default)
    {
        Append<TimeOnly>(timeOnly, format);
    }

    public void Append(Guid guid, [StringSyntax(StringSyntaxAttribute.GuidFormat)] ReadOnlySpan<char> format = default)
    {
        Append<Guid>(guid, format);
    }

    public void Append<TSpanFormattable>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default) where TSpanFormattable : ISpanFormattable
    {
        const int maximumFormattingTries = 10;
        int countOfFailedTries = 0;
        while (true)
        {
            if (countOfFailedTries == maximumFormattingTries)
            {
                throw new InvalidOperationException($"Trying to format the {typeof(TSpanFormattable)} failed {countOfFailedTries} times. The method aborted.");
            }

            ValueStringBuilder builder = new(FreeBufferSpan);
            if (!builder.TryAppend(spanFormattable, format))
            {
                countOfFailedTries++;
                GrowBuffer();
                continue;
            }

            Advance(builder.Length);
            break;
        }
    }

    void ICollection<char>.Clear() => Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear(bool clearBuffer = false)
    {
        Length = 0;
        if (clearBuffer)
        {
            _buffer.Span.Clear();
        }
    }

    [Pure]
    public override string ToString()
    {
        return new(WrittenSpan);
    }

    public void CopyTo(List<char> destination, int offset = 0)
    {
        DefaultCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(char[] destination, int offset = 0)
    {
        DefaultCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<char> destination)
    {
        DefaultCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<char> destination)
    {
        DefaultCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    public void CopyTo(ref char destination)
    {
        DefaultCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(char* destination)
    {
        DefaultCopier<char> copier = new(WrittenSpan);
        copier.CopyTo(destination);
    }

    void ICollection<char>.Add(char c)
    {
        Append(c);
    }

    bool ICollection<char>.Contains(char c)
    {
        return WrittenSpan.Contains(c);
    }

    bool ICollection<char>.Remove(char c)
    {
        throw new NotSupportedException();
    }

    [Pure]
    public IEnumerator<char> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return WrittenSpan[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    public bool Equals(PooledStringBuilder builder, StringComparison comparisonType)
    {
        return Equals(builder.WrittenSpan, comparisonType);
    }

    [Pure]
    public bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType)
    {
        return WrittenSpan.Equals(str, comparisonType);
    }

    public bool Equals(PooledStringBuilder? other)
    {
        return Length == other?.Length && _buffer.Equals(other._buffer);
    }

    public override bool Equals(object? obj)
    {
        return obj is PooledStringBuilder other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    [Pure]
    public int GetHashCode(StringComparison comparisonType)
    {
        return string.GetHashCode(WrittenSpan, comparisonType);
    }

    public static bool operator ==(PooledStringBuilder? left, PooledStringBuilder? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PooledStringBuilder? left, PooledStringBuilder? right)
    {
        return !(left == right);
    }
}
