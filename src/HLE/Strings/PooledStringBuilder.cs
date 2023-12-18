using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
public sealed partial class PooledStringBuilder(int capacity)
    : IDisposable, ICollection<char>, IEquatable<PooledStringBuilder>, ICopyable<char>, IIndexAccessible<char>, IReadOnlyCollection<char>,
        ISpanProvider<char>, IMemoryProvider<char>
{
    public ref char this[int index] => ref WrittenSpan[index];

    char IIndexAccessible<char>.this[int index] => WrittenSpan[index];

    public ref char this[Index index] => ref WrittenSpan[index];

    public Span<char> this[Range range] => WrittenSpan[range];

    public int Length { get; private set; }

    int ICollection<char>.Count => Length;

    int ICountable.Count => Length;

    int IReadOnlyCollection<char>.Count => Length;

    public int Capacity => _buffer.Length;

    public Span<char> WrittenSpan => _buffer.AsSpan(..Length);

    public Memory<char> WrittenMemory => _buffer.AsMemory(..Length);

    public Span<char> FreeBufferSpan => _buffer.AsSpan(Length..);

    public Memory<char> FreeBufferMemory => _buffer.AsMemory(Length..);

    public int FreeBufferSize => Capacity - Length;

    bool ICollection<char>.IsReadOnly => false;

    [SuppressMessage("ReSharper", "NotDisposedResource", Justification = "is disposed in Disposed()")]
    internal RentedArray<char> _buffer = capacity == 0 ? [] : ArrayPool<char>.Shared.RentAsRentedArray(capacity);

    public PooledStringBuilder() : this(0)
    {
    }

    public PooledStringBuilder(ReadOnlySpan<char> str) : this(str.Length)
    {
        CopyWorker<char>.Copy(str, _buffer.AsSpan());
        Length = str.Length;
    }

    public void Dispose() => _buffer.Dispose();

    Span<char> ISpanProvider<char>.GetSpan() => WrittenSpan;

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => WrittenSpan;

    Memory<char> IMemoryProvider<char>.GetMemory() => WrittenMemory;

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => WrittenMemory;

    private void GrowBuffer(int sizeHint)
    {
        Debug.Assert(sizeHint > 0);

        using RentedArray<char> oldBuffer = _buffer;
        int newSize = BufferHelpers.GrowArray(oldBuffer.Length, sizeHint);
        RentedArray<char> newBuffer = ArrayPool<char>.Shared.RentAsRentedArray(newSize);
        if (Length != 0)
        {
            CopyWorker<char>.Copy(ref oldBuffer.Reference, ref newBuffer.Reference, (uint)Length);
        }

        _buffer = newBuffer;
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity <= Capacity)
        {
            return;
        }

        GrowBuffer(capacity - Capacity);
    }

    public void Advance(int length) => Length += length;

    public void Append(scoped ReadOnlySpan<char> chars)
    {
        switch (chars.Length)
        {
            case 0:
                return;
            case 1:
                Append(chars[0]);
                return;
        }

        int freeBufferSize = FreeBufferSize;
        if (freeBufferSize < chars.Length)
        {
            GrowBuffer(chars.Length - freeBufferSize);
        }

        ref char destination = ref Unsafe.Add(ref _buffer.Reference, Length);
        ref char source = ref MemoryMarshal.GetReference(chars);
        CopyWorker<char>.Copy(ref source, ref destination, (uint)chars.Length);
        Length += chars.Length;
    }

    public void Append(char c)
    {
        if (Length == Capacity)
        {
            GrowBuffer(1);
        }

        Unsafe.Add(ref _buffer.Reference, Length++) = c;
    }

    public void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<byte>(value, format);

    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<sbyte>(value, format);

    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<short>(value, format);

    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<ushort>(value, format);

    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<int>(value, format);

    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<uint>(value, format);

    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<long>(value, format);

    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<ulong>(value, format);

    public void Append(Int128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<Int128>(value, format);

    public void Append(UInt128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<UInt128>(value, format);

    public void Append(nint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<nint>(value, format);

    public void Append(nuint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<nuint>(value, format);

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<float>(value, format);

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<double>(value, format);

    public void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default)
        => Append<DateTime>(dateTime, format);

    public void Append(DateTimeOffset dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] ReadOnlySpan<char> format = default)
        => Append<DateTimeOffset>(dateTime, format);

    public void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] ReadOnlySpan<char> format = default)
        => Append<TimeSpan>(timeSpan, format);

    public void Append(DateOnly dateOnly, [StringSyntax(StringSyntaxAttribute.DateOnlyFormat)] ReadOnlySpan<char> format = default)
        => Append<DateOnly>(dateOnly, format);

    public void Append(TimeOnly timeOnly, [StringSyntax(StringSyntaxAttribute.TimeOnlyFormat)] ReadOnlySpan<char> format = default)
        => Append<TimeOnly>(timeOnly, format);

    public void Append(Guid guid, [StringSyntax(StringSyntaxAttribute.GuidFormat)] ReadOnlySpan<char> format = default)
        => Append<Guid>(guid, format);

    public void Append<TSpanFormattable>(TSpanFormattable formattable, ReadOnlySpan<char> format = default)
        where TSpanFormattable : ISpanFormattable
    {
        const int MaximumFormattingTries = 5;
        int countOfFailedTries = 0;
        while (true)
        {
            if (countOfFailedTries == MaximumFormattingTries)
            {
                ThrowMaximumFormatTriesExceeded<TSpanFormattable>(countOfFailedTries);
            }

            ValueStringBuilder builder = new(FreeBufferSpan);
            if (!builder.TryAppend(formattable, format))
            {
                countOfFailedTries++;
                GrowBuffer(128);
                continue;
            }

            Advance(builder.Length);
            break;
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaximumFormatTriesExceeded<TSpanFormattable>(int countOfFailedTries) where TSpanFormattable : ISpanFormattable
        => throw new InvalidOperationException(
            $"Trying to format the {typeof(TSpanFormattable)} failed {countOfFailedTries} times. The method aborted.");

    public void Replace(char oldChar, char newChar) => WrittenSpan.Replace(oldChar, newChar);

    public void Clear() => Length = 0;

    [Pure]
    public override string ToString() => Length == 0 ? string.Empty : new(WrittenSpan);

    [Pure]
    public string ToString(int start) => new(WrittenSpan[start..]);

    [Pure]
    public string ToString(int start, int length) => new(WrittenSpan.Slice(start, length));

    [Pure]
    public string ToString(Range range) => new(WrittenSpan[range]);

    public void CopyTo(List<char> destination, int offset = 0)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(char[] destination, int offset = 0)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<char> destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<char> destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref char destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(char* destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    void ICollection<char>.Add(char c) => Append(c);

    bool ICollection<char>.Contains(char c) => WrittenSpan.Contains(c);

    bool ICollection<char>.Remove(char c) => throw new NotSupportedException();

    public ArrayEnumerator<char> GetEnumerator() => new(_buffer.Array, 0, Length);

    IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(PooledStringBuilder builder, StringComparison comparisonType) => Equals(builder.WrittenSpan, comparisonType);

    [Pure]
    public bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType) => ((ReadOnlySpan<char>)WrittenSpan).Equals(str, comparisonType);

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledStringBuilder? other) => Length == other?.Length && _buffer.Equals(other._buffer);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is PooledStringBuilder other && Equals(other);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [Pure]
    public int GetHashCode(StringComparison comparisonType) => string.GetHashCode(WrittenSpan, comparisonType);

    public static bool operator ==(PooledStringBuilder? left, PooledStringBuilder? right) => Equals(left, right);

    public static bool operator !=(PooledStringBuilder? left, PooledStringBuilder? right) => !(left == right);
}
