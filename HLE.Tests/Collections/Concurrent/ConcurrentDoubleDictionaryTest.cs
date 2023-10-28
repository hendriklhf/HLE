using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HLE.Collections.Concurrent;
using Xunit;

namespace HLE.Tests.Collections.Concurrent;

public sealed class ConcurrentConcurrentDoubleDictionaryTest
{
    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public void SetTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        Assert.Equal(value, dictionary[1]);
        Assert.Equal(value, dictionary["a"]);

        Assert.Throws<KeyNotFoundException>(() => { dictionary[1, "b"] = value; });

        Assert.Throws<KeyNotFoundException>(() => { dictionary[2, "a"] = value; });

        Assert.Throws<KeyNotFoundException>(() => { dictionary[2, "b"] = value; });

        Assert.Single(dictionary);
        dictionary[1, "a"] = "abc";
        Assert.Equal("abc", dictionary[1]);
        Assert.Equal("abc", dictionary["a"]);

        Assert.True(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void TryAddTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        Assert.False(dictionary.TryAdd(1, "a", value));
        Assert.False(dictionary.TryAdd(1, "b", value));
        Assert.False(dictionary.TryAdd(2, "a", value));

        Assert.True(dictionary.TryAdd(2, "b", value));
        Assert.Equal(2, dictionary.Count);

        Assert.True(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public void AddOrSetTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";

        dictionary.AddOrSet(1, "a", value);
        Assert.Equal(value, dictionary[1]);
        Assert.Equal(value, dictionary["a"]);
        Assert.Single(dictionary);

        dictionary.AddOrSet(1, "a", "abc");
        Assert.Equal("abc", dictionary[1]);
        Assert.Equal("abc", dictionary["a"]);
        Assert.Single(dictionary);

        Assert.Throws<KeyNotFoundException>(() => { dictionary.AddOrSet(1, "b", value); });

        Assert.Throws<KeyNotFoundException>(() => { dictionary.AddOrSet(2, "a", value); });

        dictionary.AddOrSet(2, "b", value);
        Assert.Equal(value, dictionary[2]);
        Assert.Equal(value, dictionary["b"]);
        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void TryGetValueTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        bool success = dictionary.TryGetByPrimaryKey(1, out string? retrievedValue);
        Assert.True(success);
        Assert.Equal(value, retrievedValue);

        success = dictionary.TryGetBySecondaryKey("a", out retrievedValue);
        Assert.True(success);
        Assert.Equal(value, retrievedValue);

        success = dictionary.TryGetByPrimaryKey(2, out retrievedValue);
        Assert.False(success);
        Assert.Null(retrievedValue);

        success = dictionary.TryGetBySecondaryKey("b", out retrievedValue);
        Assert.False(success);
        Assert.Null(retrievedValue);

        Assert.True(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void RemoveTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);
        dictionary.AddOrSet(2, "b", value);
        dictionary.AddOrSet(3, "c", value);

        Assert.False(dictionary.Remove(3, "b"));
        Assert.True(dictionary.ContainsPrimaryKey(3) && dictionary.ContainsSecondaryKey("b"));
        Assert.True(dictionary.Remove(3, "c"));

        Assert.Equal(2, dictionary.Count);
        Assert.Equal(value, dictionary[1]);
        Assert.Equal(value, dictionary["a"]);

        Assert.True(dictionary.TryAdd(3, "c", value));

        Assert.True(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void ClearTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);
        dictionary.AddOrSet(2, "b", value);
        dictionary.AddOrSet(3, "c", value);

        dictionary.Clear();
        Assert.Empty(dictionary);

        Assert.True(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void ContainsTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        Assert.True(dictionary.ContainsPrimaryKey(1));
        Assert.False(dictionary.ContainsPrimaryKey(2));

        Assert.True(dictionary.ContainsSecondaryKey("a"));
        Assert.False(dictionary.ContainsSecondaryKey("b"));

        Assert.True(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }
}
