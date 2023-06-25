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
public sealed class PoolBufferList<T> : IList<T>, ICopyable<T>, ICountable, IEquatable<PoolBufferList<T>>, IDisposable, IIndexAccessible<T> where T : IEquatable<T>
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

    public int Count => _bufferWriter.Count;

    public int Capacity => _bufferWriter.Capacity;

    public bool IsReadOnly => false;

    private readonly PoolBufferWriter<T> _bufferWriter;

    private const int _defaultCapacity = 16;

    public PoolBufferList() : this(_defaultCapacity)
    {
    }

    public PoolBufferList(int capacity)
    {
        _bufferWriter = new(capacity);
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

    [Pure]
    public List<T> ToList()
    {
        List<T> result = new(Count);
        CollectionsMarshal.SetCount(result, Count);
        Span<T> resultSpan = CollectionsMarshal.AsSpan(result);
        CopyTo(resultSpan);
        return result;
    }

    public void Add(T item)
    {
        _bufferWriter.GetReference() = item;
        _bufferWriter.Advance(1);
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetNonEnumeratedCount(out int itemsCount))
        {
            ref T destination = ref _bufferWriter.GetReference(itemsCount);
            T[] buffer = _bufferWriter._buffer.Array;
            if (items.TryNonEnumeratedCopyTo(buffer, Count))
            {
                _bufferWriter.Advance(itemsCount);
                return;
            }

            int i = 0;
            foreach (T item in items)
            {
                Unsafe.Add(ref destination, i++) = item;
            }

            _bufferWriter.Advance(itemsCount);
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

        ref T destinationReference = ref _bufferWriter.GetReference(items.Length);
        ref byte destinationReferenceAsByte = ref Unsafe.As<T, byte>(ref destinationReference);

        Unsafe.CopyBlock(ref destinationReferenceAsByte, ref sourceReferenceAsByte, (uint)(sizeof(T) * items.Length));
        _bufferWriter.Advance(items.Length);
    }

    public void Clear()
    {
        _bufferWriter.Clear();
    }

    public void EnsureCapacity(int capacity)
    {
        _bufferWriter.EnsureCapacity(capacity);
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
        _bufferWriter.GetSpan();
        _bufferWriter.Advance(1);
        _bufferWriter.WrittenSpan[index..^1].CopyTo(_bufferWriter.WrittenSpan[(index + 1)..]);
        _bufferWriter.WrittenSpan[index] = item;
    }

    public void RemoveAt(int index)
    {
        _bufferWriter.WrittenSpan[(index + 1)..].CopyTo(_bufferWriter.WrittenSpan[index..]);
        _bufferWriter.Advance(-1);
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopyableCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        DefaultCopyableCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        DefaultCopyableCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        DefaultCopyableCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        DefaultCopyableCopier<T> copier = new(AsSpan());
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        DefaultCopyableCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _bufferWriter.Count; i++)
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
