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
public sealed unsafe class NativeString :
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

    private byte* _buffer;

    public static NativeString Empty => new();

    public NativeString()
    {
    }

    [MustDisposeResource]
    public NativeString(int length)
    {
        if (length == 0)
        {
            _buffer = null;
            Length = 0;
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        nuint neededBufferSize = ObjectMarshal.GetRawStringSize(length);
        byte* buffer = (byte*)NativeMemory.AlignedAlloc(neededBufferSize, (nuint)sizeof(nuint));

        *(nuint*)buffer = 0;
        RawStringData* rawStringData = (RawStringData*)(buffer + sizeof(nuint));
        rawStringData->MethodTable = ObjectMarshal.GetMethodTable<string>();
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
            _buffer = null;
            Length = 0;
            return;
        }

        nuint neededBufferSize = ObjectMarshal.GetRawStringSize(chars.Length);
        byte* buffer = (byte*)NativeMemory.AlignedAlloc(neededBufferSize, (nuint)sizeof(nuint));

        *(nuint*)buffer = 0;
        RawStringData* rawStringData = (RawStringData*)(buffer + sizeof(nuint));
        rawStringData->MethodTable = ObjectMarshal.GetMethodTable<string>();
        rawStringData->Length = chars.Length;
        SpanHelpers.Copy(chars, &rawStringData->FirstChar);

        Length = chars.Length;
        _buffer = buffer;
    }

    ~NativeString() => TryDispose();

    public void Dispose()
    {
        if (!TryDispose())
        {
            return;
        }

        GC.SuppressFinalize(this);
    }

    private bool TryDispose()
    {
        byte* buffer = _buffer;
        if (buffer == null)
        {
            return false;
        }

        NativeMemory.AlignedFree(buffer);
        _buffer = null;
        return true;
    }

    [Pure]
    public RawStringData* AsRawStringData() => (RawStringData*)(_buffer + sizeof(nuint));

    [Pure]
    public string AsString() => Length == 0 ? string.Empty : ObjectMarshal.ReadObject<string>(AsRawStringData());

    [Pure]
    public Span<char> AsSpan() => MemoryMarshal.CreateSpan(ref GetCharsReference(), Length);

    [Pure]
    public Span<char> AsSpan(int start) => new Slicer<char>(ref GetCharsReference(), Length).SliceSpan(start);

    [Pure]
    public Span<char> AsSpan(int start, int length) => new Slicer<char>(ref GetCharsReference(), Length).SliceSpan(start, length);

    [Pure]
    public Span<char> AsSpan(Range range) => new Slicer<char>(ref GetCharsReference(), Length).SliceSpan(range);

    [Pure]
    public Memory<char> AsMemory() => new NativeMemoryManager<char>(&AsRawStringData()->FirstChar, Length).Memory;

    Span<char> ISpanProvider<char>.GetSpan() => AsSpan();

    ReadOnlySpan<char> IReadOnlySpanProvider<char>.GetReadOnlySpan() => AsSpan();

    Memory<char> IMemoryProvider<char>.GetMemory() => AsMemory();

    ReadOnlyMemory<char> IReadOnlyMemoryProvider<char>.GetReadOnlyMemory() => AsMemory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref char GetCharsReference() => ref Unsafe.AsRef<char>(&AsRawStringData()->FirstChar);

    [Pure]
    public override string ToString() => new(AsSpan());

    public NativeMemoryEnumerator<char> GetEnumerator() => new((char*)Unsafe.AsPointer(ref GetCharsReference()), Length);

    // ReSharper disable once NotDisposedResourceIsReturned
    IEnumerator<char> IEnumerable<char>.GetEnumerator() => Length == 0 ? EmptyEnumeratorCache<char>.Enumerator : GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals([NotNullWhen(true)] NativeString? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public int GetHashCode(StringComparison comparison) => AsString().GetHashCode(comparison);

    public static bool operator ==(NativeString left, NativeString right) => left.Equals(right);

    public static bool operator !=(NativeString left, NativeString right) => !(left == right);
}
