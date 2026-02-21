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

namespace HLE.Memory;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay("\"{AsString()}\"")]
public readonly unsafe partial struct NativeString :
    IReadOnlyList<char>,
    IDisposable,
    IEquatable<NativeString>,
    IIndexable<char>,
    ISpanProvider<char>,
    IReadOnlySpanProvider<char>,
    IMemoryProvider<char>,
    IReadOnlyMemoryProvider<char>
{
    public ref char this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return ref Unsafe.Add(ref GetCharsReference(), index);
        }
    }

    char IReadOnlyList<char>.this[int index] => this[index];

    char IIndexable<char>.this[int index] => this[index];

    char IIndexable<char>.this[Index index] => this[index];

    public ref char this[Index index] => ref this[index.GetOffset(Length)];

    public Span<char> this[Range range] => AsSpan(range);

    public int Length { get; }

    int ICountable.Count => Length;

    int IReadOnlyCollection<char>.Count => Length;

    private readonly NativeMemory<byte> _memory;

    public static NativeString Empty => new();

    public NativeString() => _memory = NativeMemory<byte>.Empty;

    public NativeString(int length)
    {
        if (length == 0)
        {
            _memory = NativeMemory<byte>.Empty;
            Length = 0;
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        nuint neededBufferSize = ObjectMarshal.GetRawStringSize(length);
        NativeMemory<byte> memory = NativeMemory<byte>.Alloc(int.CreateChecked(neededBufferSize));
        byte* buffer = memory.Pointer;

        *(nuint*)buffer = 0;
        RawStringData* rawStringData = (RawStringData*)(buffer + sizeof(nuint));
        rawStringData->MethodTable = ObjectMarshal.GetMethodTable<string>();
        rawStringData->Length = length;
        SpanHelpers.Clear(&rawStringData->FirstChar, length);

        Length = length;
        _memory = memory;
    }

    public NativeString(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            _memory = NativeMemory<byte>.Empty;
            Length = 0;
            return;
        }

        nuint neededBufferSize = ObjectMarshal.GetRawStringSize(chars.Length);
        NativeMemory<byte> memory = NativeMemory<byte>.Alloc(int.CreateChecked(neededBufferSize), false);
        byte* buffer = memory.Pointer;

        *(nuint*)buffer = 0;
        RawStringData* rawStringData = (RawStringData*)(buffer + sizeof(nuint));
        rawStringData->MethodTable = ObjectMarshal.GetMethodTable<string>();
        rawStringData->Length = chars.Length;
        SpanHelpers.Copy(chars, &rawStringData->FirstChar);

        Length = chars.Length;
        _memory = memory;
    }

    public void Dispose() => _memory.Dispose();

    [Pure]
    public RawStringData* AsRawStringData() => (RawStringData*)(_memory.Pointer + sizeof(nuint));

    [Pure]
    public string AsString() => Length == 0 ? string.Empty : ObjectMarshal.ReadObject<string>(AsRawStringData());

    [Pure]
    public Span<char> AsSpan() => MemoryMarshal.CreateSpan(ref GetCharsReference(), Length);

    [Pure]
    public Span<char> AsSpan(int start) => Slicer.Slice(ref GetCharsReference(), Length, start);

    [Pure]
    public Span<char> AsSpan(int start, int length) => Slicer.Slice(ref GetCharsReference(), Length, start, length);

    [Pure]
    public Span<char> AsSpan(Range range) => Slicer.Slice(ref GetCharsReference(), Length, range);

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan() => AsSpan();

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan(int start) => AsSpan(start..);

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan(int start, int length) => AsSpan(start, length);

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.AsSpan(Range range) => AsSpan(range);

    [Pure]
    public Memory<char> AsMemory() => new MemoryManager(this).Memory;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref char GetCharsReference() => ref Unsafe.AsRef<char>(&AsRawStringData()->FirstChar);

    [Pure]
    public override string ToString() => new(AsSpan());

    public NativeMemoryEnumerator<char> GetEnumerator() => new((char*)Unsafe.AsPointer(ref GetCharsReference()), Length);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<char>.Enumerator : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(NativeString other) => Length == other.Length && _memory.Equals(other._memory);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is NativeString other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Length, _memory);

    public int GetHashCode(StringComparison comparison) => AsString().GetHashCode(comparison);

    public static bool operator ==(NativeString left, NativeString right) => left.Equals(right);

    public static bool operator !=(NativeString left, NativeString right) => !(left == right);
}
