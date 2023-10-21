using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Strings;

[DebuggerDisplay("\"{ToString()}\"")]
public ref partial struct ValueStringBuilder
{
    public readonly ref char this[int index] => ref WrittenSpan[index];

    public readonly ref char this[Index index] => ref WrittenSpan[index];

    public readonly Span<char> this[Range range] => WrittenSpan[range];

    public int Length { get; private set; }

    public readonly int Capacity => _buffer.Length;

    public readonly Span<char> WrittenSpan => _buffer[..Length];

    public readonly Span<char> FreeBuffer => _buffer[Length..];

    public readonly int FreeBufferSize => Capacity - Length;

    public static ValueStringBuilder Empty => new();

    internal readonly Span<char> _buffer = Span<char>.Empty;

    public ValueStringBuilder()
    {
    }

    public ValueStringBuilder(Span<char> buffer) => _buffer = buffer;

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

        if (FreeBufferSize < span.Length)
        {
            ThrowNotEnoughSpaceException();
        }

        ref char destination = ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), Length);
        ref char source = ref MemoryMarshal.GetReference(span);
        CopyWorker<char>.Memmove(ref destination, ref source, (nuint)span.Length);
        Advance(span.Length);
    }

    public void Append(char c)
    {
        if (Length == Capacity)
        {
            ThrowNotEnoughSpaceException();
        }

        Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), Length++) = c;
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

    public void Append<TSpanFormattable>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default) where TSpanFormattable : ISpanFormattable
    {
        if (!spanFormattable.TryFormat(FreeBuffer, out int charsWritten, format, null))
        {
            ThrowNotEnoughSpaceException();
        }

        Advance(charsWritten);
    }

    internal bool TryAppend<TSpanFormattable>(TSpanFormattable spanFormattable, ReadOnlySpan<char> format = default) where TSpanFormattable : ISpanFormattable
    {
        bool success = spanFormattable.TryFormat(FreeBuffer, out int charsWritten, format, null);
        Advance(charsWritten);
        return success;
    }

    public void Clear() => Length = 0;

    public readonly MemoryEnumerator<char> GetEnumerator() => new(ref MemoryMarshal.GetReference(_buffer), Length);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString() => new(WrittenSpan);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj) => false;

    [Pure]
    public readonly bool Equals(ValueStringBuilder other)
    {
        ref char bufferReference = ref MemoryMarshal.GetReference(_buffer);
        ref char otherBufferReference = ref MemoryMarshal.GetReference(other._buffer);
        return Unsafe.AreSame(ref bufferReference, ref otherBufferReference) && Length == other.Length;
    }

    [Pure]
    public readonly bool Equals(ValueStringBuilder other, StringComparison comparisonType)
        => Equals(other.WrittenSpan, comparisonType);

    [Pure]
    public readonly bool Equals(ReadOnlySpan<char> str, StringComparison comparisonType)
        => ((ReadOnlySpan<char>)WrittenSpan).Equals(str, comparisonType);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => GetHashCode(StringComparison.Ordinal);

    [Pure]
    public readonly int GetHashCode(StringComparison comparisonType) => string.GetHashCode(WrittenSpan, comparisonType);

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNotEnoughSpaceException()
        => throw new InvalidOperationException("There is not enough space left in the buffer to write to.");

    public static bool operator ==(ValueStringBuilder left, ValueStringBuilder right) => left.Equals(right);

    public static bool operator !=(ValueStringBuilder left, ValueStringBuilder right) => !(left == right);
}
