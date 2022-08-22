using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HLE.Collections;

[DebuggerDisplay("Count = {Count}")]
public class HDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull
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
            return;
        }

        if (_dictionary.ContainsKey(key))
        {
            _dictionary[key] = value;
        }
        else
        {
            _dictionary.Add(key, value);
        }
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
