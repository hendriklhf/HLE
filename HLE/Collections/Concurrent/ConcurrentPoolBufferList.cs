﻿using System;
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
public sealed class ConcurrentPoolBufferList<T> : IList<T>, ICopyable<T>, IEquatable<ConcurrentPoolBufferList<T>>, IDisposable where T : IEquatable<T>
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
    private readonly SemaphoreSlim _bufferWriterLock = new(1);

    public ConcurrentPoolBufferList() : this(5)
    {
    }

    public ConcurrentPoolBufferList(int capacity, int defaultElementGrowth = 10)
    {
        _bufferWriter = new(capacity, defaultElementGrowth);
    }

    public ConcurrentPoolBufferList(PoolBufferWriter<T> bufferWriter)
    {
        _bufferWriter = bufferWriter;
    }

    ~ConcurrentPoolBufferList()
    {
        _bufferWriter.Dispose();
        _bufferWriterLock.Dispose();
    }

    [Pure]
    public Span<T> AsSpan()
    {
        return _bufferWriter.WrittenSpan;
    }

    [Pure]
    public Memory<T> AsMemory()
    {
        return _bufferWriter.WrittenMemory;
    }

    [Pure]
    public T[] ToArray()
    {
        return _bufferWriter.WrittenSpan.ToArray();
    }

    private void AddWithLock(T item)
    {
        _bufferWriterLock.Wait();
        try
        {
            AddWithoutLock(item);
        }
        finally
        {
            _bufferWriterLock.Release();
        }
    }

    private void AddWithoutLock(T item)
    {
        _bufferWriter.GetSpan()[0] = item;
        _bufferWriter.Advance(1);
    }

    public void Add(T item)
    {
        AddWithLock(item);
    }

    public void AddRange<TCollection>(TCollection items) where TCollection : IEnumerable<T>
    {
        switch (items)
        {
            case T[] array:
                AddRange(array);
                break;
            case List<T> list:
                AddRange(list);
                break;
            default:
                _bufferWriterLock.Wait();
                try
                {
                    foreach (T item in items)
                    {
                        AddWithoutLock(item);
                    }
                }
                finally
                {
                    _bufferWriterLock.Release();
                }

                break;
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
        _bufferWriterLock.Wait();
        try
        {
            Span<T> destination = _bufferWriter.GetSpan(items.Length);
            items.CopyTo(destination);
            _bufferWriter.Advance(items.Length);
        }
        finally
        {
            _bufferWriterLock.Release();
        }
    }

    public void Clear()
    {
        _bufferWriterLock.Wait();
        try
        {
            _bufferWriter.Clear();
        }
        finally
        {
            _bufferWriterLock.Release();
        }
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

        _bufferWriterLock.Wait();
        try
        {
            _bufferWriter.WrittenSpan[(index + 1)..].CopyTo(_bufferWriter.WrittenSpan[index..]);
            _bufferWriter.Advance(-1);
        }
        finally
        {
            _bufferWriterLock.Release();
        }

        return true;
    }

    [Pure]
    public int IndexOf(T item)
    {
        return _bufferWriter.WrittenSpan.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        _bufferWriterLock.Wait();
        try
        {
            _bufferWriter.GetSpan(1);
            _bufferWriter.Advance(1);
            _bufferWriter.WrittenSpan[index..^1].CopyTo(_bufferWriter.WrittenSpan[(index + 1)..]);
            _bufferWriter.WrittenSpan[index] = item;
        }
        finally
        {
            _bufferWriterLock.Release();
        }
    }

    public void RemoveAt(int index)
    {
        _bufferWriterLock.Wait();
        try
        {
            _bufferWriter.WrittenSpan[(index + 1)..].CopyTo(_bufferWriter.WrittenSpan[index..]);
            _bufferWriter.Advance(-1);
        }
        finally
        {
            _bufferWriterLock.Release();
        }
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
        _bufferWriterLock.Dispose();
    }

    [Pure]
    public bool Equals(ConcurrentPoolBufferList<T>? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ConcurrentPoolBufferList<T> other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
    }
}