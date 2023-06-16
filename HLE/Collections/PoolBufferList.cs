using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class PoolBufferList<T> : IList<T>, ICopyable<T>, IEquatable<PoolBufferList<T>>, IDisposable where T : IEquatable<T>
{
    public T this[int index]
    {
        get => _bufferWriter.WrittenSpan[index];
        set => _bufferWriter.WrittenSpan[index] = value;
    }

    public T this[Index index]
    {
        get => _bufferWriter.WrittenSpan[index];
        set => _bufferWriter.WrittenSpan[index] = value;
    }

    public Span<T> this[Range range] => _bufferWriter.WrittenSpan[range];

    public int Count => _bufferWriter.Length;

    public int Capacity => _bufferWriter.Capacity;

    public bool IsReadOnly => false;

    private readonly PoolBufferWriter<T> _bufferWriter;

    public PoolBufferList() : this(5)
    {
    }

    public PoolBufferList(int capacity)
    {
        _bufferWriter = new(capacity, capacity << 1);
    }

    public PoolBufferList(int capacity, int defaultElementGrowth)
    {
        _bufferWriter = new(capacity, defaultElementGrowth);
    }

    public PoolBufferList(PoolBufferWriter<T> bufferWriter)
    {
        _bufferWriter = bufferWriter;
    }

    ~PoolBufferList()
    {
        _bufferWriter.Dispose();
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return _bufferWriter.WrittenSpan;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<T> AsMemory()
    {
        return _bufferWriter.WrittenMemory;
    }

    [Pure]
    public T[] ToArray()
    {
        return _bufferWriter.WrittenSpan.ToArray();
    }

    public void Add(T item)
    {
        _bufferWriter.GetSpan()[0] = item;
        _bufferWriter.Advance(1);
    }

    public void AddRange<TCollection>(TCollection items) where TCollection : IEnumerable<T>
    {
        if (items.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            AddRange(span);
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

    public void AddRange(ReadOnlySpan<T> items)
    {
        Span<T> destination = _bufferWriter.GetSpan(items.Length);
        items.CopyTo(destination);
        _bufferWriter.Advance(items.Length);
    }

    public void Clear()
    {
        _bufferWriter.Clear();
    }

    [Pure]
    public bool Contains(T item)
    {
        return _bufferWriter.WrittenSpan.Contains(item);
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        _bufferWriter.WrittenSpan[(index + 1)..].CopyTo(_bufferWriter.WrittenSpan[index..]);
        _bufferWriter.Advance(-1);
        return true;
    }

    [Pure]
    public int IndexOf(T item)
    {
        return _bufferWriter.WrittenSpan.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        _bufferWriter.GetSpan(1);
        _bufferWriter.Advance(1);
        _bufferWriter.WrittenSpan[index..^1].CopyTo(_bufferWriter.WrittenSpan[(index + 1)..]);
        _bufferWriter.WrittenSpan[index] = item;
    }

    public void RemoveAt(int index)
    {
        _bufferWriter.WrittenSpan[(index + 1)..].CopyTo(_bufferWriter.WrittenSpan[index..]);
        _bufferWriter.Advance(-1);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyTo(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(destination), offset));
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination.Span));
    }

    public void CopyTo(Span<T> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination));
    }

    public unsafe void CopyTo(ref T destination)
    {
        CopyTo((T*)Unsafe.AsPointer(ref destination));
    }

    public unsafe void CopyTo(T* destination)
    {
        T* source = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(_bufferWriter.WrittenSpan));
        Unsafe.CopyBlock(destination, source, (uint)(_bufferWriter.Length * sizeof(T)));
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _bufferWriter.Length; i++)
        {
            yield return _bufferWriter.WrittenSpan[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _bufferWriter.Dispose();
    }

    [Pure]
    public bool Equals(PoolBufferList<T>? other)
    {
        return ReferenceEquals(this, other) || Count == other?.Count && _bufferWriter.Equals(other._bufferWriter);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is PoolBufferList<T> other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return _bufferWriter.GetHashCode();
    }
}
