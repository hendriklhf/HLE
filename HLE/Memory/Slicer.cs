using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

internal readonly ref struct Slicer<T>
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

    public unsafe Slicer(T* pointer, int length) : this(ref Unsafe.AsRef<T>(pointer), length)
    {
    }

    public Slicer(ref T bufferReference, int length)
    {
        _buffer = ref bufferReference;
        _length = length;
    }

    [Pure]
    public Span<T> CreateSpan(Range range)
    {
        int start = range.Start.GetOffset(_length);
        int end = range.End.GetOffset(_length);
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start);

        int length = end - start;
        return CreateSpan(start, length);
    }

    [Pure]
    public Span<T> CreateSpan(int start) => CreateSpan(start, _length - start);

    [Pure]
    public Span<T> CreateSpan(int start, int length)
    {
        ref T startReference = ref GetStartReferenceAndValidate(start, length);
        return MemoryMarshal.CreateSpan(ref startReference, length);
    }

    [Pure]
    public ReadOnlySpan<T> CreateReadOnlySpan(Range range)
    {
        int start = range.Start.GetOffset(_length);
        int length = range.End.GetOffset(_length) - start;
        return CreateReadOnlySpan(start, length);
    }

    [Pure]
    public ReadOnlySpan<T> CreateReadOnlySpan(int start) => CreateReadOnlySpan(start, _length - start);

    [Pure]
    public ReadOnlySpan<T> CreateReadOnlySpan(int start, int length)
    {
        ref T startReference = ref GetStartReferenceAndValidate(start, length);
        return MemoryMarshal.CreateReadOnlySpan(ref startReference, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref T GetStartReferenceAndValidate(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)start, (uint)_length);
        ArgumentOutOfRangeException.ThrowIfNegative(_length - (uint)start - (uint)length);
        return ref Unsafe.Add(ref _buffer, start);
    }
}
