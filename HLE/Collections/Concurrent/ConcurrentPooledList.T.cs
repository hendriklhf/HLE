using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Memory;

namespace HLE.Collections.Concurrent;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
[CollectionBuilder(typeof(ConcurrentPooledList), nameof(ConcurrentPooledList.Create))]
public sealed class ConcurrentPooledList<T> : IList<T>, ICopyable<T>, ICountable, IEquatable<ConcurrentPooledList<T>>, IDisposable,
    IIndexAccessible<T>, IReadOnlyList<T>, ICollectionProvider<T>
    where T : IEquatable<T>
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
        Monitor.Enter(_list);
        try
        {
            _list.Add(item);
        }
        finally
        {
            Monitor.Exit(_list);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        Monitor.Enter(_list);
        try
        {
            _list.AddRange(items);
        }
        finally
        {
            Monitor.Exit(_list);
        }
    }

    public void AddRange(List<T> items) => AddRange(CollectionsMarshal.AsSpan(items));

    public void AddRange(params T[] items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(Span<T> items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(ReadOnlySpan<T> items)
    {
        Monitor.Enter(_list);
        try
        {
            _list.AddRange(items);
        }
        finally
        {
            Monitor.Exit(_list);
        }
    }

    public void Clear()
    {
        Monitor.Enter(_list);
        try
        {
            _list.Clear();
        }
        finally
        {
            Monitor.Exit(_list);
        }
    }

    public void EnsureCapacity(int capacity)
    {
        Monitor.Enter(_list);
        try
        {
            _list.EnsureCapacity(capacity);
        }
        finally
        {
            Monitor.Exit(_list);
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
            int index = _list.IndexOf(item);
            return index >= 0 && _list.Remove(item);
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
    public ArrayEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(ConcurrentPooledList<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => _list.GetHashCode();

    public static bool operator ==(ConcurrentPooledList<T>? left, ConcurrentPooledList<T>? right) => Equals(left, right);

    public static bool operator !=(ConcurrentPooledList<T>? left, ConcurrentPooledList<T>? right) => !(left == right);
}
