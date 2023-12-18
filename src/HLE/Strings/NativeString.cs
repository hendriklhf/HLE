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
    ISpanProvider<char>, IMemoryProvider<char>
{
    public readonly ref char this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.Add(ref GetCharsReference(), index);
        }
    }

    readonly char IReadOnlyList<char>.this[int index] => this[index];

    readonly char IIndexAccessible<char>.this[int index] => this[index];

    public readonly ref char this[Index index] => ref this[index.GetOffset(Length)];

    public readonly Span<char> this[Range range] => AsSpan(range);

    public int Length { get; }

    readonly int ICountable.Count => Length;

    readonly int IReadOnlyCollection<char>.Count => Length;

    private NativeMemory<byte> _buffer = [];

    public static NativeString Empty => new();

    // object header + method table pointer + string length
    private static readonly int s_firstCharByteOffset = sizeof(nuint) + sizeof(nuint) + sizeof(int);

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
        Unsafe.InitBlock(&rawStringData->FirstChar, 0, (uint)length * sizeof(char));

        Length = length;
        _buffer = buffer;
    }

    public NativeString(ReadOnlySpan<char> chars)
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
        CopyWorker<char>.Copy(chars, &rawStringData->FirstChar);

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
    {
        if (Length == 0)
        {
            return string.Empty;
        }

        byte* buffer = _buffer.Pointer;
        byte* methodTablePointer = buffer + sizeof(nuint);
        return RawDataMarshal.ReadObject<string>(methodTablePointer)!;
    }

    [Pure]
    public readonly Span<char> AsSpan() => MemoryMarshal.CreateSpan(ref GetCharsReference(), Length);

    [Pure]
    public readonly Span<char> AsSpan(int start) => new Slicer<char>(ref GetCharsReference(), Length).SliceSpan(start);

    [Pure]
    public readonly Span<char> AsSpan(int start, int length) => new Slicer<char>(ref GetCharsReference(), Length).SliceSpan(start, length);

    [Pure]
    public readonly Span<char> AsSpan(Range range) => new Slicer<char>(ref GetCharsReference(), Length).SliceSpan(range);

    [Pure]
    public readonly Memory<char> AsMemory() => new NativeMemoryManager<char>((char*)Unsafe.AsPointer(ref GetCharsReference()), Length).Memory;

    readonly Span<char> ISpanProvider<char>.GetSpan() => AsSpan();

    readonly ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => AsSpan();

    readonly Memory<char> IMemoryProvider<char>.GetMemory() => AsMemory();

    readonly ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => AsMemory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref char GetCharsReference()
    {
        byte* buffer = _buffer.Pointer;
        char* firstChar = (char*)(buffer + s_firstCharByteOffset);
        return ref Unsafe.AsRef<char>(firstChar);
    }

    [Pure]
    public static string Alloc(int length) => length == 0 ? string.Empty : new NativeString(length).AsString();

    [Pure]
    public static string Alloc(ReadOnlySpan<char> chars) => chars.Length == 0 ? string.Empty : new NativeString(chars).AsString();

    public static void Free(string? str)
    {
        if (str is null or { Length: 0 })
        {
            return;
        }

        nuint* ptr = *(nuint**)&str;
        NativeMemory.AlignedFree(--ptr);
    }

    [Pure]
    public override readonly string ToString() => new(AsSpan());

    public readonly NativeMemoryEnumerator<char> GetEnumerator() => new((char*)Unsafe.AsPointer(ref GetCharsReference()), Length);

    readonly IEnumerator<char> IEnumerable<char>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly bool Equals(NativeString other) => AsString() == other.AsString();

    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is NativeString other && Equals(other);

    public override readonly int GetHashCode() => AsString().GetHashCode();

    public readonly int GetHashCode(StringComparison comparison) => AsString().GetHashCode(comparison);

    public static bool operator ==(NativeString left, NativeString right) => left.Equals(right);

    public static bool operator !=(NativeString left, NativeString right) => !(left == right);
}
