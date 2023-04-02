using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> : IEnumerable<TValue>, IEquatable<DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>> where TPrimaryKey : notnull where TSecondaryKey : notnull
{
    public TValue this[TPrimaryKey key] => _values[key];

    public TValue this[TSecondaryKey key] => _values[_secondaryKeyTranslations[key]];

    public int Count => _values.Count;

    public IEnumerable<TValue> Values => _values.Values;

    private readonly Dictionary<TSecondaryKey, TPrimaryKey> _secondaryKeyTranslations = new();
    private readonly Dictionary<TPrimaryKey, TValue> _values = new();

    public DoubleDictionary()
    {
    }

    public DoubleDictionary(int capacity)
    {
        _values = new(capacity);
        _secondaryKeyTranslations = new(capacity);
    }

    public void Add(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        _values.Add(primaryKey, value);
        try
        {
            _secondaryKeyTranslations.Add(secondaryKey, primaryKey);
        }
        catch
        {
            _values.Remove(primaryKey);
            throw;
        }
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

        _values.Remove(primaryKey);
        return false;
    }

    public bool TryGetValue(TPrimaryKey key, out TValue? value)
    {
        return _values.TryGetValue(key, out value);
    }

    public bool TryGetValue(TSecondaryKey key, out TValue? value)
    {
        if (_secondaryKeyTranslations.TryGetValue(key, out TPrimaryKey? primaryKey))
        {
            return _values.TryGetValue(primaryKey, out value);
        }

        value = default;
        return false;
    }

    public bool Remove(TPrimaryKey key)
    {
        return _values.Remove(key);
    }

    public bool Remove(TSecondaryKey key)
    {
        return _secondaryKeyTranslations.Remove(key, out TPrimaryKey? primaryKey) && _values.Remove(primaryKey);
    }

    public void Clear()
    {
        _secondaryKeyTranslations.Clear();
        _values.Clear();
    }

    [Pure]
    public bool ContainsPrimaryKey(TPrimaryKey key)
    {
        return _values.ContainsKey(key);
    }

    [Pure]
    public bool ContainsValue(TValue value)
    {
        return _values.ContainsValue(value);
    }

    public int EnsureCapacity(int capacity)
    {
        _secondaryKeyTranslations.EnsureCapacity(capacity);
        return _values.EnsureCapacity(capacity);
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
    public bool Equals(DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return Equals(obj as DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>);
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(_secondaryKeyTranslations, _values);
    }
}
