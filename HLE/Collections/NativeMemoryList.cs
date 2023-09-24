using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Collections;

public sealed class NativeMemoryList<T>(int capacity)
    : IList<T>, ICopyable<T>, ICountable, IEquatable<NativeMemoryList<T>>, IDisposable, IIndexAccessible<T>, IReadOnlyList<T>, ISpanProvider<T>
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

    internal NativeMemory<T> _buffer = new((int)BitOperations.RoundUpToPowerOf2((uint)capacity), false);

    private const int _defaultCapacity = 8;
    private const int _maximumCapacity = 1 << 30;

    public NativeMemoryList() : this(_defaultCapacity)
    {
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }

    [Pure]
    public Span<T> AsSpan() => _buffer.AsSpan(..Count);

    [Pure]
    public unsafe Span<T> AsSpan(int start) => new Slicer<T>(_buffer.Pointer, Count).CreateSpan(start);

    [Pure]
    public unsafe Span<T> AsSpan(int start, int length) => new Slicer<T>(_buffer.Pointer, Count).CreateSpan(start, length);

    [Pure]
    public unsafe Span<T> AsSpan(Range range) => new Slicer<T>(_buffer.Pointer, Count).CreateSpan(range);

    [Pure]
    public T[] ToArray()
    {
        Span<T> items = AsSpan();
        T[] result = GC.AllocateUninitializedArray<T>(Count);
        items.CopyToUnsafe(result);
        return result;
    }

    [Pure]
    public unsafe List<T> ToList()
    {
        List<T> result = new(Count);
        CopyWorker<T> copyWorker = new(_buffer.Pointer, Count);
        copyWorker.CopyTo(result);
        return result;
    }

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void GrowIfNeeded(int sizeHint)
    {
        int freeSpace = Capacity - Count;
        if (freeSpace >= sizeHint)
        {
            return;
        }

        if (Capacity == _maximumCapacity)
        {
            ThrowMaximumListCapacityReached();
        }

        int neededSize = sizeHint - freeSpace;
        using NativeMemory<T> oldBuffer = _buffer;
        int newLength = (int)BitOperations.RoundUpToPowerOf2((uint)(oldBuffer.Length + neededSize));
        if (newLength < Capacity)
        {
            ThrowMaximumListCapacityReached();
        }

        _buffer = new(newLength, false);
        CopyWorker<T> copyWorker = new(oldBuffer.Pointer, Count);
        copyWorker.CopyTo(_buffer.Pointer);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaximumListCapacityReached()
    {
        throw new InvalidOperationException("The maximum list capacity has been reached.");
    }

    public unsafe void Add(T item)
    {
        GrowIfNeeded(1);
        _buffer.Pointer[Count++] = item;
    }

    public unsafe void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            AddRange(span);
            return;
        }

        if (items.TryGetNonEnumeratedCount(out int itemsCount))
        {
            GrowIfNeeded(itemsCount);
            T* destination = _buffer.Pointer + Count;
            if (items is ICopyable<T> copyable)
            {
                copyable.CopyTo(destination);
                Count += itemsCount;
                return;
            }

            foreach (T item in items)
            {
                destination[Count++] = item;
            }

            return;
        }

        foreach (T item in items)
        {
            Add(item);
        }
    }

    public void AddRange(List<T> items)
    {
        AddRange((ReadOnlySpan<T>)CollectionsMarshal.AsSpan(items));
    }

    public void AddRange(params T[] items)
    {
        AddRange((ReadOnlySpan<T>)items);
    }

    public void AddRange(Span<T> items)
    {
        AddRange((ReadOnlySpan<T>)items);
    }

    public unsafe void AddRange(ReadOnlySpan<T> items)
    {
        GrowIfNeeded(items.Length);
        CopyWorker<T> copyWorker = new(items);
        copyWorker.CopyTo(_buffer.Pointer + Count);
        Count += items.Length;
    }

    public void Clear()
    {
        Count = 0;
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity < Capacity)
        {
            return;
        }

        int neededSpace = capacity - Capacity;
        GrowIfNeeded(neededSpace);
    }

    [Pure]
    public bool Contains(T item)
    {
        return AsSpan().Contains(item);
    }

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
    public int IndexOf(T item)
    {
        return AsSpan().IndexOf(item);
    }

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

    public unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    public bool Equals(NativeMemoryList<T>? other)
    {
        return ReferenceEquals(this, other) || Count == other?.Count && _buffer.Equals(other._buffer);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is NativeMemoryList<T> other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }
}
