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

namespace HLE.Strings;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{AsString()}\"")]
public unsafe struct NativeString : IReadOnlyList<char>, IDisposable, IEquatable<NativeString>, ICountable, IIndexAccessible<char>,
    ISpanProvider<char>
{
    public readonly ref char this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.Add(ref CharsReference, index);
        }
    }

    readonly char IReadOnlyList<char>.this[int index] => this[index];

    readonly char IIndexAccessible<char>.this[int index] => this[index];

    public readonly ref char this[Index index] => ref this[index.GetOffset(Length)];

    public readonly Span<char> this[Range range] => AsSpan(range);

    public readonly ref char CharsReference => ref Unsafe.As<byte, char>(ref Unsafe.Add(ref _buffer.Reference, sizeof(nuint) * 2 + sizeof(int)));

    public int Length { get; }

    readonly int ICountable.Count => Length;

    readonly int IReadOnlyCollection<char>.Count => Length;

    private NativeMemory<byte> _buffer = [];

    public static NativeString Empty => new();

    public NativeString()
    {
    }

    public NativeString(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (length == 0)
        {
            _buffer = [];
            Length = 0;
            return;
        }

        nuint neededBufferSize = RawDataMarshal.GetRawStringSize(length);
        if (neededBufferSize > int.MaxValue)
        {
            ThrowNeededBufferSizeExceedsMaxInt32Value();
        }

        NativeMemory<byte> buffer = new((int)neededBufferSize, false);
        RawStringData* rawStringData = (RawStringData*)(buffer._pointer + sizeof(nuint));
        rawStringData->MethodTablePointer = (nuint)typeof(string).TypeHandle.Value;
        rawStringData->Length = length;
        Unsafe.InitBlock(&rawStringData->Chars, 0, (uint)(length * sizeof(char)));

        Length = length;
        _buffer = buffer;
    }

    public NativeString(ReadOnlySpan<char> chars) : this(chars.Length)
    {
        if (chars.Length == 0)
        {
            _buffer = [];
            Length = 0;
            return;
        }

        nuint neededBufferSize = RawDataMarshal.GetRawStringSize(chars.Length);
        if (neededBufferSize > int.MaxValue)
        {
            ThrowNeededBufferSizeExceedsMaxInt32Value();
        }

        NativeMemory<byte> buffer = new((int)neededBufferSize, false);
        byte* bufferPointer = buffer._pointer;
        *(nuint*)bufferPointer = 0;

        RawStringData* rawStringData = (RawStringData*)(bufferPointer + sizeof(nuint));
        rawStringData->MethodTablePointer = (nuint)typeof(string).TypeHandle.Value;
        rawStringData->Length = chars.Length;
        CopyWorker<char>.Copy(chars, &rawStringData->Chars);

        Length = chars.Length;
        _buffer = buffer;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNeededBufferSizeExceedsMaxInt32Value()
        => throw new InvalidOperationException($"The needed buffer size exceeds the maximum {typeof(int)} value.");

    public void Dispose() => _buffer.Dispose();

    [Pure]
    public readonly string AsString()
        => Length == 0 ? string.Empty : RawDataMarshal.ReadObject<string, byte>(ref Unsafe.Add(ref _buffer.Reference, sizeof(nuint)))!;

    [Pure]
    public readonly Span<char> AsSpan() => MemoryMarshal.CreateSpan(ref CharsReference, Length);

    [Pure]
    public readonly Span<char> AsSpan(int start) => new Slicer<char>(ref CharsReference, Length).SliceSpan(start);

    [Pure]
    public readonly Span<char> AsSpan(int start, int length) => new Slicer<char>(ref CharsReference, Length).SliceSpan(start, length);

    [Pure]
    public readonly Span<char> AsSpan(Range range) => new Slicer<char>(ref CharsReference, Length).SliceSpan(range);

    readonly Span<char> ISpanProvider<char>.GetSpan() => AsSpan();

    [Pure]
    public static string Alloc(int length) => length == 0 ? string.Empty : new NativeString(length).AsString();

    [Pure]
    public static string Alloc(ReadOnlySpan<char> chars) => chars.Length == 0 ? string.Empty : new NativeString(chars).AsString();

    public static void Free(string? str)
    {
        if (str is not { Length: not 0 })
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

    public readonly NativeMemoryEnumerator<char> GetEnumerator() => new((char*)Unsafe.AsPointer(ref CharsReference), Length);

    readonly IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly bool Equals(NativeString other) => AsString() == other.AsString();

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is NativeString other && Equals(other);

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => AsString().GetHashCode();

    public readonly int GetHashCode(StringComparison comparison) => AsString().GetHashCode(comparison);

    public static bool operator ==(NativeString left, NativeString right) => left.Equals(right);

    public static bool operator !=(NativeString left, NativeString right) => !(left == right);
}
