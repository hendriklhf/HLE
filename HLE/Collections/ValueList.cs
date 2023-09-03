using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Collections;

public ref struct ValueList<T> where T : IEquatable<T>
{
    public readonly ref T this[int index] => ref AsSpan()[index];

    public readonly ref T this[Index index] => ref AsSpan()[index];

    public readonly Span<T> this[Range range] => _buffer[range];

    public int Count { get; private set; }

    public readonly int Capacity => _buffer.Length;

    private readonly Span<T> _buffer = Span<T>.Empty;

    public static ValueList<T> Empty => new();

    public ValueList()
    {
    }

    public ValueList(Span<T> buffer)
    {
        _buffer = buffer;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan()
    {
        return _buffer[..Count];
    }

    [Pure]
    public readonly T[] ToArray()
    {
        return AsSpan().ToArray();
    }

    [Pure]
    public readonly List<T> ToList()
    {
        List<T> result = new(Count);
        CollectionsMarshal.SetCount(result, Count);
        Span<T> resultSpan = CollectionsMarshal.AsSpan(result);
        CopyTo(resultSpan);
        return result;
    }

    public void Add(T item)
    {
        ThrowIfNotEnoughSpace(1);
        _buffer[Count++] = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            ThrowIfNotEnoughSpace(span.Length);
            Span<T> destination = _buffer[Count..];
            span.CopyToUnsafe(destination);
            Count += span.Length;
            return;
        }

        if (items.TryGetNonEnumeratedCount(out int count))
        {
            ThrowIfNotEnoughSpace(count);
            ref T destinationReference = ref MemoryMarshal.GetReference(_buffer);
            foreach (T item in items)
            {
                Unsafe.Add(ref destinationReference, Count++) = item;
            }

            return;
        }

        foreach (T item in items)
        {
            Add(item);
        }
    }

    public void AddRange(List<T> items) => AddRange(CollectionsMarshal.AsSpan(items));

    public void AddRange(params T[] items) => AddRange(items.AsSpan());

    public void AddRange(Span<T> items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(ReadOnlySpan<T> items)
    {
        ThrowIfNotEnoughSpace(items.Length);
        Span<T> destination = _buffer[Count..];
        items.CopyToUnsafe(destination);
        Count += items.Length;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _buffer[..Count].Clear();
        }

        Count = 0;
    }

    [Pure]
    public readonly bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        Span<T> buffer = _buffer[..Count];
        buffer[(index + 1)..].CopyTo(buffer[index..]);
        Count--;
        return true;
    }

    [Pure]
    public readonly int IndexOf(T item)
    {
        return _buffer[..Count].IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);
        ThrowIfNotEnoughSpace(1);

        _buffer[index..^1].CopyTo(_buffer[(index + 1)..]);
        _buffer[index] = item;
        Count++;
    }

    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count);

        _buffer[(index + 1)..].CopyTo(_buffer[index..]);
        Count--;
    }

    public readonly void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public readonly void CopyTo(T[] destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public readonly void CopyTo(Memory<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public readonly void CopyTo(Span<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public readonly void CopyTo(ref T destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(ref destination);
    }

    public readonly unsafe void CopyTo(T* destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private readonly void ThrowIfNotEnoughSpace(int itemsToAdd)
    {
        if (itemsToAdd > Capacity - Count)
        {
            throw new InvalidOperationException("Maximum buffer capacity reached.");
        }
    }

    public readonly Enumerator GetEnumerator()
    {
        return new(_buffer[..Count]);
    }

    public readonly bool Equals(ValueList<T> other)
    {
        ref T thisReference = ref MemoryMarshal.GetReference(_buffer);
        ref T otherReference = ref MemoryMarshal.GetReference(other._buffer);
        return Unsafe.AreSame(ref thisReference, ref otherReference) && Count == other.Count;
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return false;
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode()
    {
        return 0;
    }

    public static bool operator ==(ValueList<T> left, ValueList<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ValueList<T> left, ValueList<T> right)
    {
        return !(left == right);
    }

    public ref struct Enumerator
    {
        public ref readonly T Current => ref _buffer[_index++];

        private readonly ReadOnlySpan<T> _buffer;
        private int _index;

        public Enumerator(ReadOnlySpan<T> buffer)
        {
            _buffer = buffer;
        }

        public readonly bool MoveNext() => _index < _buffer.Length;
    }
}
