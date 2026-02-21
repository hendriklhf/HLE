using System;
using System.Buffers;
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
public sealed partial class PooledStringBuilder :
    IStringBuilder,
    IDisposable,
    ICollection<char>,
    IEquatable<PooledStringBuilder>,
    ICopyable<char>,
    IIndexable<char>,
    IReadOnlyCollection<char>,
    IMemoryProvider<char>,
    IReadOnlyMemoryProvider<char>
{
    public ref char this[int index] => ref WrittenSpan[index];

    char IIndexable<char>.this[int index] => WrittenSpan[index];

    char IIndexable<char>.this[Index index] => WrittenSpan[index];

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

    internal char[]? _buffer;

    public PooledStringBuilder() => _buffer = [];

    public PooledStringBuilder(int capacity) => _buffer = ArrayPool<char>.Shared.Rent(capacity);

    public PooledStringBuilder(ReadOnlySpan<char> str)
    {
        char[] buffer = ArrayPool<char>.Shared.Rent(str.Length);
        SpanHelpers.Copy(str, buffer);
        _buffer = buffer;
        Length = str.Length;
    }

    public void Dispose()
    {
        char[]? buffer = _buffer;
        if (buffer is null)
        {
            return;
        }

        _buffer = null;
        ArrayPool<char>.Shared.Return(buffer);
    }

    [Pure]
    public Span<char> AsSpan() => GetBuffer().AsSpanUnsafe(0, Length);

    [Pure]
    public Span<char> AsSpan(int start) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), Length, start);

    [Pure]
    public Span<char> AsSpan(int start, int length) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), Length, start, length);

    [Pure]
    public Span<char> AsSpan(Range range) => Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(GetBuffer()), Length, range);

    [Pure]
    public Memory<char> AsMemory() => GetBuffer().AsMemory(0, Length);

    [Pure]
    public Memory<char> AsMemory(int start) => AsMemory()[start..];

    [Pure]
    public Memory<char> AsMemory(int start, int length) => AsMemory().Slice(start, length);

    [Pure]
    public Memory<char> AsMemory(Range range) => AsMemory()[range];

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.AsMemory() => AsMemory();

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.AsMemory(int start) => AsMemory(start..);

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.AsMemory(int start, int length) => AsMemory(start, length);

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.AsMemory(Range range) => AsMemory(range);

    public void EnsureCapacity(int capacity) => GetDestination(capacity - Capacity);

    public void Advance(int length) => Length += length;

    void IStringBuilder.Append(ref DefaultInterpolatedStringHandler chars)
    {
        Append(chars.Text);
        chars.Clear();
    }

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
                Length += span.Length;
                return;
            }

            switch (chars)
            {
                case ICollection<char> collection:
                    Debug.Assert(collection.Count == elementCount);
                    char[] buffer = GetBuffer();
                    Debug.Assert(buffer.Length >= Length + collection.Count);
                    collection.CopyTo(buffer, Length);
                    Length += collection.Count;
                    return;
                case ICopyable<char> copyable:
                    Debug.Assert(copyable.Count == elementCount);
                    copyable.CopyTo(ref destination);
                    Length += copyable.Count;
                    return;
                case IIndexable<char> indexable:
                    for (int i = 0; i < indexable.Count; i++)
                    {
                        destination = indexable[i];
                        destination = ref Unsafe.Add(ref destination, 1);
                    }

                    Length += indexable.Count;
                    return;
            }

            foreach (char c in chars)
            {
                destination = c;
                destination = ref Unsafe.Add(ref destination, 1);
            }

            Length += elementCount;
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

    public void Append(ReadOnlySpan<char> chars) => Append(ref MemoryMarshal.GetReference(chars), chars.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Append(ref char chars, int length)
    {
        ref char destination = ref GetDestination(length);
        SpanHelpers.Memmove(ref destination, ref chars, length);
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
        while (!TryFormat(value, FreeBufferSpan, out charsWritten, format))
        {
            Grow(128);

            if (++countOfFailedTries >= MaximumFormattingTries)
            {
                ThrowMaximumFormatTriesExceeded(countOfFailedTries);
            }
        }

        Advance(charsWritten);

        return;

        [DoesNotReturn]
        static void ThrowMaximumFormatTriesExceeded(int countOfFailedTries)
            => throw new InvalidOperationException($"Trying to format the {typeof(T)} failed {countOfFailedTries} times. The method aborted.");
    }

    private static bool TryFormat<T>(T value, Span<char> destination, out int charsWritten, string? format)
    {
        if (typeof(T).IsEnum)
        {
            return TryFormatEnum(null, value, destination, out charsWritten, format);
        }

#pragma warning disable IDE0038, RCS1220
        if (value is IInterpolatedStringHandler)
        {
            // constrained call to avoid boxing for value types
            return TryCopyTo(((IInterpolatedStringHandler)value).Text, destination, out charsWritten);
        }

        string? str;
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

    private static bool TryCopyTo(ReadOnlySpan<char> str, Span<char> destination, out int charsWritten)
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

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private ref char Grow(int neededSize)
    {
        Debug.Assert(neededSize > 0);

        char[] oldBuffer = GetBuffer();
        int newSize = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
        if (Length != 0)
        {
            ref char source = ref MemoryMarshal.GetArrayDataReference(oldBuffer);
            ref char destination = ref MemoryMarshal.GetArrayDataReference(newBuffer);
            SpanHelpers.Memmove(ref destination, ref source, Length);
        }

        ArrayPool<char>.Shared.Return(oldBuffer);
        _buffer = newBuffer;
        return ref ArrayMarshal.GetUnsafeElementAt(newBuffer, Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private ref char GetDestination(int sizeHint)
    {
        char[] buffer = GetBuffer();
        int length = Length;
        int freeBufferSize = buffer.Length - length;
        if (freeBufferSize >= sizeHint)
        {
            return ref ArrayMarshal.GetUnsafeElementAt(buffer, length);
        }

        return ref Grow(sizeHint - freeBufferSize);
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

    public void CopyTo(List<char> destination, int offset = 0)
        => SpanHelpers.CopyChecked(WrittenSpan, destination, offset);

    public void CopyTo(char[] destination, int offset = 0)
        => SpanHelpers.CopyChecked(WrittenSpan, destination.AsSpan(offset..));

    public void CopyTo(Memory<char> destination) => SpanHelpers.CopyChecked(WrittenSpan, destination.Span);

    public void CopyTo(Span<char> destination) => SpanHelpers.CopyChecked(WrittenSpan, destination);

    public void CopyTo(ref char destination) => SpanHelpers.Copy(WrittenSpan, ref destination);

    public unsafe void CopyTo(char* destination) => SpanHelpers.Copy(WrittenSpan, destination);

    void ICollection<char>.Add(char item) => Append(item);

    bool ICollection<char>.Contains(char item) => WrittenSpan.Contains(item);

    bool ICollection<char>.Remove(char item)
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

    public ArrayEnumerator<char> GetEnumerator() => new(GetBuffer(), 0, Length);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<char>.Enumerator : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public override string ToString() => Length == 0 ? string.Empty : new(WrittenSpan);

    [Pure]
    public string ToString(int start) => new(WrittenSpan[start..]);

    [Pure]
    public string ToString(int start, int length) => new(WrittenSpan.Slice(start, length));

    [Pure]
    public string ToString(Range range) => new(WrittenSpan[range]);

    [Pure]
    public bool Equals(PooledStringBuilder builder, StringComparison comparisonType) => Equals(builder.WrittenSpan, comparisonType);

    [Pure]
    public bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType) => WrittenSpan.Equals(str, comparisonType);

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledStringBuilder? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [Pure]
    public int GetHashCode(StringComparison comparisonType) => string.GetHashCode(WrittenSpan, comparisonType);

    public static bool operator ==(PooledStringBuilder? left, PooledStringBuilder? right) => Equals(left, right);

    public static bool operator !=(PooledStringBuilder? left, PooledStringBuilder? right) => !(left == right);
}
