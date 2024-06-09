using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Text;

[DebuggerDisplay("\"{ToString()}\"")]
[method: MustDisposeResource]
public sealed partial class PooledStringBuilder(int capacity) :
    IDisposable,
    ICollection<char>,
    IEquatable<PooledStringBuilder>,
    ICopyable<char>,
    IIndexable<char>,
    IReadOnlyCollection<char>,
    ISpanProvider<char>,
    IMemoryProvider<char>
{
    public ref char this[int index] => ref WrittenSpan[index];

    char IIndexable<char>.this[int index] => WrittenSpan[index];

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
        Debug.Assert(_buffer is not null);
        SpanHelpers<char>.Copy(str, _buffer);
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

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    [SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter")]
    public void Append([InterpolatedStringHandlerArgument("")] InterpolatedStringHandler chars)
    {
        // handler contains all the logic
    }

    public void Append(List<char> chars) => Append(ref ListMarshal.GetReference(chars), chars.Count);

    public void Append(char[] chars) => Append(ref MemoryMarshal.GetArrayDataReference(chars), chars.Length);

    public void Append(string str) => Append(StringMarshal.GetReference(str), str.Length);

    public void Append(ReadOnlySpan<char> chars) => Append(ref MemoryMarshal.GetReference(chars), chars.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Append(ref char chars, int length)
    {
        GrowIfNeeded(length);
        ref char destination = ref Unsafe.Add(ref GetBufferReference(), Length);
        SpanHelpers<char>.Memmove(ref destination, ref chars, (uint)length);
        Length += length;
    }

    public void Append(char c)
    {
        GrowIfNeeded(1);
        Unsafe.Add(ref GetBufferReference(), Length++) = c;
    }

    public void Append(char c, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        GrowIfNeeded(count);
        FreeBufferSpan.SliceUnsafe(0, count).Fill(c);
        Length += count;
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

    public void Append(Int128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<Int128>(value, format);

    public void Append(UInt128 value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<UInt128>(value, format);

    public void Append(nint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<nint>(value, format);

    public void Append(nuint value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<nuint>(value, format);

    public void Append(float value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<float>(value, format);

    public void Append(double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => Append<double>(value, format);

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
        while (!TryFormat(value, FreeBufferSpan, out charsWritten, format))
        {
            Grow(128);

            if (++countOfFailedTries >= MaximumFormattingTries)
            {
                ThrowMaximumFormatTriesExceeded<T>(countOfFailedTries);
            }
        }

        Advance(charsWritten);
    }

    public static bool TryFormat<T>(T value, Span<char> destination, out int charsWritten, string? format)
    {
        if (typeof(T).IsEnum)
        {
            return TryFormatEnum(null, value, destination, out charsWritten, format);
        }

        string? str;
        if (value is IFormattable)
        {
            if (value is ISpanFormattable)
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
        return true;
    }

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "TryFormatUnconstrained")]
    private static extern bool TryFormatEnum<TEnum>(Enum? c, TEnum value, Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.EnumFormat)] ReadOnlySpan<char> format = default);

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaximumFormatTriesExceeded<T>(int countOfFailedTries)
        => throw new InvalidOperationException($"Trying to format the {typeof(T)} failed {countOfFailedTries} times. The method aborted.");

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
            SpanHelpers<char>.Memmove(ref destination, ref source, (uint)Length);
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
    internal char[] GetBuffer()
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

    public void CopyTo(ref char destination) => SpanHelpers<char>.Copy(WrittenSpan, ref destination);

    public unsafe void CopyTo(char* destination) => SpanHelpers<char>.Copy(WrittenSpan, destination);

    void ICollection<char>.Add(char item) => Append(item);

    bool ICollection<char>.Contains(char item) => WrittenSpan.Contains(item);

    bool ICollection<char>.Remove(char item) => throw new NotSupportedException();

    public ArrayEnumerator<char> GetEnumerator() => new(GetBuffer(), 0, Length);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<char>.Enumerator : GetEnumerator();

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
