using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Memory;

namespace HLE.Collections.Concurrent;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class ConcurrentPooledList<T> : IList<T>, ICopyable<T>, ICountable, IEquatable<ConcurrentPooledList<T>>, IDisposable,
    IIndexAccessible<T>, IReadOnlyList<T>, ICollectionProvider<T>
    where T : IEquatable<T>
{
    public T this[int index]
    {
        get => _list[index];
        set
        {
            ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

            _listLock.Wait();
            try
            {
                _list[index] = value;
            }
            finally
            {
                _listLock.Release();
            }
        }
    }

    public T this[Index index]
    {
        get => this[index.GetOffset(Count)];
        set => this[index.GetOffset(Count)] = value;
    }

    public int Count => _list.Count;

    public int Capacity => _list.Capacity;

    bool ICollection<T>.IsReadOnly => false;

    internal readonly PooledList<T> _list;
    private SemaphoreSlim? _listLock = new(1);

    public ConcurrentPooledList() => _list = [];

    public ConcurrentPooledList(int capacity) => _list = new(capacity);

    public void Dispose()
    {
        _list.Dispose();
        _listLock?.Dispose();
        _listLock = null;
    }

    [Pure]
    public T[] ToArray() => _list.ToArray();

    [Pure]
    public List<T> ToList() => _list.ToList();

    public void Add(T item)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            _list.Add(item);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            _list.AddRange(items);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void AddRange(List<T> items) => AddRange(CollectionsMarshal.AsSpan(items));

    public void AddRange(params T[] items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(Span<T> items) => AddRange((ReadOnlySpan<T>)items);

    public void AddRange(ReadOnlySpan<T> items)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            _list.AddRange(items);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            _list.Clear();
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void EnsureCapacity(int capacity)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            _list.EnsureCapacity(capacity);
        }
        finally
        {
            _listLock.Release();
        }
    }

    [Pure]
    public bool Contains(T item) => _list.Contains(item);

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            return _list.Remove(item);
        }
        finally
        {
            _listLock.Release();
        }
    }

    [Pure]
    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            _list.Insert(index, item);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void RemoveAt(int index)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            _list.RemoveAt(index);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination, offset);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination, offset);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void CopyTo(Memory<T> destination)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void CopyTo(Span<T> destination)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public void CopyTo(ref T destination)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(ref destination);
        }
        finally
        {
            _listLock.Release();
        }
    }

    public unsafe void CopyTo(T* destination)
    {
        ObjectDisposedException.ThrowIf(_listLock is null, typeof(ConcurrentPooledList<T>));

        _listLock.Wait();
        try
        {
            CopyWorker<T> copyWorker = new(_list.AsSpan());
            copyWorker.CopyTo(destination);
        }
        finally
        {
            _listLock.Release();
        }
    }

    // TODO: enumerator has to lock the semaphore
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
