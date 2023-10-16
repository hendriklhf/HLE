using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>(
        DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> dictionary,
        IEqualityComparer<TPrimaryKey>? primaryKeyEqualityComparer = null,
        IEqualityComparer<TSecondaryKey>? secondaryKeyEqualityComparer = null
    )
    : IEnumerable<TValue>, ICountable, IEquatable<FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>>, IReadOnlySpanProvider<TValue>
    where TPrimaryKey : IEquatable<TPrimaryKey>
    where TSecondaryKey : IEquatable<TSecondaryKey>
{
    public TValue this[TPrimaryKey key] => _values[key];

    public TValue this[TSecondaryKey key] => _values[_secondaryKeyTranslations[key]];

    public int Count => _values.Count;

    public ImmutableArray<TValue> Values => _values.Values;

    internal readonly FrozenDictionary<TPrimaryKey, TValue> _values = dictionary._values.ToFrozenDictionary(primaryKeyEqualityComparer);
    internal readonly FrozenDictionary<TSecondaryKey, TPrimaryKey> _secondaryKeyTranslations = dictionary._secondaryKeyTranslations.ToFrozenDictionary(secondaryKeyEqualityComparer);

    public bool TryGetByPrimaryKey(TPrimaryKey key, [MaybeNullWhen(false)] out TValue value)
        => _values.TryGetValue(key, out value);

    public bool TryGetBySecondaryKey(TSecondaryKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_secondaryKeyTranslations.TryGetValue(key, out TPrimaryKey? primaryKey))
        {
            return TryGetByPrimaryKey(primaryKey, out value);
        }

        value = default;
        return false;
    }

    [Pure]
    public bool ContainsPrimaryKey(TPrimaryKey key) => _values.ContainsKey(key);

    [Pure]
    public bool ContainsSecondaryKey(TSecondaryKey key) => _secondaryKeyTranslations.ContainsKey(key);

    ReadOnlySpan<TValue> IReadOnlySpanProvider<TValue>.GetReadOnlySpan() => Values.AsSpan();

    public ArrayEnumerator<TValue> GetEnumerator()
    {
        TValue[]? array = ImmutableCollectionsMarshal.AsArray(Values);
        Debug.Assert(array is not null);
        return new(array, 0, Count);
    }

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(_secondaryKeyTranslations, _values);

    public static bool operator ==(FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? left, FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? right)
        => Equals(left, right);

    public static bool operator !=(FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? left, FrozenDoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? right)
        => !(left == right);
}
