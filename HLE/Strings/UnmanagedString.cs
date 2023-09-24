using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Strings;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{AsString()}\"")]
public unsafe struct UnmanagedString : IDisposable, IEquatable<UnmanagedString>, ICountable, IIndexAccessible<char>, ISpanProvider<char>
{
    public readonly ref char this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.Add(ref CharsReference, index);
        }
    }

    readonly char IIndexAccessible<char>.this[int index] => this[index];

    public readonly ref char this[Index index] => ref this[index.GetOffset(Length)];

    public readonly Span<char> this[Range range] => AsSpan(range);

    public readonly ref char CharsReference => ref Unsafe.As<byte, char>(ref Unsafe.Add(ref _buffer.Reference, sizeof(nuint) + sizeof(int)));

    public int Length { get; }

    readonly int ICountable.Count => Length;

    private RentedArray<byte> _buffer = RentedArray<byte>.Empty;

    public static UnmanagedString Empty => new();

    public UnmanagedString()
    {
    }

    private UnmanagedString(int length, RentedArray<byte> buffer)
    {
        _buffer = buffer;
        Length = length;
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }

    [Pure]
    public readonly string AsString()
    {
        return Length == 0 ? string.Empty : RawDataMarshal.GetObjectFromRawData<string>(ref Unsafe.Add(ref _buffer.Reference, sizeof(nuint)));
    }

    [Pure]
    public readonly Span<char> AsSpan() => MemoryMarshal.CreateSpan(ref CharsReference, Length);

    [Pure]
    public readonly Span<char> AsSpan(int start) => new Slicer<char>(ref CharsReference, Length).CreateSpan(start);

    [Pure]
    public readonly Span<char> AsSpan(int start, int length) => new Slicer<char>(ref CharsReference, Length).CreateSpan(start, length);

    [Pure]
    public readonly Span<char> AsSpan(Range range) => new Slicer<char>(ref CharsReference, Length).CreateSpan(range);

    readonly Span<char> ISpanProvider<char>.GetSpan() => AsSpan();

    [Pure]
    public static UnmanagedString Create(int length)
    {
        RentedArray<byte> buffer = ArrayPool<byte>.Shared.CreateRentedArray(GetNeededByteCount(length));
        WriteMetadata(length, ref buffer.Reference);
        buffer.AsSpan(sizeof(nuint) + sizeof(int), length * sizeof(char)).Clear();
        return new(length, buffer);
    }

    [Pure]
    public static UnmanagedString Create(ReadOnlySpan<char> chars)
    {
        int length = chars.Length;
        RentedArray<byte> buffer = ArrayPool<byte>.Shared.CreateRentedArray(GetNeededByteCount(length));
        WriteMetadata(length, ref buffer.Reference);
        Span<char> charBuffer = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, char>(ref Unsafe.Add(ref buffer.Reference, sizeof(nuint) + sizeof(int))), length + 1);
        chars.CopyToUnsafe(charBuffer);
        charBuffer[^1] = '\0';
        return new(length, buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteMetadata(int length, ref byte bufferReference)
    {
        ref nuint methodTableReference = ref Unsafe.As<byte, nuint>(ref bufferReference);
        methodTableReference = RawDataMarshal.GetMethodTablePointer(string.Empty);
        ref int lengthReference = ref Unsafe.As<nuint, int>(ref Unsafe.Add(ref methodTableReference, 1));
        lengthReference = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetNeededByteCount(int stringLength)
    {
        return sizeof(nuint) /* method table pointer */ +
               sizeof(int) /* string length */ +
               stringLength * sizeof(char) /* chars */ +
               sizeof(char) /* zero-char */;
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString() => new(AsSpan());

    public readonly bool Equals(UnmanagedString other)
    {
        return AsString() == other.AsString();
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj is UnmanagedString other && Equals(other);
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode()
    {
        return AsString().GetHashCode();
    }

    public static bool operator ==(UnmanagedString left, UnmanagedString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UnmanagedString left, UnmanagedString right)
    {
        return !(left == right);
    }
}
