using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Collections.Concurrent;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> :
    IReadOnlyCollection<TValue>,
    ICountable,
    IEquatable<ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>>,
    ICollectionProvider<TValue>
    where TPrimaryKey : IEquatable<TPrimaryKey>
    where TSecondaryKey : IEquatable<TSecondaryKey>
{
    public TValue this[TPrimaryKey key] => _dictionary[key];

    public TValue this[TSecondaryKey key] => _dictionary[key];

    [SuppressMessage("Design", "CA1044:Properties should not be write only", Justification = "reading doesn't make sense")]
    public TValue this[TPrimaryKey primaryKey, TSecondaryKey secondaryKey]
    {
        set
        {
            lock (_dictionary)
            {
                _dictionary[primaryKey, secondaryKey] = value;
            }
        }
    }

    public int Count => _dictionary.Count;

    public IReadOnlyCollection<TValue> Values => _dictionary.Values;

    internal readonly DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> _dictionary;

    public ConcurrentDoubleDictionary() => _dictionary = [];

    public ConcurrentDoubleDictionary
    (
        int capacity,
        IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null,
        IEqualityComparer<TSecondaryKey>? secondaryKeyComparer = null
    )
        => _dictionary = new(capacity, primaryKeyComparer, secondaryKeyComparer);

    public ConcurrentDoubleDictionary
    (
        IEqualityComparer<TPrimaryKey>? primaryKeyComparer,
        IEqualityComparer<TSecondaryKey>? secondaryKeyComparer
    )
        => _dictionary = new(primaryKeyComparer, secondaryKeyComparer);

    public bool TryAdd(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        lock (_dictionary)
        {
            return _dictionary.TryAdd(primaryKey, secondaryKey, value);
        }
    }

    public void AddOrSet(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        lock (_dictionary)
        {
            _dictionary.AddOrSet(primaryKey, secondaryKey, value);
        }
    }

    public bool TryGetByPrimaryKey(TPrimaryKey key, [MaybeNullWhen(false)] out TValue value)
        => _dictionary.TryGetByPrimaryKey(key, out value);

    public bool TryGetBySecondaryKey(TSecondaryKey key, [MaybeNullWhen(false)] out TValue value)
        => _dictionary.TryGetBySecondaryKey(key, out value);

    public bool Remove(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
    {
        lock (_dictionary)
        {
            return _dictionary.Remove(primaryKey, secondaryKey);
        }
    }

    public void Clear()
    {
        lock (_dictionary)
        {
            _dictionary.Clear();
        }
    }

    [Pure]
    public bool ContainsPrimaryKey(TPrimaryKey key) => _dictionary.ContainsPrimaryKey(key);

    [Pure]
    public bool ContainsSecondaryKey(TSecondaryKey key) => _dictionary.ContainsSecondaryKey(key);

    [Pure]
    public TValue[] ToArray() => _dictionary.ToArray();

    [Pure]
    public TValue[] ToArray(int start) => _dictionary.ToArray(start..);

    [Pure]
    public TValue[] ToArray(int start, int length) => _dictionary.ToArray(start, length);

    [Pure]
    public TValue[] ToArray(Range range) => _dictionary.ToArray(range);

    [Pure]
    public List<TValue> ToList() => _dictionary.ToList();

    [Pure]
    public List<TValue> ToList(int start) => _dictionary.ToList(start..);

    [Pure]
    public List<TValue> ToList(int start, int length) => _dictionary.ToList(start, length);

    [Pure]
    public List<TValue> ToList(Range range) => _dictionary.ToList(range);

    // ReSharper disable once NotDisposedResourceIsReturned
    public IEnumerator<TValue> GetEnumerator() => _dictionary.Count == 0 ? EmptyEnumeratorCache<TValue>.Enumerator : _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other)
        => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => _dictionary.GetHashCode();

    public static bool operator ==(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? left, ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? right)
        => Equals(left, right);

    public static bool operator !=(ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? left, ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? right)
        => !(left == right);
}
