using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Count = {Count}")]
public sealed class DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>
    : IEnumerable<TValue>, ICountable, IEquatable<DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>>
    where TPrimaryKey : IEquatable<TPrimaryKey> where TSecondaryKey : IEquatable<TSecondaryKey>
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

    public IReadOnlyCollection<TValue> Values => _values.Values;

    internal readonly Dictionary<TPrimaryKey, TValue> _values;
    internal readonly Dictionary<TSecondaryKey, TPrimaryKey> _secondaryKeyTranslations;

    public DoubleDictionary()
    {
#pragma warning disable IDE0028
        _values = new();
        _secondaryKeyTranslations = new();
#pragma warning restore IDE0028
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

            ThrowKeyNotFoundException("The given secondary key does not exists with the matching primary key.");
        }

        valueRef = value;
        primaryKeyRef = primaryKey;
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
    public bool ContainsPrimaryKey(TPrimaryKey key)
    {
        return _values.ContainsKey(key);
    }

    [Pure]
    public bool ContainsSecondaryKey(TSecondaryKey key)
    {
        return _secondaryKeyTranslations.ContainsKey(key);
    }

    [Pure]
    public bool ContainsValue(TValue value)
    {
        return _values.ContainsValue(value);
    }

    public void EnsureCapacity(int capacity)
    {
        _secondaryKeyTranslations.EnsureCapacity(capacity);
        _values.EnsureCapacity(capacity);
    }

    public TValue[] ToArray()
    {
        TValue[] result = GC.AllocateUninitializedArray<TValue>(Count);
        _values.Values.TryEnumerateInto(result, out int writtenElementCount);
        Debug.Assert(writtenElementCount == Count);
        return result;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFoundException(string message) => throw new KeyNotFoundException(message);

    public IEnumerator<TValue> GetEnumerator() => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(DoubleDictionary<TPrimaryKey, TSecondaryKey, TValue>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => HashCode.Combine(_values, _secondaryKeyTranslations);
}
