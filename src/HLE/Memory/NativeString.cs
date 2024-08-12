using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Memory;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("\"{AsString()}\"")]
public readonly unsafe struct NativeString :
    IReadOnlyList<char>,
    IDisposable,
    IEquatable<NativeString>,
    ICountable,
    IIndexable<char>,
    ISpanProvider<char>,
    IMemoryProvider<char>
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

    public ref char this[Index index] => ref this[index.GetOffset(Length)];

    public Span<char> this[Range range] => AsSpan(range);

    public int Length { get; }

    int ICountable.Count => Length;

    int IReadOnlyCollection<char>.Count => Length;

    private readonly NativeMemory<byte> _memory;

    public static NativeString Empty { get; } = new();

    public NativeString() => _memory = NativeMemory<byte>.Empty;

    [MustDisposeResource]
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
        NativeMemory<byte> memory = new(int.CreateChecked(neededBufferSize));
        byte* buffer = memory.Pointer;

        *(nuint*)buffer = 0;
        RawStringData* rawStringData = (RawStringData*)(buffer + sizeof(nuint));
        rawStringData->MethodTable = ObjectMarshal.GetMethodTable<string>();
        rawStringData->Length = length;
        Unsafe.InitBlock(&rawStringData->FirstChar, 0, (uint)length * sizeof(char));

        Length = length;
        _memory = memory;
    }

    [MustDisposeResource]
    public NativeString(ReadOnlySpan<char> chars)
    {
        if (chars.Length == 0)
        {
            _memory = NativeMemory<byte>.Empty;
            Length = 0;
            return;
        }

        nuint neededBufferSize = ObjectMarshal.GetRawStringSize(chars.Length);
        NativeMemory<byte> memory = new(int.CreateChecked(neededBufferSize));
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
    public RawStringData* AsRawStringData()
    {
        if (Length == 0)
        {
            ThrowCantGetRawDataOnEmptyString();
        }

        return (RawStringData*)(_memory.Pointer + sizeof(nuint));
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCantGetRawDataOnEmptyString() => throw new InvalidOperationException("The string is empty, therefore getting the raw data is not possible.");

    [Pure]
    public string AsString() => Length == 0 ? string.Empty : ObjectMarshal.ReadObject<string>(AsRawStringData());

    [Pure]
    public Span<char> AsSpan() => Length == 0 ? [] : MemoryMarshal.CreateSpan(ref GetCharsReference(), Length);

    [Pure]
    public Span<char> AsSpan(int start) => new Slicer<char>(AsSpan()).SliceSpan(start);

    [Pure]
    public Span<char> AsSpan(int start, int length) => new Slicer<char>(AsSpan()).SliceSpan(start, length);

    [Pure]
    public Span<char> AsSpan(Range range) => new Slicer<char>(AsSpan()).SliceSpan(range);

    [Pure]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "not needed")]
    public Memory<char> AsMemory() => Length == 0 ? Memory<char>.Empty : new NativeMemoryManager<char>(&AsRawStringData()->FirstChar, Length).Memory;

    Span<char> ISpanProvider<char>.GetSpan() => AsSpan();

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => AsSpan();

    Memory<char> IMemoryProvider<char>.GetMemory() => AsMemory();

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => AsMemory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref char GetCharsReference()
    {
        Debug.Assert(Length != 0);
        return ref Unsafe.AsRef<char>(&AsRawStringData()->FirstChar);
    }

    [Pure]
    public override string ToString() => Length == 0 ? string.Empty : new(AsSpan());

    public NativeMemoryEnumerator<char> GetEnumerator() => Length == 0 ? new(null, 0) : new((char*)Unsafe.AsPointer(ref GetCharsReference()), Length);

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
