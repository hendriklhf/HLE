using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

[method: MustDisposeResource]
public sealed unsafe class NativeMemoryList<T>(int capacity) :
    IList<T>,
    ICopyable<T>,
    IEquatable<NativeMemoryList<T>>,
    IDisposable,
    IIndexAccessible<T>,
    IReadOnlyList<T>,
    ISpanProvider<T>,
    ICollectionProvider<T>,
    IMemoryProvider<T>
    where T : unmanaged, IEquatable<T>
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

    public int Count { get; private set; }

    public int Capacity => _buffer.Length;

    bool ICollection<T>.IsReadOnly => false;

    [SuppressMessage("ReSharper", "NotDisposedResource", Justification = "assigned to a field")]
    internal NativeMemory<T> _buffer = capacity == 0 ? [] : new(BufferHelpers.GrowBuffer(0, (uint)capacity), false);

    [MustDisposeResource]
    public NativeMemoryList() : this(0)
    {
    }

    public void Dispose() => _buffer.Dispose();

    [Pure]
    public Span<T> AsSpan() => _buffer.AsSpan(..Count);

    [Pure]
    public Span<T> AsSpan(int start) => new Slicer<T>(_buffer.Pointer, Count).SliceSpan(start);

    [Pure]
    public Span<T> AsSpan(int start, int length) => new Slicer<T>(_buffer.Pointer, Count).SliceSpan(start, length);

    [Pure]
    public Span<T> AsSpan(Range range) => new Slicer<T>(_buffer.Pointer, Count).SliceSpan(range);

    [Pure]
    public Memory<T> AsMemory() => new NativeMemoryManager<T>(_buffer.Pointer, Count).Memory;

    [Pure]
    public T[] ToArray()
    {
        int count = Count;
        if (count == 0)
        {
            return [];
        }

        Span<T> source = AsSpan();
        T[] result = GC.AllocateUninitializedArray<T>(count);
        CopyWorker<T>.Copy(source, result);
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

        T* source = _buffer.Pointer;
        List<T> result = new(count);
        CopyWorker<T> copyWorker = new(source, count);
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

        int neededSize = sizeHint - freeSpace;
        Grow(neededSize);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // dont inline as slow path
    private void Grow(int neededSize)
    {
        Debug.Assert(neededSize >= 0);

        using NativeMemory<T> oldBuffer = _buffer;
        T* source = _buffer.Pointer;

        int newLength = BufferHelpers.GrowArray((uint)oldBuffer.Length, (uint)neededSize);
        NativeMemory<T> newBuffer = new(newLength, false);

        CopyWorker<T>.Copy(source, newBuffer.Pointer, (uint)Count);

        _buffer = newBuffer;
    }

    public void Add(T item)
    {
        GrowIfNeeded(1);
        _buffer.Pointer[Count++] = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            AddRange(span);
            return;
        }

        if (items.TryGetNonEnumeratedCount(out int itemsCount))
        {
            GrowIfNeeded(itemsCount);
            int count = Count;
            T* destination = _buffer.Pointer + count;
            if (items is ICopyable<T> copyable)
            {
                copyable.CopyTo(destination);
                Count = count + itemsCount;
                return;
            }

            foreach (T item in items)
            {
                destination[count++] = item;
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
        GrowIfNeeded(items.Length);
        CopyWorker<T> copyWorker = new(items);
        copyWorker.CopyTo(_buffer.Pointer + Count);
        Count += items.Length;
    }

    public void Clear() => Count = 0;

    public void EnsureCapacity(int capacity)
    {
        int currentCapacity = Capacity;
        if (capacity < currentCapacity)
        {
            return;
        }

        int neededSpace = capacity - currentCapacity;
        GrowIfNeeded(neededSpace);
    }

    [Pure]
    public bool Contains(T item) => AsSpan().Contains(item);

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        AsSpan((index + 1)..).CopyTo(AsSpan(index..));
        Count--;
        return true;
    }

    [Pure]
    public int IndexOf(T item) => AsSpan().IndexOf(item);

    public void Insert(int index, T item)
    {
        GrowIfNeeded(1);
        Count++;
        AsSpan(index..^1).CopyTo(AsSpan((index + 1)..));
        AsSpan()[index] = item;
    }

    public void RemoveAt(int index)
    {
        AsSpan((index + 1)..).CopyTo(AsSpan(index..));
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

    public void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public NativeMemoryEnumerator<T> GetEnumerator() => new(_buffer.Pointer, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] NativeMemoryList<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(NativeMemoryList<T>? left, NativeMemoryList<T>? right) => Equals(left, right);

    public static bool operator !=(NativeMemoryList<T>? left, NativeMemoryList<T>? right) => !(left == right);
}
