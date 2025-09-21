using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue> :
    IReadOnlyCollection<TValue>,
    IReadOnlyDictionary<TPrimaryKey, TValue>,
    ICountable,
    IEquatable<DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>>,
    ICollectionProvider<TValue>
    where TPrimaryKey : IEquatable<TPrimaryKey>
    where TSecondaryKey : IEquatable<TSecondaryKey>
{
    public TValue this[TPrimaryKey key] => _values[key];

    public TValue this[TSecondaryKey key] => _values[_secondaryKeyTranslations[key]];

    [SuppressMessage("Design", "CA1044:Properties should not be write only", Justification = "reading makes no sense here")]
    public TValue this[TPrimaryKey primaryKey, TSecondaryKey secondaryKey]
    {
        set
        {
            ref TValue valueRef = ref CollectionsMarshal.GetValueRefOrNullRef(_values, primaryKey);
            if (Unsafe.IsNullRef(ref valueRef))
            {
                ThrowKeyNotFoundException("The primary key could not be found.");
            }

            TValue oldValue = valueRef;
            valueRef = value;

            ref TPrimaryKey primaryKeyRef = ref CollectionsMarshal.GetValueRefOrNullRef(_secondaryKeyTranslations, secondaryKey);
            if (Unsafe.IsNullRef(ref primaryKeyRef))
            {
                valueRef = oldValue;
                ThrowKeyNotFoundException("The secondary key could not be found.");
            }

            if (!primaryKeyRef.Equals(primaryKey))
            {
                valueRef = oldValue;
                ThrowKeyNotFoundException("The given secondary key does not exists with the matching primary key.");
            }

            primaryKeyRef = primaryKey;
        }
    }

    public int Count => _values.Count;

    public IReadOnlyCollection<TPrimaryKey> PrimaryKeys => _values.Keys;

    public IReadOnlyCollection<TSecondaryKey> SecondaryKeys => _secondaryKeyTranslations.Keys;

    IEnumerable<TPrimaryKey> IReadOnlyDictionary<TPrimaryKey, TValue>.Keys => PrimaryKeys;

    IEnumerable<TValue> IReadOnlyDictionary<TPrimaryKey, TValue>.Values => Values;

    public IReadOnlyCollection<TValue> Values => _values.Values;

    internal readonly Dictionary<TPrimaryKey, TValue> _values;
    internal readonly Dictionary<TSecondaryKey, TPrimaryKey> _secondaryKeyTranslations;

    public DoubleDictionary()
    {
        _values = [];
        _secondaryKeyTranslations = [];
    }

    public DoubleDictionary(IEqualityComparer<TPrimaryKey>? primaryKeyComparer, IEqualityComparer<TSecondaryKey>? secondaryKeyComparer)
    {
        _values = new(primaryKeyComparer);
        _secondaryKeyTranslations = new(secondaryKeyComparer);
    }

    public DoubleDictionary(int capacity, IEqualityComparer<TPrimaryKey>? primaryKeyComparer = null, IEqualityComparer<TSecondaryKey>? secondaryKeyComparer = null)
    {
        _values = new(capacity, primaryKeyComparer);
        _secondaryKeyTranslations = new(capacity, secondaryKeyComparer);
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

    public void AddOrSet(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
    {
        ref TValue? valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_values, primaryKey, out bool primaryKeyExists);
        ref TPrimaryKey? primaryKeyRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_secondaryKeyTranslations, secondaryKey, out bool secondaryKeyExists);

        bool bothKeysExistAndOldPrimaryKeyEqualsNewPrimaryKey = primaryKeyExists && secondaryKeyExists && primaryKey.Equals(primaryKeyRef);
        bool bothKeysDontExist = !primaryKeyExists && !secondaryKeyExists;
        if (!bothKeysExistAndOldPrimaryKeyEqualsNewPrimaryKey && !bothKeysDontExist)
        {
            if (!primaryKeyExists)
            {
                _values.Remove(primaryKey);
            }

            if (!secondaryKeyExists)
            {
                _secondaryKeyTranslations.Remove(secondaryKey);
            }

            ThrowKeyNotFoundException("The given secondary key does not exist with the matching primary key.");
        }

        valueRef = value;
        primaryKeyRef = primaryKey;
    }

    public bool TryGetByPrimaryKey(TPrimaryKey key, [MaybeNullWhen(false)] out TValue value)
        => _values.TryGetValue(key, out value);

    public bool TryGetBySecondaryKey(TSecondaryKey key, [MaybeNullWhen(false)] out TValue value)
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
            _values.Add(primaryKey, value);
            return false;
        }

        _secondaryKeyTranslations.Remove(secondaryKey);
        return true;
    }

    public void Clear()
    {
        _secondaryKeyTranslations.Clear();
        _values.Clear();
    }

    [Pure]
    public bool ContainsPrimaryKey(TPrimaryKey key) => _values.ContainsKey(key);

    [Pure]
    public bool ContainsSecondaryKey(TSecondaryKey key) => _secondaryKeyTranslations.ContainsKey(key);

    [Pure]
    public bool ContainsValue(TValue value) => _values.ContainsValue(value);

    bool IReadOnlyDictionary<TPrimaryKey, TValue>.ContainsKey(TPrimaryKey key) => ContainsPrimaryKey(key);

    bool IReadOnlyDictionary<TPrimaryKey, TValue>.TryGetValue(TPrimaryKey key, [MaybeNullWhen(false)] out TValue value) => TryGetByPrimaryKey(key, out value);

    public void EnsureCapacity(int capacity)
    {
        _secondaryKeyTranslations.EnsureCapacity(capacity);
        _values.EnsureCapacity(capacity);
    }

    public TValue[] ToArray()
    {
        int count = Count;
        if (count == 0)
        {
            return [];
        }

        TValue[] result = GC.AllocateUninitializedArray<TValue>(count);
        _values.Values.TryEnumerateInto(result, out int writtenElementCount);
        Debug.Assert(writtenElementCount == count);
        return result;
    }

    [Pure]
    public TValue[] ToArray(int start) => ToArray(start..Count);

    [Pure]
    public TValue[] ToArray(int start, int length) => _values.Values.Skip(start).Take(length).ToArray();

    [Pure]
    public TValue[] ToArray(Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(Count);
        return ToArray(start, length);
    }

    [Pure]
    public List<TValue> ToList()
    {
        int count = Count;
        if (count == 0)
        {
            return [];
        }

        List<TValue> result = new(count);
        CollectionsMarshal.SetCount(result, count);
        Span<TValue> buffer = CollectionsMarshal.AsSpan(result);
        _values.Values.TryEnumerateInto(buffer, out int writtenElementCount);
        Debug.Assert(writtenElementCount == count);
        return result;
    }

    [Pure]
    public List<TValue> ToList(int start) => ToList(start..Count);

    [Pure]
    public List<TValue> ToList(Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(Count);
        return ToList(start, length);
    }

    [Pure]
    public List<TValue> ToList(int start, int length) => _values.Values.Skip(start).Take(length).ToList();

    [DoesNotReturn]
    private static void ThrowKeyNotFoundException(string message) => throw new KeyNotFoundException(message);

    // ReSharper disable once NotDisposedResourceIsReturned
    public IEnumerator<TValue> GetEnumerator() => Count == 0 ? EmptyEnumeratorCache<TValue>.Enumerator : Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IEnumerator<KeyValuePair<TPrimaryKey, TValue>> IEnumerable<KeyValuePair<TPrimaryKey, TValue>>.GetEnumerator() => _values.GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(_values, _secondaryKeyTranslations);

    public static bool operator ==(
        DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? left,
        DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? right
    )
        => Equals(left, right);

    public static bool operator !=(
        DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? left,
        DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? right
    )
        => !(left == right);
}
