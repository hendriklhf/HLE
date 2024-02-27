using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
public ref partial struct ValueStringBuilder
{
    public readonly ref char this[int index] => ref WrittenSpan[index];

    public readonly ref char this[Index index] => ref WrittenSpan[index];

    public readonly Span<char> this[Range range] => WrittenSpan[range];

    [SuppressMessage("ReSharper", "StructMemberCanBeMadeReadOnly", Justification = "setter is mutating the instance, so Length is not readonly")]
    public int Length
    {
        readonly get => _lengthAndIsStackalloced.Integer;
        private set
        {
            Debug.Assert(value >= 0);
            _lengthAndIsStackalloced.SetIntegerUnsafe(value);
        }
    }

    private bool IsStackalloced
    {
        readonly get => _lengthAndIsStackalloced.Bool;
        set => _lengthAndIsStackalloced.Bool = value;
    }

    private int BufferLength
    {
        readonly get => _bufferLengthAndIsDisposed.Integer;
        set
        {
            Debug.Assert(value >= 0);
            _bufferLengthAndIsDisposed.SetIntegerUnsafe(value);
        }
    }

    private bool IsDisposed
    {
        readonly get => _bufferLengthAndIsDisposed.Bool;
        set => _bufferLengthAndIsDisposed.Bool = value;
    }

    public readonly int Capacity => BufferLength;

    public readonly Span<char> WrittenSpan => MemoryMarshal.CreateSpan(ref GetBufferReference(), Length);

    public readonly Span<char> FreeBufferSpan => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref GetBufferReference(), Length), FreeBufferSize);

    public readonly int FreeBufferSize => Capacity - Length;

    private ref char _buffer;
    private IntBoolUnion<int> _bufferLengthAndIsDisposed;
    private IntBoolUnion<int> _lengthAndIsStackalloced;

    [MustDisposeResource]
    public ValueStringBuilder()
    {
        _buffer = ref Unsafe.NullRef<char>();
        IsStackalloced = true;
    }

    [MustDisposeResource]
    public ValueStringBuilder(int capacity)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(capacity);
        _buffer = ref MemoryMarshal.GetArrayDataReference(buffer);
        BufferLength = buffer.Length;
    }

    [MustDisposeResource]
    public ValueStringBuilder(Span<char> buffer)
    {
        _buffer = ref MemoryMarshal.GetReference(buffer);
        BufferLength = buffer.Length;
        IsStackalloced = true;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        if (IsStackalloced)
        {
            _buffer = ref Unsafe.NullRef<char>();
            BufferLength = 0;
            IsDisposed = true;
            return;
        }

        char[] array = SpanMarshal.AsArray(ref _buffer);
        ArrayPool<char>.Shared.Return(array);

        _buffer = ref Unsafe.NullRef<char>();
        BufferLength = 0;
        IsDisposed = true;
    }

    public void Advance(int length) => Length += length;

    public void Append(scoped ReadOnlySpan<char> span)
    {
        switch (span.Length)
        {
            case 0:
                return;
            case 1:
                Append(span[0]);
                return;
        }

        GrowIfNeeded(span.Length);

        ref char destination = ref Unsafe.Add(ref GetBufferReference(), Length);
        ref char source = ref MemoryMarshal.GetReference(span);
        SpanHelpers<char>.Memmove(ref destination, ref source, (uint)span.Length);
        Length += span.Length;
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

    public void Append(nint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<nint>(value, format);

    public void Append(nuint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<nuint>(value, format);

    public void Append(Int128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<Int128>(value, format);

    public void Append(UInt128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format = default)
        => Append<UInt128>(value, format);

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
        do
        {
            if (formattable.TryFormat(FreeBufferSpan, out int writtenChars, format, null))
            {
                Advance(writtenChars);
                return;
            }

            if (++countOfFailedTries == MaximumFormattingTries)
            {
                ThrowMaximumFormatTriesExceeded<TSpanFormattable>(countOfFailedTries);
                break;
            }

            Grow(256);
        }
        while (true);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaximumFormatTriesExceeded<TSpanFormattable>(int countOfFailedTries) where TSpanFormattable : ISpanFormattable
        => throw new InvalidOperationException(
            $"Trying to format the {typeof(TSpanFormattable)} failed {countOfFailedTries} times. The method aborted.");

    public readonly void Replace(char oldChar, char newChar) => WrittenSpan.Replace(oldChar, newChar);

    public void Clear() => Length = 0;

    public void EnsureCapacity(int capacity) => GrowIfNeeded(capacity - Capacity);

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

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private void Grow(int neededSize)
    {
        Debug.Assert(neededSize >= 0);

        int length = Length;
        Span<char> oldBuffer = GetBuffer();

        int newSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        Span<char> newBuffer = ArrayPool<char>.Shared.Rent(newSize);
        if (length != 0)
        {
            SpanHelpers<char>.Copy(oldBuffer.SliceUnsafe(..length), newBuffer);
        }

        _buffer = ref MemoryMarshal.GetReference(newBuffer);
        BufferLength = newBuffer.Length;

        if (IsStackalloced)
        {
            IsStackalloced = false;
            return;
        }

        char[] array = SpanMarshal.AsArray(oldBuffer);
        ArrayPool<char>.Shared.Return(array);
    }

    private readonly ref char GetBufferReference()
    {
        if (IsDisposed)
        {
            ThrowHelper.ThrowObjectDisposedException(typeof(ValueStringBuilder));
        }

        return ref _buffer;
    }

    internal readonly Span<char> GetBuffer() => MemoryMarshal.CreateSpan(ref GetBufferReference(), BufferLength);

    public readonly MemoryEnumerator<char> GetEnumerator() => new(ref GetBufferReference(), Length);

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
    public readonly bool Equals(ValueStringBuilder other) => Length == other.Length && GetBuffer() == other.GetBuffer();

    [Pure]
    public readonly bool Equals(ValueStringBuilder other, StringComparison comparisonType)
        => Equals(other.WrittenSpan, comparisonType);

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType)
        => ((ReadOnlySpan<char>)WrittenSpan).Equals(str, comparisonType);

    [Pure]
    public override readonly int GetHashCode() => GetHashCode(StringComparison.Ordinal);

    [Pure]
    public readonly int GetHashCode(StringComparison comparisonType) => string.GetHashCode(WrittenSpan, comparisonType);

    public static bool operator ==(ValueStringBuilder left, ValueStringBuilder right) => left.Equals(right);

    public static bool operator !=(ValueStringBuilder left, ValueStringBuilder right) => !(left == right);
}
