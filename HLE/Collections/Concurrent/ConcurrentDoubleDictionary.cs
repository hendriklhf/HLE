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

    [SuppressMessage("Design", "CA1044:Properties should not be write only", Justification = "reading doesn't make sense")]
    public TValue this[TPrimaryKey primaryKey, TSecondaryKey secondaryKey]
    {
        set
        {
            ObjectDisposedException.ThrowIf(_dictionaryLock is null, typeof(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>));

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
    private SemaphoreSlim? _dictionaryLock = new(1);

    public ConcurrentDoubleDictionary()
    {
#pragma warning disable IDE0028
        _dictionary = new();
#pragma warning restore IDE0028
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
        _dictionaryLock?.Dispose();
        _dictionaryLock = null;
    }

    public bool TryAdd(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        ObjectDisposedException.ThrowIf(_dictionaryLock is null, typeof(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>));

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
        ObjectDisposedException.ThrowIf(_dictionaryLock is null, typeof(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>));

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
        ObjectDisposedException.ThrowIf(_dictionaryLock is null, typeof(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>));

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
        ObjectDisposedException.ThrowIf(_dictionaryLock is null, typeof(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>));

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
    public bool ContainsPrimaryKey(TPrimaryKey key) => _dictionary.ContainsPrimaryKey(key);

    [Pure]
    public bool ContainsSecondaryKey(TSecondaryKey key) => _dictionary.ContainsSecondaryKey(key);

    [Pure]
    public TValue[] ToArray() => _dictionary.ToArray();

    public IEnumerator<TValue> GetEnumerator() => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other)
        => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => _dictionary.GetHashCode();
}
