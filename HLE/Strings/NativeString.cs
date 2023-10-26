using System;
using System.Collections;
using System.Collections.Generic;
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
public unsafe struct NativeString : IReadOnlyCollection<char>, IDisposable, IEquatable<NativeString>, ICountable, IIndexAccessible<char>, ISpanProvider<char>
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

    public readonly ref char CharsReference => ref Unsafe.As<byte, char>(ref Unsafe.Add(ref _buffer.Reference, sizeof(nuint) * 2 + sizeof(int)));

    public int Length { get; }

    readonly int ICountable.Count => Length;

    readonly int IReadOnlyCollection<char>.Count => Length;

    private NativeMemory<byte> _buffer = NativeMemory<byte>.Empty;

    public static NativeString Empty => new();

    public NativeString()
    {
    }

    public NativeString(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (length == 0)
        {
            _buffer = NativeMemory<byte>.Empty;
            Length = 0;
            return;
        }

        int neededBufferSize = StringRawDataWriter.GetNeededBufferSize(length);
        NativeMemory<byte> buffer = new(neededBufferSize);
        StringRawDataWriter writer = new(ref buffer.Reference);
        writer.Write(length);

        Length = length;
        _buffer = buffer;
    }

    public NativeString(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            _buffer = NativeMemory<byte>.Empty;
            Length = 0;
            return;
        }

        int length = chars.Length;
        int neededBufferSize = StringRawDataWriter.GetNeededBufferSize(length);
        NativeMemory<byte> buffer = new(neededBufferSize, false);
        StringRawDataWriter writer = new(ref buffer.Reference);
        writer.Write(chars);

        Length = length;
        _buffer = buffer;
    }

    public void Dispose() => _buffer.Dispose();

    [Pure]
    public readonly string AsString()
        => Length == 0 ? string.Empty : RawDataMarshal.ReadObject<string>(ref Unsafe.Add(ref _buffer.Reference, sizeof(nuint)));

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
    public static string Alloc(int length) => length == 0 ? string.Empty : new NativeString(length).AsString();

    [Pure]
    public static string Alloc(ReadOnlySpan<char> chars) => chars.Length == 0 ? string.Empty : new NativeString(chars).AsString();

    public static void Free(string? str)
    {
        if (str is not { Length: > 0 })
        {
            return;
        }

        ref nuint methodTableReference = ref RawDataMarshal.GetMethodTableReference(str);
        methodTableReference = ref Unsafe.Subtract(ref methodTableReference, 1);
        void* ptr = Unsafe.AsPointer(ref methodTableReference);
        Debug.Assert((nuint)ptr % (nuint)sizeof(nuint) == 0); // is aligned
        NativeMemory.AlignedFree(ptr);
    }

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString() => new(AsSpan());

    public readonly CharEnumerator GetEnumerator() => AsString().GetEnumerator();

    readonly IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly bool Equals(NativeString other) => AsString() == other.AsString();

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj) => obj is NativeString other && Equals(other);

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => AsString().GetHashCode();

    public static bool operator ==(NativeString left, NativeString right) => left.Equals(right);

    public static bool operator !=(NativeString left, NativeString right) => !(left == right);
}
