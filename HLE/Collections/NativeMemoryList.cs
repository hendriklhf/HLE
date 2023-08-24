using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Collections;

public sealed class NativeMemoryList<T> : IList<T>, ICopyable<T>, ICountable, IEquatable<NativeMemoryList<T>>, IDisposable, IIndexAccessible<T>, IReadOnlyList<T>
    where T : unmanaged, IEquatable<T>
{
    public ref T this[int index] => ref AsSpan()[index];

    T IIndexAccessible<T>.this[int index] => this[index];

    T IReadOnlyList<T>.this[int index] => this[index];

    T IList<T>.this[int index]
    {
        get => AsSpan()[index];
        set => AsSpan()[index] = value;
    }

    public ref T this[Index index] => ref AsSpan()[index];

    public Span<T> this[Range range] => AsSpan()[range];

    public int Count { get; private set; }

    public int Capacity => _buffer.Length;

    bool ICollection<T>.IsReadOnly => false;

    internal NativeMemory<T> _buffer;

    private const int _defaultCapacity = 16;

    public NativeMemoryList() : this(_defaultCapacity)
    {
    }

    public NativeMemoryList(int capacity)
    {
        _buffer = new(capacity, false);
    }

    ~NativeMemoryList()
    {
        _buffer.Dispose();
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return _buffer[..Count];
    }

    [Pure]
    public T[] ToArray()
    {
        return AsSpan().ToArray();
    }

    [Pure]
    public List<T> ToList()
    {
        List<T> result = new(Count);
        CollectionsMarshal.SetCount(result, Count);
        Span<T> resultSpan = CollectionsMarshal.AsSpan(result);
        CopyTo(resultSpan);
        return result;
    }

    private void GrowIfNeeded(int neededSpace)
    {
        int freeSpace = Capacity - Count;
        if (freeSpace >= neededSpace)
        {
            return;
        }

        if (neededSpace < _defaultCapacity)
        {
            neededSpace = _defaultCapacity;
        }

        using NativeMemory<T> oldBuffer = _buffer;
        NativeMemory<T> newBuffer = new(oldBuffer.Length + neededSpace, false);
        oldBuffer.CopyTo(newBuffer.AsSpan());
        _buffer = newBuffer;
    }

    public unsafe void Add(T item)
    {
        GrowIfNeeded(1);
        T* ptr = NativeMemoryMarshal<T>.GetPointer(_buffer);
        ptr[Count++] = item;
    }

    public unsafe void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetNonEnumeratedCount(out int itemsCount))
        {
            GrowIfNeeded(itemsCount);
            T* destination = NativeMemoryMarshal<T>.GetPointer(_buffer) + Count;
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
        AddRange(CollectionsMarshal.AsSpan(items));
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
        ref T sourceReference = ref MemoryMarshal.GetReference(items);
        ref byte sourceReferenceAsByte = ref Unsafe.As<T, byte>(ref sourceReference);

        GrowIfNeeded(items.Length);
        ref T destinationReference = ref Unsafe.AsRef<T>(NativeMemoryMarshal<T>.GetPointer(_buffer) + Count);
        ref byte destinationReferenceAsByte = ref Unsafe.As<T, byte>(ref destinationReference);

        Unsafe.CopyBlock(ref destinationReferenceAsByte, ref sourceReferenceAsByte, (uint)(sizeof(T) * items.Length));
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

        AsSpan()[(index + 1)..].CopyTo(AsSpan()[index..]);
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
        AsSpan()[index..^1].CopyTo(AsSpan()[(index + 1)..]);
        AsSpan()[index] = item;
    }

    public void RemoveAt(int index)
    {
        AsSpan()[(index + 1)..].CopyTo(AsSpan()[index..]);
        Count--;
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _buffer.Dispose();
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
