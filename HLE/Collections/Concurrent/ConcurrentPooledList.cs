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
public sealed class ConcurrentPooledList<T> : IList<T>, ICopyable<T>, ICountable, IEquatable<ConcurrentPooledList<T>>, IDisposable, IIndexAccessible<T>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    public T this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public T this[Index index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public Span<T> this[Range range] => _list[range];

    public int Count => _list.Count;

    public int Capacity => _list.Capacity;

    public bool IsReadOnly => false;

    internal readonly PooledList<T> _list;
    private readonly SemaphoreSlim _listLock = new(1);

    public ConcurrentPooledList()
    {
        _list = new();
    }

    public ConcurrentPooledList(int capacity)
    {
        _list = new(capacity);
    }

    public ConcurrentPooledList(PooledBufferWriter<T> bufferWriter)
    {
        _list = new(bufferWriter);
    }

    ~ConcurrentPooledList()
    {
        _list.Dispose();
        _listLock.Dispose();
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        return _list.AsSpan();
    }

    [Pure]
    public T[] ToArray()
    {
        return _list.ToArray();
    }

    [Pure]
    public List<T> ToList()
    {
        return _list.ToList();
    }

    public void Add(T item)
    {
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
    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

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
    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
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

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _list.Dispose();
        _listLock.Dispose();
    }

    [Pure]
    public bool Equals(ConcurrentPooledList<T>? other)
    {
        return ReferenceEquals(this, other) || Count == other?.Count && _list.Equals(other._list);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ConcurrentPooledList<T> other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return _list.GetHashCode();
    }
}
