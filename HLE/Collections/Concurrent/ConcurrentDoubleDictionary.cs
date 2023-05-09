using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Collections.Concurrent;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> : IEnumerable<TValue>, IEquatable<ConcurrentDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>>
    where TPrimaryKey : IEquatable<TPrimaryKey> where TSecondaryKey : IEquatable<TSecondaryKey>
{
    public TValue this[TPrimaryKey key] => _values[key];

    public TValue this[TSecondaryKey key] => _values[_secondaryKeyTranslations[key]];

    public TValue this[TPrimaryKey primaryKey, TSecondaryKey secondaryKey]
    {
        set
        {
            if (!_values.ContainsKey(primaryKey))
            {
                throw new KeyNotFoundException("The primary key could not be found.");
            }

            if (!_secondaryKeyTranslations.TryGetValue(secondaryKey, out TPrimaryKey? secondaryKeyPartner))
            {
                throw new KeyNotFoundException("The secondary key could not be found.");
            }

            if (!secondaryKeyPartner.Equals(primaryKey))
            {
                throw new KeyNotFoundException("The given secondary key does not exists with the matching primary key.");
            }

            _values[primaryKey] = value;
            _secondaryKeyTranslations[secondaryKey] = primaryKey;
        }
    }

    public int Count => _values.Count;

    public IEnumerable<TValue> Values => _values.Values;

    internal readonly ConcurrentDictionary<TPrimaryKey, TValue> _values;
    internal readonly ConcurrentDictionary<TSecondaryKey, TPrimaryKey> _secondaryKeyTranslations;

    public ConcurrentDoubleDictionary()
    {
        _values = new();
        _secondaryKeyTranslations = new();
    }

    public ConcurrentDoubleDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null, IEqualityComparer<TSecondaryKey>? secondaryKeyComparer = null)
    {
        _values = new(concurrencyLevel, capacity, primaryKeyComparer);
        _secondaryKeyTranslations = new(concurrencyLevel, capacity, secondaryKeyComparer);
    }

    public ConcurrentDoubleDictionary(int capacity, IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null, IEqualityComparer<TSecondaryKey>? secondaryKeyComparer = null)
    {
        _values = new(Environment.ProcessorCount, capacity, primaryKeyComparer);
        _secondaryKeyTranslations = new(Environment.ProcessorCount, capacity, secondaryKeyComparer);
    }

    public ConcurrentDoubleDictionary(IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null, IEqualityComparer<TSecondaryKey>? secondaryKeyComparer = null)
    {
        _values = new(Environment.ProcessorCount, 0, primaryKeyComparer);
        _secondaryKeyTranslations = new(Environment.ProcessorCount, 0, secondaryKeyComparer);
    }

    public bool TryAdd(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        if (!_values.TryAdd(primaryKey, value))
        {
            return false;
        }

        if (_secondaryKeyTranslations.TryAdd(secondaryKey, primaryKey))
        {
            return true;
        }

        _values.Remove(primaryKey, out _);
        return false;
    }

    public void AddOrSet(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        bool primaryKeyExists = _values.ContainsKey(primaryKey);
        bool secondaryKeyExists = _secondaryKeyTranslations.TryGetValue(secondaryKey, out TPrimaryKey? secondaryKeyPartner);
        bool bothKeysExistAndOldPrimaryKeyEqualsNewPrimaryKey = primaryKeyExists && secondaryKeyExists && primaryKey.Equals(secondaryKeyPartner);
        bool bothKeysDontExist = !primaryKeyExists && !secondaryKeyExists;
        if (!bothKeysDontExist && !bothKeysExistAndOldPrimaryKeyEqualsNewPrimaryKey)
        {
            throw new KeyNotFoundException("The given secondary key does not exists with the matching primary key.");
        }

        _values.AddOrSet(primaryKey, value);
        _secondaryKeyTranslations.AddOrSet(secondaryKey, primaryKey);
    }

    public bool TryGetValue(TPrimaryKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _values.TryGetValue(key, out value);
    }

    public bool TryGetValue(TSecondaryKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_secondaryKeyTranslations.TryGetValue(key, out TPrimaryKey? primaryKey))
        {
            return _values.TryGetValue(primaryKey, out value);
        }

        value = default;
        return false;
    }

    public bool Remove(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
    {
        if (!_values.Remove(primaryKey, out TValue? value))
        {
            return false;
        }

        if (!_secondaryKeyTranslations[secondaryKey].Equals(primaryKey))
        {
            _values.TryAdd(primaryKey, value);
            return false;
        }

        _secondaryKeyTranslations.Remove(secondaryKey, out _);
        return true;
    }

    public void Clear()
    {
        _secondaryKeyTranslations.Clear();
        _values.Clear();
    }

    [Pure]
    public bool ContainsKey(TPrimaryKey key)
    {
        return _values.ContainsKey(key);
    }

    [Pure]
    public bool ContainsKey(TSecondaryKey key)
    {
        return _secondaryKeyTranslations.ContainsKey(key);
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
        return HashCode.Combine(_secondaryKeyTranslations, _values);
    }
}
