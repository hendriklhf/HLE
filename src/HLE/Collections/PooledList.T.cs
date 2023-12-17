using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(PooledList), nameof(PooledList.Create))]
public sealed class PooledList<T>(int capacity)
    : IList<T>, ICopyable<T>, IEquatable<PooledList<T>>, IDisposable, IIndexAccessible<T>, IReadOnlyList<T>, ISpanProvider<T>,
        ICollectionProvider<T>, IMemoryProvider<T>
{
    public ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);
            return ref Unsafe.Add(ref _buffer.Reference, index);
        }
    }

    T IIndexAccessible<T>.this[int index] => this[index];

    T IReadOnlyList<T>.this[int index] => this[index];

    T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }

    public ref T this[Index index] => ref this[index.GetOffset(Count)];

    public Span<T> this[Range range] => AsSpan(range);

    public int Count { get; internal set; }

    public int Capacity => _buffer.Length;

    bool ICollection<T>.IsReadOnly => false;

    [SuppressMessage("ReSharper", "NotDisposedResource", Justification = "is disposed in Dispose()")]
    internal RentedArray<T> _buffer = capacity == 0 ? [] : ArrayPool<T>.Shared.RentAsRentedArray(capacity);

    public PooledList() : this(0)
    {
    }

    public PooledList(ReadOnlySpan<T> items) : this(items.Length)
    {
        CopyWorker<T>.Copy(items, _buffer.AsSpan());
        Count = items.Length;
    }

    public void Dispose() => _buffer.Dispose();

    [Pure]
    public Span<T> AsSpan() => _buffer.AsSpan(..Count);

    [Pure]
    public Span<T> AsSpan(int start) => new Slicer<T>(ref _buffer.Reference, Count).SliceSpan(start);

    [Pure]
    public Span<T> AsSpan(int start, int length) => new Slicer<T>(ref _buffer.Reference, Count).SliceSpan(start, length);

    [Pure]
    public Span<T> AsSpan(Range range) => new Slicer<T>(ref _buffer.Reference, Count).SliceSpan(range);

    [Pure]
    public Memory<T> AsMemory() => _buffer.AsMemory(..Count);

    [Pure]
    public T[] ToArray()
    {
        int count = Count;
        if (count == 0)
        {
            return [];
        }

        T[] result = GC.AllocateUninitializedArray<T>(count);
        CopyWorker<T>.Copy(_buffer.AsSpan(..count), result);
        return result;
    }

    [Pure]
    public List<T> ToList()
    {
        int count = Count;
        if (count == 0)
        {
            return [];
        }

        List<T> result = new(count);
        CopyWorker<T> copyWorker = new(ref _buffer.Reference, count);
        copyWorker.CopyTo(result);
        return result;
    }

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

    Memory<T> IMemoryProvider<T>.GetMemory() => AsMemory();

    ReadOnlyMemory<T> IReadOnlyMemoryProvider<T>.GetReadOnlyMemory() => AsMemory();

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private void GrowIfNeeded(int sizeHint)
    {
        int freeSpace = Capacity - Count;
        if (freeSpace >= sizeHint)
        {
            return;
        }

        Grow(sizeHint - freeSpace);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private void Grow(int neededSize)
    {
        int count = Count;
        using RentedArray<T> oldBuffer = _buffer;
        int newSize = BufferHelpers.GrowArray(oldBuffer.Length, neededSize);
        RentedArray<T> newBuffer = ArrayPool<T>.Shared.RentAsRentedArray(newSize);
        if (count != 0)
        {
            CopyWorker<T>.Copy(oldBuffer.AsSpan(..count), newBuffer.AsSpan());
        }

        _buffer = newBuffer;
    }

    public void Add(T item)
    {
        GrowIfNeeded(1);
        Unsafe.Add(ref _buffer.Reference, Count++) = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetNonEnumeratedCount(out int itemsCount))
        {
            if (itemsCount == 0)
            {
                return;
            }

            GrowIfNeeded(itemsCount);
            T[] buffer = _buffer.Array;
            int count = Count;
            if (items.TryNonEnumeratedCopyTo(buffer, count))
            {
                Count = count + itemsCount;
                return;
            }

            ref T destination = ref _buffer.Reference;
            foreach (T item in items)
            {
                Unsafe.Add(ref destination, count++) = item;
            }

            Count = count;
            return;
        }

        foreach (T item in items)
        {
            Add(item);
        }
    }

    public void AddRange(List<T> items) => AddRange((ReadOnlySpan<T>)CollectionsMarshal.AsSpan(items));

    public void AddRange(params T[] items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(Span<T> items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(ReadOnlySpan<T> items)
    {
        if (items.Length == 0)
        {
            return;
        }

        GrowIfNeeded(items.Length);
        int count = Count;
        ref T destination = ref Unsafe.Add(ref _buffer.Reference, count);
        CopyWorker<T> copyWorker = new(items);
        copyWorker.CopyTo(ref destination);
        Count = count + items.Length;
    }

    /// <summary>
    /// Trims unused buffer size.<br/>
    /// This method should ideally be called, when <see cref="Capacity"/> of the <see cref="PooledList{T}"/> is much larger than <see cref="Count"/>.
    /// </summary>
    /// <example>
    /// After having removed a lot of items from the <see cref="PooledList{T}"/> <see cref="Capacity"/> will be much larger than <see cref="Count"/>.
    /// If there are 32 items remaining and the <see cref="Capacity"/> is 1024, the buffer of 1024 items will be returned to the <see cref="ArrayPool{T}"/>
    /// and a new buffer that has at least the size of the remaining items will be rented and the remaining 32 items are copied into it.
    /// </example>
    public void TrimBuffer()
    {
        int count = Count;
        int trimmedBufferSize = BufferHelpers.GrowArray(count, 0);
        if (trimmedBufferSize == Capacity)
        {
            return;
        }

        using RentedArray<T> oldBuffer = _buffer;
        if (trimmedBufferSize == 0)
        {
            oldBuffer.Dispose();
            _buffer = [];
            return;
        }

        RentedArray<T> newBuffer = ArrayPool<T>.Shared.RentAsRentedArray(trimmedBufferSize);
        CopyWorker<T>.Copy(ref oldBuffer.Reference, ref newBuffer.Reference, (uint)count);
        _buffer = newBuffer;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _buffer.AsSpan(..Count).Clear();
        }

        Count = 0;
    }

    public void EnsureCapacity(int capacity) => GrowIfNeeded(capacity - Capacity);

    [Pure]
    public bool Contains(T item) => IndexOf(item) >= 0;

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        RentedArray<T> buffer = _buffer;
        buffer.AsSpan((index + 1)..).CopyTo(buffer.AsSpan(index..));
        Count--;
        return true;
    }

    [Pure]
    public int IndexOf(T item) => Array.IndexOf(_buffer.Array, item, 0, Count);

    public void Insert(int index, T item)
    {
        GrowIfNeeded(1);
        Count++;
        RentedArray<T> buffer = _buffer;
        buffer.AsSpan(index..^1).CopyTo(buffer.AsSpan((index + 1)..));
        buffer[index] = item;
    }

    public void RemoveAt(int index)
    {
        RentedArray<T> buffer = _buffer;
        buffer.AsSpan((index + 1)..).CopyTo(buffer.AsSpan(index..));
        Count--;
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    [Pure]
    public ArrayEnumerator<T> GetEnumerator() => new(_buffer.Array, 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] PooledList<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(PooledList<T>? left, PooledList<T>? right) => Equals(left, right);

    public static bool operator !=(PooledList<T>? left, PooledList<T>? right) => !(left == right);
}
