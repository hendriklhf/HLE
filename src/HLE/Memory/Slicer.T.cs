using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public readonly ref partial struct Slicer<T>
{
    private readonly ref T _buffer;
    private readonly int _length;

    public Slicer(List<T> list) : this(CollectionsMarshal.AsSpan(list))
    {
    }

    public Slicer(T[] array) : this(ref MemoryMarshal.GetArrayDataReference(array), array.Length)
    {
    }

    public Slicer(Span<T> span) : this(ref MemoryMarshal.GetReference(span), span.Length)
    {
    }

    public Slicer(ReadOnlySpan<T> span) : this(ref MemoryMarshal.GetReference(span), span.Length)
    {
    }

    public unsafe Slicer(T* buffer, int length) : this(ref Unsafe.AsRef<T>(buffer), length)
    {
    }

    public Slicer(ref T buffer, int length)
    {
        _buffer = ref buffer;
        _length = length;
    }

    [Pure]
    public Span<T> SliceSpan(Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(_length);
        return SliceSpan(start, length);
    }

    [Pure]
    public Span<T> SliceSpan(int start) => SliceSpan(start, _length - start);

    [Pure]
    public Span<T> SliceSpan(int start, int length)
    {
        ref T startReference = ref GetStart(ref _buffer, _length, start, length);
        return MemoryMarshal.CreateSpan(ref startReference, length);
    }

    [Pure]
    public ReadOnlySpan<T> SliceReadOnlySpan(Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(_length);
        return SliceReadOnlySpan(start, length);
    }

    [Pure]
    public ReadOnlySpan<T> SliceReadOnlySpan(int start) => SliceReadOnlySpan(start, _length - start);

    [Pure]
    public ReadOnlySpan<T> SliceReadOnlySpan(int start, int length)
    {
        ref T startReference = ref GetStart(ref _buffer, _length, start, length);
        return MemoryMarshal.CreateReadOnlySpan(ref startReference, length);
    }
}
