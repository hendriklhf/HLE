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

namespace HLE.Strings;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{AsString()}\"")]
public unsafe struct NativeString :
    IReadOnlyList<char>,
    IDisposable,
    IEquatable<NativeString>,
    ICountable,
    IIndexAccessible<char>,
    ISpanProvider<char>,
    IMemoryProvider<char>
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

    public static int MaximumLength { get; } = (int.MaxValue - sizeof(nuint) * 2 - sizeof(int)) / 2;

    public NativeString()
    {
    }

    [MustDisposeResource]
    public NativeString(int length)
    {
        if (length == 0)
        {
            _buffer = [];
            Length = 0;
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (length > MaximumLength)
        {
            ThrowLengthExceedsMaximumLength(length);
        }

        nuint neededBufferSize = RawDataMarshal.GetRawStringSize(length);
        NativeMemory<byte> buffer = new((int)neededBufferSize, false);

        RawStringData* rawStringData = (RawStringData*)(buffer._memory + sizeof(nuint));
        rawStringData->MethodTablePointer = (nuint)typeof(string).TypeHandle.Value;
        rawStringData->Length = length;
        Unsafe.InitBlock(&rawStringData->FirstChar, 0, (uint)length * sizeof(char));

        Length = length;
        _buffer = buffer;
    }

    [MustDisposeResource]
    public NativeString(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            _buffer = [];
            Length = 0;
            return;
        }

        if (chars.Length > MaximumLength)
        {
            ThrowLengthExceedsMaximumLength(chars.Length);
        }

        nuint neededBufferSize = RawDataMarshal.GetRawStringSize(chars.Length);
        NativeMemory<byte> buffer = new((int)neededBufferSize, false);

        byte* bufferPointer = buffer._memory;
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
    private static void ThrowLengthExceedsMaximumLength(int length, [CallerArgumentExpression(nameof(length))] string? paramName = null)
        => throw new ArgumentOutOfRangeException(paramName, length, $"The provided length exceeds the maximum {nameof(NativeString)} length.");

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
    // ReSharper disable once NotDisposedResource
    public static string Alloc(int length) => length == 0 ? string.Empty : new NativeString(length).AsString();

    [Pure]
    // ReSharper disable once NotDisposedResource
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
