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

[DebuggerDisplay("\"{ToString()}\"")]
public unsafe ref partial struct ValueStringBuilder :
    IStringBuilder,
    IDisposable,
    ICollection<char>,
    IEquatable<ValueStringBuilder>,
    ICopyable<char>,
    IIndexable<char>,
    IReadOnlyCollection<char>
{
    public readonly ref char this[int index] => ref WrittenSpan[index];

    readonly char IIndexable<char>.this[int index] => this[index];

    readonly char IIndexable<char>.this[Index index] => this[index];

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

    readonly int IReadOnlyCollection<char>.Count => Length;

    readonly int ICountable.Count => Length;

    readonly int ICollection<char>.Count => Length;

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

    readonly bool ICollection<char>.IsReadOnly => false;

    private ref char _buffer;
    private IntBoolUnion<int> _bufferLengthAndIsDisposed;
    private IntBoolUnion<int> _lengthAndIsStackalloced;

    public ValueStringBuilder()
    {
        _buffer = ref Unsafe.NullRef<char>();
        IsStackalloced = true;
    }

    public ValueStringBuilder(int capacity)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(capacity);
        _buffer = ref MemoryMarshal.GetArrayDataReference(buffer);
        BufferLength = buffer.Length;
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
        BufferLength = length;
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

    void IStringBuilder.Append(ref PooledInterpolatedStringHandler chars)
    {
        Append(chars.Text);
        chars.Dispose();
    }

    public void Append([InterpolatedStringHandlerArgument("")] InterpolatedStringHandler chars) => this = chars.Builder;

    public void Append(List<char> chars) => Append(ref ListMarshal.GetReference(chars), chars.Count);

    public void Append(char[] chars) => Append(ref MemoryMarshal.GetArrayDataReference(chars), chars.Length);

    public void Append(string str) => Append(ref StringMarshal.GetReference(str), str.Length);

    public void Append(scoped ReadOnlySpan<char> chars) => Append(ref MemoryMarshal.GetReference(chars), chars.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Append(scoped ref char chars, int length)
    {
        ref char destination = ref GetDestination(length);
        SpanHelpers.Memmove(ref destination, ref chars, (uint)length);
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

    public void Append(List<string> strings) => Append(ref ListMarshal.GetReference(strings), strings.Count);

    public void Append(string[] strings) => Append(ref MemoryMarshal.GetArrayDataReference(strings), strings.Length);

    public void Append(Span<string> strings) => Append(ref MemoryMarshal.GetReference(strings), strings.Length);

    public void Append(ReadOnlySpan<string> strings) => Append(ref MemoryMarshal.GetReference(strings), strings.Length);

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
            SpanHelpers.Memmove(ref destination, ref StringMarshal.GetReference(s), (uint)s.Length);
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
        while (!TryFormat(value, MemoryMarshal.CreateSpan(ref destination, Capacity - length), out charsWritten, format))
        {
            destination = ref GrowAndGetDestination(128);

            if (++countOfFailedTries >= MaximumFormattingTries)
            {
                ThrowMaximumFormatTriesExceeded(countOfFailedTries);
            }
        }

        Length = length + charsWritten;

        return;

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
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

    readonly Span<char> ISpanProvider<char>.GetSpan() => WrittenSpan;

    readonly ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => WrittenSpan;

    void ICollection<char>.Add(char item) => Append(item);

    readonly bool ICollection<char>.Remove(char item) => throw new NotSupportedException();

    readonly bool ICollection<char>.Contains(char item) => WrittenSpan.Contains(item);

    public readonly void CopyTo(List<char> destination, int offset = 0)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(char[] destination, int offset = 0)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(Memory<char> destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(Span<char> destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(ref char destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(ref destination);
    }

    public readonly void CopyTo(char* destination)
    {
        CopyWorker<char> copyWorker = new(WrittenSpan);
        copyWorker.CopyTo(destination);
    }

    private ref char GetDestination(int sizeHint)
    {
        Debug.Assert(sizeHint >= 0);

        if (IsDisposed)
        {
            ThrowHelper.ThrowObjectDisposedException<ValueStringBuilder>();
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
        BufferLength = newBuffer.Length;

        if (IsStackalloced)
        {
            IsStackalloced = false;
            return ref Unsafe.Add(ref _buffer, length);
        }

        char[] array = SpanMarshal.AsArray(oldBuffer);
        ArrayPool<char>.Shared.Return(array);
        return ref Unsafe.Add(ref _buffer, length);
    }

    private readonly ref char GetBufferReference()
    {
        if (IsDisposed)
        {
            ThrowHelper.ThrowObjectDisposedException<ValueStringBuilder>();
        }

        return ref _buffer;
    }

    internal readonly Span<char> GetBuffer() => MemoryMarshal.CreateSpan(ref GetBufferReference(), BufferLength);

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
    public readonly bool Equals(scoped ValueStringBuilder other) => Length == other.Length && GetBuffer() == other.GetBuffer();

    [Pure]
    public readonly bool Equals(scoped ValueStringBuilder other, StringComparison comparisonType)
        => Equals(other.WrittenSpan, comparisonType);

    [Pure]
    public readonly bool Equals(scoped ReadOnlySpan<char> str, StringComparison comparisonType)
        => WrittenSpan.Equals(str, comparisonType);

    [Pure]
    public override readonly int GetHashCode() => GetHashCode(StringComparison.Ordinal);

    [Pure]
    public readonly int GetHashCode(StringComparison comparisonType) => string.GetHashCode(WrittenSpan, comparisonType);

    public static bool operator ==(ValueStringBuilder left, ValueStringBuilder right) => left.Equals(right);

    public static bool operator !=(ValueStringBuilder left, ValueStringBuilder right) => !(left == right);
}
