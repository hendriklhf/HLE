using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HLE.Collections;

/// <summary>
/// A type of dictionary that doesn't need .TryGetValue(...) or .TryAdd(...).
/// Data can only be added and retrieved via the indexer. If a key isn't found in the dictionary, null will be returned.
/// Therefore <typeparamref name="TValue"/> must be a reference type.
/// </summary>
/// <typeparam name="TKey">The key.</typeparam>
/// <typeparam name="TValue">The value.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public sealed class HDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull where TValue : class
{
    public int Count => _dictionary.Count;

    public TKey[] Keys => _dictionary.Keys.ToArray();

    public TValue[] Values => _dictionary.Values.ToArray();

    public TValue? this[TKey key]
    {
        get => Get(key);
        set => Set(key, value);
    }

    private readonly Dictionary<TKey, TValue> _dictionary;

    public HDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
        _dictionary = new(collection);
    }

    public HDictionary()
    {
        _dictionary = new();
    }

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public bool ContainsValue(TValue value) => _dictionary.ContainsValue(value);

    public bool Remove(TKey key) => _dictionary.Remove(key);

    public void Clear() => _dictionary.Clear();

    private TValue? Get(TKey key)
    {
        return _dictionary.TryGetValue(key, out TValue? value) ? value : default;
    }

    private void Set(TKey key, TValue? value)
    {
        if (value is null)
        {
            throw new InvalidOperationException($"{nameof(value)} is null. You can't add null to the dictionary.");
        }

        if (!_dictionary.TryAdd(key, value))
        {
            _dictionary[key] = value;
        }
    }

    public static implicit operator HDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return new(dictionary);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
