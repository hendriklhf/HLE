using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Text;

[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("\"{ToString()}\"")]
public unsafe ref partial struct ValueStringBuilder :
#if NET9_0_OR_GREATER
    IEquatable<ValueStringBuilder>,
#endif
    IStringBuilder,
    IDisposable,
    ICollection<char>,
    ICopyable<char>,
    IIndexable<char>,
    IReadOnlyCollection<char>,
    IReadOnlySpanProvider<char>
{
    public readonly ref char this[int index] => ref WrittenSpan[index];

    readonly char IIndexable<char>.this[int index] => this[index];

    readonly char IIndexable<char>.this[Index index] => this[index];

    public readonly ref char this[Index index] => ref WrittenSpan[index];

    public readonly Span<char> this[Range range] => WrittenSpan[range];

    public int Length { get; private set; }

    readonly int IReadOnlyCollection<char>.Count => Length;

    readonly int ICountable.Count => Length;

    readonly int ICollection<char>.Count => Length;

    public int Capacity { get; private set; }

    public readonly Span<char> WrittenSpan => MemoryMarshal.CreateSpan(ref GetBufferReference(), Length);

    public readonly Span<char> FreeBufferSpan => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref GetBufferReference(), Length), FreeBufferSize);

    public readonly int FreeBufferSize => Capacity - Length;

    readonly bool ICollection<char>.IsReadOnly => false;

    private ref char _buffer;
    private Flags _flags;

    public ValueStringBuilder() => _buffer = ref Unsafe.NullRef<char>();

    public ValueStringBuilder(int capacity)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(capacity);
        _buffer = ref MemoryMarshal.GetArrayDataReference(buffer);
        Capacity = buffer.Length;
        _flags = Flags.IsRentedArray;
    }

    public ValueStringBuilder(Span<char> buffer) : this(ref MemoryMarshal.GetReference(buffer), buffer.Length)
    {
    }

    public ValueStringBuilder(char* buffer, int length) : this(ref Unsafe.AsRef<char>(buffer), length)
    {
    }

    public ValueStringBuilder(ref char buffer, int length)
    {
        _buffer = ref buffer;
        Capacity = length;
    }

    public void Dispose()
    {
        Flags flags = _flags;
        if ((flags & Flags.IsDisposed) != 0)
        {
            return;
        }

        _flags = Flags.IsDisposed;

        ref char buffer = ref _buffer;
        _buffer = ref Unsafe.NullRef<char>();

        if ((flags & Flags.IsRentedArray) == 0)
        {
            return;
        }

        char[] array = SpanMarshal.AsArray(ref buffer);
        ArrayPool<char>.Shared.Return(array);
    }

    [Pure]
    public readonly Span<char> AsSpan() => GetBuffer().SliceUnsafe(0, Length);

    [Pure]
    public readonly Span<char> AsSpan(int start) => Slicer.Slice(ref GetBufferReference(), Length, start);

    [Pure]
    public readonly Span<char> AsSpan(int start, int length) => Slicer.Slice(ref GetBufferReference(), Length, start, length);

    [Pure]
    public readonly Span<char> AsSpan(Range range) => Slicer.Slice(ref GetBufferReference(), Length, range);

    readonly ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan() => AsSpan();

    readonly ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan(int start) => AsSpan(start..);

    readonly ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan(int start, int length) => AsSpan(start, length);

    readonly ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan(Range range) => AsSpan(range);

    public void Advance(int length) => Length += length;

    void IStringBuilder.Append(ref PooledInterpolatedStringHandler chars)
    {
        Append(chars.Text);
        chars.Dispose();
    }

    public void Append([InterpolatedStringHandlerArgument("")] ref InterpolatedStringHandler chars)
        => this = chars.Builder;

    public void Append(IEnumerable<char> chars)
    {
        if (chars.TryGetNonEnumeratedCount(out int elementCount))
        {
            if (elementCount == 0)
            {
                return;
            }

            ref char destination = ref GetDestination(elementCount);

            if (chars.TryGetReadOnlySpan(out ReadOnlySpan<char> span))
            {
                SpanHelpers.Memmove(ref destination, ref MemoryMarshal.GetReference(span), span.Length);
                Advance(span.Length);
                return;
            }

            switch (chars)
            {
                case ICollection<char> collection when (_flags & Flags.IsRentedArray) != 0:
                    char[] buffer = SpanMarshal.AsArray(GetBuffer());
                    Debug.Assert(buffer.GetType() == typeof(char[]));
                    Debug.Assert(buffer.Length >= Length + collection.Count);
                    collection.CopyTo(buffer, Length);
                    Advance(collection.Count);
                    return;
                case ICopyable<char> copyable:
                    copyable.CopyTo(ref destination);
                    Advance(copyable.Count);
                    return;
                case IIndexable<char> indexable:
                    for (int i = 0; i < indexable.Count; i++)
                    {
                        destination = indexable[i];
                        destination = ref Unsafe.Add(ref destination, 1);
                    }

                    Advance(indexable.Count);
                    return;
            }

            foreach (char c in chars)
            {
                destination = c;
                destination = ref Unsafe.Add(ref destination, 1);
            }

            Advance(elementCount);
            return;
        }

        foreach (char c in chars)
        {
            Append(c);
        }
    }

    public void Append(List<char> chars) => Append(ref ListMarshal.GetReference(chars), chars.Count);

    public void Append(char[] chars) => Append(ref MemoryMarshal.GetArrayDataReference(chars), chars.Length);

    public void Append(string str) => Append(ref StringMarshal.GetReference(str), str.Length);

    public void Append(scoped ReadOnlySpan<char> chars) => Append(ref MemoryMarshal.GetReference(chars), chars.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Append(scoped ref char chars, int length)
    {
        SpanHelpers.Memmove(ref GetDestination(length), ref chars, length);
        Length += length;
    }

    public void Append(char c)
    {
        GetDestination(1) = c;
        Length++;
    }

    public void Append(char c, int count)
    {
        if (count == 0)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(count);

        MemoryMarshal.CreateSpan(ref GetDestination(count), count).Fill(c);
        Length += count;
    }

    public void Append(IEnumerable<string> strings)
    {
        if (strings.TryGetReadOnlySpan(out ReadOnlySpan<string> span))
        {
            Append(span);
            return;
        }

#if NET10_0_OR_GREATER
        InlineArray8<string> buffer = default;
        using ValueList<string> list = new(buffer);
#else
        using ValueList<string> list = [];
#endif
        list.AddRange(strings);
        Append(list.AsSpan());
    }

    public void Append(List<string> strings) => Append(ref ListMarshal.GetReference(strings), strings.Count);

    public void Append(string[] strings) => Append(ref MemoryMarshal.GetArrayDataReference(strings), strings.Length);

    public void Append(scoped Span<string> strings) => Append(ref MemoryMarshal.GetReference(strings), strings.Length);

    public void Append(params scoped ReadOnlySpan<string> strings) => Append(ref MemoryMarshal.GetReference(strings), strings.Length);

    private void Append(ref string strings, int length)
    {
        int sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += Unsafe.Add(ref strings, i).Length;
        }

        ref char destination = ref GetDestination(sum);
        for (int i = 0; i < length; i++)
        {
            string s = Unsafe.Add(ref strings, i);
            SpanHelpers.Memmove(ref destination, ref StringMarshal.GetReference(s), s.Length);
            destination = ref Unsafe.Add(ref destination, s.Length);
        }

        Length += sum;
    }

    public void Append(byte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<byte>(value, format);

    public void Append(sbyte value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<sbyte>(value, format);

    public void Append(short value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<short>(value, format);

    public void Append(ushort value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<ushort>(value, format);

    public void Append(int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<int>(value, format);

    public void Append(uint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<uint>(value, format);

    public void Append(long value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<long>(value, format);

    public void Append(ulong value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<ulong>(value, format);

    public void Append(nint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<nint>(value, format);

    public void Append(nuint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<nuint>(value, format);

    public void Append(Int128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<Int128>(value, format);

    public void Append(UInt128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<UInt128>(value, format);

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<float>(value, format);

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<double>(value, format);

    public void Append(decimal value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<decimal>(value, format);

    public void Append(DateTime dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null)
        => Append<DateTime>(dateTime, format);

    public void Append(DateTimeOffset dateTime, [StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string? format = null)
        => Append<DateTimeOffset>(dateTime, format);

    public void Append(TimeSpan timeSpan, [StringSyntax(StringSyntaxAttribute.TimeSpanFormat)] string? format = null)
        => Append<TimeSpan>(timeSpan, format);

    public void Append(DateOnly dateOnly, [StringSyntax(StringSyntaxAttribute.DateOnlyFormat)] string? format = null)
        => Append<DateOnly>(dateOnly, format);

    public void Append(TimeOnly timeOnly, [StringSyntax(StringSyntaxAttribute.TimeOnlyFormat)] string? format = null)
        => Append<TimeOnly>(timeOnly, format);

    public void Append(Guid guid, [StringSyntax(StringSyntaxAttribute.GuidFormat)] string? format = null)
        => Append<Guid>(guid, format);

    public void Append<T>(T value, string? format = null)
    {
        const int MaximumFormattingTries = 5;

        int countOfFailedTries = 0;
        int charsWritten;
        ref char destination = ref GetDestination(1);
        int length = Length;
        int capacity = Capacity;
        while (!TryFormat(value, MemoryMarshal.CreateSpan(ref destination, capacity - length), out charsWritten, format))
        {
            destination = ref GrowAndGetDestination(128);
            capacity = Capacity;

            if (++countOfFailedTries >= MaximumFormattingTries)
            {
                ThrowMaximumFormatTriesExceeded(countOfFailedTries);
            }
        }

        Length += charsWritten;

        return;

        [DoesNotReturn]
        static void ThrowMaximumFormatTriesExceeded(int countOfFailedTries)
            => throw new InvalidOperationException($"Trying to format the {typeof(T)} failed {countOfFailedTries} times. The method aborted.");
    }

    public static bool TryFormat<T>(T value, Span<char> destination, out int charsWritten, string? format)
    {
        if (typeof(T).IsEnum)
        {
            return TryFormatEnum(null, value, destination, out charsWritten, format);
        }

        string? str;
#pragma warning disable IDE0038, RCS1220
        if (value is IFormattable)
        {
            if (value is ISpanFormattable)
#pragma warning restore IDE0038, RCS1220
            {
                // constrained call to avoid boxing for value types
                return ((ISpanFormattable)value).TryFormat(destination, out charsWritten, format, null);
            }

            // constrained call to avoid boxing for value types
            str = ((IFormattable)value).ToString(format, null);
            return TryCopyTo(str, destination, out charsWritten);
        }

        str = value?.ToString() ?? string.Empty;
        return TryCopyTo(str, destination, out charsWritten);
    }

    private static bool TryCopyTo(string str, Span<char> destination, out int charsWritten)
    {
        if (str.TryCopyTo(destination))
        {
            charsWritten = str.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "TryFormatUnconstrained")]
    private static extern bool TryFormatEnum<TEnum>(
        Enum? c,
        TEnum value,
        Span<char> destination,
        out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.EnumFormat)]
        ReadOnlySpan<char> format = default
    );

    public void Clear() => Length = 0;

    public void EnsureCapacity(int capacity) => GetDestination(capacity - Capacity);

    void ICollection<char>.Add(char item) => Append(item);

    readonly bool ICollection<char>.Remove(char item)
    {
        Span<char> writtenSpan = WrittenSpan;
        int index = writtenSpan.IndexOf(item);
        if (index == -1)
        {
            return false;
        }

        ref char src = ref Unsafe.Add(ref MemoryMarshal.GetReference(writtenSpan), index + 1);
        ref char dst = ref Unsafe.Add(ref src, -1);
        SpanHelpers.Memmove(ref dst, ref src, writtenSpan.Length - index - 1);
        return true;
    }

    readonly bool ICollection<char>.Contains(char item) => WrittenSpan.Contains(item);

    public readonly void CopyTo(List<char> destination, int offset = 0)
        => SpanHelpers.CopyChecked(WrittenSpan, destination, offset);

    public readonly void CopyTo(char[] destination, int offset = 0)
        => SpanHelpers.CopyChecked(WrittenSpan, destination.AsSpan(offset..));

    public readonly void CopyTo(Memory<char> destination) => SpanHelpers.CopyChecked(WrittenSpan, destination.Span);

    public readonly void CopyTo(Span<char> destination) => SpanHelpers.CopyChecked(WrittenSpan, destination);

    public readonly void CopyTo(ref char destination) => SpanHelpers.Copy(WrittenSpan, ref destination);

    public readonly void CopyTo(char* destination) => SpanHelpers.Copy(WrittenSpan, destination);

    private ref char GetDestination(int sizeHint)
    {
        Debug.Assert(sizeHint >= 0);

        if ((_flags & Flags.IsDisposed) != 0)
        {
#if NET9_0_OR_GREATER
            ThrowHelper.ThrowObjectDisposedException<ValueStringBuilder>();
#else
            ThrowHelper.ThrowObjectDisposedException(typeof(ValueStringBuilder));
#endif
        }

        int length = Length;
        int freeBufferSize = Capacity - length;
        if (freeBufferSize >= sizeHint)
        {
            return ref Unsafe.Add(ref _buffer, length);
        }

        return ref GrowAndGetDestination(sizeHint - freeBufferSize);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private ref char GrowAndGetDestination(int neededSize)
    {
        Debug.Assert(neededSize >= 0);

        Span<char> oldBuffer = GetBuffer();
        int newSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        Span<char> newBuffer = ArrayPool<char>.Shared.Rent(newSize);
        int length = Length;
        if (length != 0)
        {
            SpanHelpers.Copy(oldBuffer.SliceUnsafe(..length), newBuffer);
        }

        _buffer = ref MemoryMarshal.GetReference(newBuffer);
        Capacity = newBuffer.Length;

        Flags flags = _flags;
        _flags |= Flags.IsRentedArray;

        if ((flags & Flags.IsRentedArray) != 0)
        {
            char[] array = SpanMarshal.AsArray(oldBuffer);
            ArrayPool<char>.Shared.Return(array);
        }

        return ref Unsafe.Add(ref _buffer, length);
    }

    private readonly ref char GetBufferReference()
    {
        if ((_flags & Flags.IsDisposed) != 0)
        {
#if NET9_0_OR_GREATER
            ThrowHelper.ThrowObjectDisposedException<ValueStringBuilder>();
#else
            ThrowHelper.ThrowObjectDisposedException(typeof(ValueStringBuilder));
#endif
        }

        return ref _buffer;
    }

    internal readonly Span<char> GetBuffer() => MemoryMarshal.CreateSpan(ref GetBufferReference(), Capacity);

    public readonly MemoryEnumerator<char> GetEnumerator() => new(WrittenSpan);

    readonly IEnumerator<char> IEnumerable<char>.GetEnumerator() => throw new NotSupportedException();

    readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    [Pure]
    public override readonly string ToString() => Length == 0 ? string.Empty : new(WrittenSpan);

    [Pure]
    public readonly string ToString(int start) => new(WrittenSpan[start..]);

    [Pure]
    public readonly string ToString(int start, int length) => new(WrittenSpan.Slice(start, length));

    [Pure]
    public readonly string ToString(Range range) => new(WrittenSpan[range]);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public readonly bool Equals(scoped ValueStringBuilder other)
        => Length == other.Length && Capacity == other.Capacity &&
           _flags == other._flags && GetBuffer() == other.GetBuffer();

    [Pure]
    public readonly bool Equals(scoped ValueStringBuilder other, StringComparison comparisonType)
        => Equals(other.WrittenSpan, comparisonType);

    [Pure]
    public readonly bool Equals(scoped ReadOnlySpan<char> str, StringComparison comparisonType)
        => WrittenSpan.Equals(str, comparisonType);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine(Length, Capacity, _flags);

    [Pure]
    public readonly int GetHashCode(StringComparison comparisonType) => string.GetHashCode(WrittenSpan, comparisonType);

    public static bool operator ==(ValueStringBuilder left, ValueStringBuilder right) => left.Equals(right);

    public static bool operator !=(ValueStringBuilder left, ValueStringBuilder right) => !(left == right);
}
