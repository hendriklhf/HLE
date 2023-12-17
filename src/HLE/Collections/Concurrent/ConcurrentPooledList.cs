using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Collections.Concurrent;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class ConcurrentPooledList<T> : IList<T>, ICopyable<T>, IEquatable<ConcurrentPooledList<T>>, IDisposable, IIndexAccessible<T>,
    IReadOnlyList<T>, ICollectionProvider<T>
{
    public T this[int index]
    {
        get
        {
            lock (_list)
            {
                return _list[index];
            }
        }
        set
        {
            lock (_list)
            {
                _list[index] = value;
            }
        }
    }

    public T this[Index index]
    {
        get => this[index.GetOffset(Count)];
        set => this[index.GetOffset(Count)] = value;
    }

    public int Count
    {
        get
        {
            lock (_list)
            {
                return _list.Count;
            }
        }
    }

    public int Capacity
    {
        get
        {
            lock (_list)
            {
                return _list.Capacity;
            }
        }
    }

    bool ICollection<T>.IsReadOnly => false;

    internal readonly PooledList<T> _list;

    public ConcurrentPooledList() => _list = [];

    public ConcurrentPooledList(ReadOnlySpan<T> items) => _list = new(items);

    public ConcurrentPooledList(int capacity) => _list = new(capacity);

    public void Dispose()
    {
        lock (_list)
        {
            _list.Dispose();
        }
    }

    [Pure]
    public T[] ToArray()
    {
        lock (_list)
        {
            return _list.ToArray();
        }
    }

    [Pure]
    public List<T> ToList()
    {
        lock (_list)
        {
            return _list.ToList();
        }
    }

    public void Add(T item)
    {
        lock (_list)
        {
            _list.Add(item);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        lock (_list)
        {
            _list.AddRange(items);
        }
    }

    public void AddRange(List<T> items) => AddRange(CollectionsMarshal.AsSpan(items));

    public void AddRange(params T[] items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(Span<T> items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(ReadOnlySpan<T> items)
    {
        lock (_list)
        {
            _list.AddRange(items);
        }
    }

    public void Clear()
    {
        lock (_list)
        {
            _list.Clear();
        }
    }

    public void EnsureCapacity(int capacity)
    {
        lock (_list)
        {
            _list.EnsureCapacity(capacity);
        }
    }

    [Pure]
    public bool Contains(T item)
    {
        lock (_list)
        {
            return _list.Contains(item);
        }
    }

    public bool Remove(T item)
    {
        lock (_list)
        {
            return _list.Remove(item);
        }
    }

    [Pure]
    public int IndexOf(T item)
    {
        lock (_list)
        {
            return _list.IndexOf(item);
        }
    }

    public void Insert(int index, T item)
    {
        lock (_list)
        {
            _list.Insert(index, item);
        }
    }

    public void RemoveAt(int index)
    {
        lock (_list)
        {
            _list.RemoveAt(index);
        }
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        lock (_list)
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination, offset);
        }
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        lock (_list)
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination, offset);
        }
    }

    public void CopyTo(Memory<T> destination)
    {
        lock (_list)
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination);
        }
    }

    public void CopyTo(Span<T> destination)
    {
        lock (_list)
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination);
        }
    }

    public void CopyTo(ref T destination)
    {
        lock (_list)
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(ref destination);
        }
    }

    public unsafe void CopyTo(T* destination)
    {
        lock (_list)
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination);
        }
    }

    // TODO: enumerator has to lock the list
    // ReSharper disable once InconsistentlySynchronizedField
    public ArrayEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] ConcurrentPooledList<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField", Justification = "doesnt need to be locked")]
    public override int GetHashCode() => _list.GetHashCode();

    public static bool operator ==(ConcurrentPooledList<T>? left, ConcurrentPooledList<T>? right) => Equals(left, right);

    public static bool operator !=(ConcurrentPooledList<T>? left, ConcurrentPooledList<T>? right) => !(left == right);
}
