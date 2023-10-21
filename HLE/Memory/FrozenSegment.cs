using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using HLE.Collections;

namespace HLE.Memory;

public unsafe struct FrozenSegment<T> : IDisposable, IEquatable<FrozenSegment<T>>, ISpanProvider<T>, ICountable, ICopyable<T>, IIndexAccessible<T>, IReadOnlyCollection<T>
    where T : unmanaged, IEquatable<T>
{
    public readonly ref T this[int index] => ref _heap[index];

    readonly T IIndexAccessible<T>.this[int index] => this[index];

    public readonly ref T this[Index index] => ref _heap[index];

    public readonly Span<T> this[Range range] => _heap[range];

    public readonly int Length => _heap.Length;

    readonly int ICountable.Count => Length;

    readonly int IReadOnlyCollection<T>.Count => Length;

    private readonly nint _segmentHandle;
    private bool _registered;
    private NativeMemory<T> _heap = NativeMemory<T>.Empty;

    public FrozenSegment()
    {
    }

    public FrozenSegment(int elementCount)
    {
        _heap = new(elementCount);
        _segmentHandle = FrozenHeap.RegisterSegment((nint)_heap.Pointer, _heap.Length);
        _registered = true;
    }

    public void Dispose()
    {
        if (_registered)
        {
            FrozenHeap.UnregisterSegment(_segmentHandle);
            _registered = false;
        }

        _heap.Dispose();
    }

    [Pure]
    public readonly Span<T> AsSpan() => _heap.AsSpan();

    [Pure]
    public readonly Span<T> AsSpan(int start) => _heap.AsSpan(start..);

    [Pure]
    public readonly Span<T> AsSpan(int start, int length) => _heap.AsSpan(start, length);

    [Pure]
    public readonly Span<T> AsSpan(Range range) => _heap.AsSpan(range);

    [Pure]
    public readonly Span<T> GetSpan() => AsSpan();

    public readonly void CopyTo(List<T> destination, int offset = 0) => _heap.CopyTo(destination, offset);

    public readonly void CopyTo(T[] destination, int offset = 0) => _heap.CopyTo(destination, offset);

    public readonly void CopyTo(Memory<T> destination) => _heap.CopyTo(destination);

    public readonly void CopyTo(Span<T> destination) => _heap.CopyTo(destination);

    public readonly void CopyTo(ref T destination) => _heap.CopyTo(ref destination);

    public readonly void CopyTo(T* destination) => _heap.CopyTo(destination);

    public readonly NativeMemoryEnumerator<T> GetEnumerator() => _heap.GetEnumerator();

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public readonly bool Equals(FrozenSegment<T> other) =>
        _segmentHandle == other._segmentHandle &&
        _registered == other._registered &&
        _heap == other._heap;

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj) => obj is FrozenSegment<T> other && Equals(other);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => _segmentHandle.GetHashCode();

    public static bool operator ==(FrozenSegment<T> left, FrozenSegment<T> right) => left.Equals(right);

    public static bool operator !=(FrozenSegment<T> left, FrozenSegment<T> right) => !(left == right);
}
