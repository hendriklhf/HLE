using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;

namespace HLE.Collections.Concurrent;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> : IEnumerable<TValue>, ICountable, IEquatable<ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>>, IDisposable
    where TPrimaryKey : IEquatable<TPrimaryKey> where TSecondaryKey : IEquatable<TSecondaryKey>
{
    public TValue this[TPrimaryKey key] => _dictionary[key];

    public TValue this[TSecondaryKey key] => _dictionary[key];

    [SuppressMessage("Design", "CA1044:Properties should not be write only")]
    public TValue this[TPrimaryKey primaryKey, TSecondaryKey secondaryKey]
    {
        set
        {
            _dictionaryLock.Wait();
            try
            {
                _dictionary[primaryKey, secondaryKey] = value;
            }
            finally
            {
                _dictionaryLock.Release();
            }
        }
    }

    public int Count => _dictionary.Count;

    public IReadOnlyCollection<TValue> Values => _dictionary.Values;

    internal readonly DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> _dictionary;
    private readonly SemaphoreSlim _dictionaryLock = new(1);

    public ConcurrentDoubleDictionary()
    {
        _dictionary = new();
    }

    public ConcurrentDoubleDictionary(int capacity, IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null, IEqualityComparer<TSecondaryKey>? secondaryKeyComparer = null)
    {
        _dictionary = new(capacity, primaryKeyComparer, secondaryKeyComparer);
    }

    public ConcurrentDoubleDictionary(IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null, IEqualityComparer<TSecondaryKey>? secondaryKeyComparer = null)
    {
        _dictionary = new(primaryKeyComparer, secondaryKeyComparer);
    }

    public void Dispose()
    {
        _dictionaryLock.Dispose();
    }

    public bool TryAdd(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        _dictionaryLock.Wait();
        try
        {
            return _dictionary.TryAdd(primaryKey, secondaryKey, value);
        }
        finally
        {
            _dictionaryLock.Release();
        }
    }

    public void AddOrSet(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        _dictionaryLock.Wait();
        try
        {
            _dictionary.AddOrSet(primaryKey, secondaryKey, value);
        }
        finally
        {
            _dictionaryLock.Release();
        }
    }

    public bool TryGetByPrimaryKey(TPrimaryKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetByPrimaryKey(key, out value);
    }

    public bool TryGetBySecondaryKey(TSecondaryKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dictionary.TryGetBySecondaryKey(key, out value);
    }

    public bool Remove(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
    {
        _dictionaryLock.Wait();
        try
        {
            return _dictionary.Remove(primaryKey, secondaryKey);
        }
        finally
        {
            _dictionaryLock.Release();
        }
    }

    public void Clear()
    {
        _dictionaryLock.Wait();
        try
        {
            _dictionary.Clear();
        }
        finally
        {
            _dictionaryLock.Release();
        }
    }

    [Pure]
    public bool ContainsPrimaryKey(TPrimaryKey key)
    {
        return _dictionary.ContainsPrimaryKey(key);
    }

    [Pure]
    public bool ContainsSecondaryKey(TSecondaryKey key)
    {
        return _dictionary.ContainsSecondaryKey(key);
    }

    [Pure]
    public TValue[] ToArray()
    {
        return _dictionary.ToArray();
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    public bool Equals(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> other && Equals(other);
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(_dictionary, _dictionaryLock);
    }
}
