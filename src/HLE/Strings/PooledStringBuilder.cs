using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
[method: MustDisposeResource]
public sealed partial class PooledStringBuilder(int capacity) :
    IDisposable,
    ICollection<char>,
    IEquatable<PooledStringBuilder>,
    ICopyable<char>,
    IIndexAccessible<char>,
    IReadOnlyCollection<char>,
    ISpanProvider<char>,
    IMemoryProvider<char>
{
    public ref char this[int index] => ref WrittenSpan[index];

    char IIndexAccessible<char>.this[int index] => WrittenSpan[index];

    public ref char this[Index index] => ref WrittenSpan[index];

    public Span<char> this[Range range] => WrittenSpan[range];

    public int Length { get; private set; }

    int ICollection<char>.Count => Length;

    int ICountable.Count => Length;

    int IReadOnlyCollection<char>.Count => Length;

    public int Capacity => GetBuffer().Length;

    public Span<char> WrittenSpan => GetBuffer().AsSpanUnsafe(..Length);

    public Memory<char> WrittenMemory => GetBuffer().AsMemory(..Length);

    public Span<char> FreeBufferSpan => GetBuffer().AsSpanUnsafe(Length..);

    public Memory<char> FreeBufferMemory => GetBuffer().AsMemory(Length..);

    public int FreeBufferSize => Capacity - Length;

    bool ICollection<char>.IsReadOnly => false;

    internal char[]? _buffer = capacity == 0 ? [] : ArrayPool<char>.Shared.Rent(capacity);

    [MustDisposeResource]
    public PooledStringBuilder() : this(0)
    {
    }

    [MustDisposeResource]
    public PooledStringBuilder(ReadOnlySpan<char> str) : this(str.Length)
    {
        CopyWorker<char>.Copy(str, _buffer);
        Length = str.Length;
    }

    public void Dispose()
    {
        char[]? buffer = _buffer;
        if (buffer is null)
        {
            return;
        }

        ArrayPool<char>.Shared.Return(buffer);
        _buffer = null;
    }

    Span<char> ISpanProvider<char>.GetSpan() => WrittenSpan;

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => WrittenSpan;

    Memory<char> IMemoryProvider<char>.GetMemory() => WrittenMemory;

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => WrittenMemory;

    public void EnsureCapacity(int capacity) => GrowIfNeeded(capacity - Capacity);

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

        GrowIfNeeded(chars.Length);

        Debug.Assert(_buffer is not null, "If _buffer is null, some exception should have been thrown before");

        ref char destination = ref Unsafe.Add(ref GetBufferReference(), Length);
        ref char source = ref MemoryMarshal.GetReference(chars);
        CopyWorker<char>.Copy(ref source, ref destination, (uint)chars.Length);
        Length += chars.Length;
    }

    public void Append(char c)
    {
        GrowIfNeeded(1);
        Unsafe.Add(ref GetBufferReference(), Length++) = c;
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
                break;
            }

            if (formattable.TryFormat(FreeBufferSpan, out int writtenChars, format, null))
            {
                Advance(writtenChars);
                return;
            }

            Grow(128);
            countOfFailedTries++;
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaximumFormatTriesExceeded<TSpanFormattable>(int countOfFailedTries) where TSpanFormattable : ISpanFormattable
        => throw new InvalidOperationException(
            $"Trying to format the {typeof(TSpanFormattable)} failed {countOfFailedTries} times. The method aborted.");

    public void Replace(char oldChar, char newChar) => WrittenSpan.Replace(oldChar, newChar);

    public void Clear() => Length = 0;

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private void Grow(int neededSize)
    {
        Debug.Assert(neededSize > 0);

        char[] oldBuffer = GetBuffer();
        int newSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
        if (Length != 0)
        {
            ref char source = ref MemoryMarshal.GetArrayDataReference(oldBuffer);
            ref char destination = ref MemoryMarshal.GetArrayDataReference(newBuffer);
            CopyWorker<char>.Copy(ref source, ref destination, (uint)Length);
        }

        ArrayPool<char>.Shared.Return(oldBuffer);
        _buffer = newBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private void GrowIfNeeded(int sizeHint)
    {
        int freeBufferSize = FreeBufferSize;
        if (freeBufferSize >= sizeHint)
        {
            return;
        }

        Grow(sizeHint - freeBufferSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private char[] GetBuffer()
    {
        char[]? buffer = _buffer;
        if (buffer is null)
        {
            ThrowHelper.ThrowObjectDisposedException<PooledStringBuilder>();
        }

        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref char GetBufferReference() => ref MemoryMarshal.GetArrayDataReference(GetBuffer());

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

    [SuppressMessage("Roslynator", "RCS1168:Parameter name differs from base name")]
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

    void ICollection<char>.Add(char item) => Append(item);

    bool ICollection<char>.Contains(char item) => WrittenSpan.Contains(item);

    bool ICollection<char>.Remove(char item) => throw new NotSupportedException();

    public ArrayEnumerator<char> GetEnumerator() => new(GetBuffer(), 0, Length);

    IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(PooledStringBuilder builder, StringComparison comparisonType) => Equals(builder.WrittenSpan, comparisonType);

    [Pure]
    public bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType) => ((ReadOnlySpan<char>)WrittenSpan).Equals(str, comparisonType);

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledStringBuilder? other) => Length == other?.Length && GetBuffer().Equals(other.GetBuffer());

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is PooledStringBuilder other && Equals(other);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [Pure]
    public int GetHashCode(StringComparison comparisonType) => string.GetHashCode(WrittenSpan, comparisonType);

    public static bool operator ==(PooledStringBuilder? left, PooledStringBuilder? right) => Equals(left, right);

    public static bool operator !=(PooledStringBuilder? left, PooledStringBuilder? right) => !(left == right);
}
