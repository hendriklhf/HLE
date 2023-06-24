using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> : IEnumerable<TValue>, ICountable, IEquatable<FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>>
    where TPrimaryKey : IEquatable<TPrimaryKey> where TSecondaryKey : IEquatable<TSecondaryKey>
{
    public TValue this[TPrimaryKey key] => _values[key];

    public TValue this[TSecondaryKey key] => _values[_secondaryKeyTranslations[key]];

    public int Count => _values.Count;

    public IEnumerable<TValue> Values => _values.Values;

    internal readonly FrozenDictionary<TPrimaryKey, TValue> _values;
    internal readonly FrozenDictionary<TSecondaryKey, TPrimaryKey> _secondaryKeyTranslations;

    public FrozenDoubleDictionary(DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> dictionary, bool optimizeForReading, IEqualityComparer<TPrimaryKey>? primaryKeyEqualityComparer = null,
        IEqualityComparer<TSecondaryKey>? secondaryKeyEqualityComparer = null)
    {
        _values = dictionary._values.ToFrozenDictionary(primaryKeyEqualityComparer, optimizeForReading);
        _secondaryKeyTranslations = dictionary._secondaryKeyTranslations.ToFrozenDictionary(secondaryKeyEqualityComparer, optimizeForReading);
    }

    public bool TryGetByPrimaryKey(TPrimaryKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _values.TryGetValue(key, out value);
    }

    public bool TryGetBySecondaryKey(TSecondaryKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_secondaryKeyTranslations.TryGetValue(key, out TPrimaryKey? primaryKey))
        {
            return _values.TryGetValue(primaryKey, out value);
        }

        value = default;
        return false;
    }

    [Pure]
    public bool ContainsPrimaryKey(TPrimaryKey key)
    {
        return _values.ContainsKey(key);
    }

    [Pure]
    public bool ContainsSecondaryKey(TSecondaryKey key)
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
    public bool Equals(FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return Equals(obj as FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>);
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(_secondaryKeyTranslations, _values);
    }
}
