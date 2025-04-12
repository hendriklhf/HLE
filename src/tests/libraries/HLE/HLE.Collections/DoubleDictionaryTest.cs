using System;
using System.Collections.Generic;
using Xunit;

namespace HLE.Collections.UnitTests;

public sealed class DoubleDictionaryTest
{
    [Fact]
    public void AddTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";
        dictionary.Add(1, "a", Value);

        Assert.Equal(Value, dictionary[1]);
        Assert.Equal(Value, dictionary["a"]);

        Assert.Throws<ArgumentException>(() => dictionary.Add(1, "a", Value));

        Assert.Throws<ArgumentException>(() => dictionary.Add(2, "a", Value));

        Assert.Throws<ArgumentException>(() => dictionary.Add(1, "b", Value));

        Assert.Single(dictionary);

        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void SetTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";
        dictionary.Add(1, "a", Value);

        Assert.Equal(Value, dictionary[1]);
        Assert.Equal(Value, dictionary["a"]);

        Assert.Throws<KeyNotFoundException>(() => dictionary[1, "b"] = Value);

        Assert.Throws<KeyNotFoundException>(() => dictionary[2, "a"] = Value);

        Assert.Throws<KeyNotFoundException>(() => dictionary[2, "b"] = Value);

        Assert.Single(dictionary);
        dictionary[1, "a"] = "abc";
        Assert.Equal("abc", dictionary[1]);
        Assert.Equal("abc", dictionary["a"]);

        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void TryAddTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";
        dictionary.Add(1, "a", Value);

        Assert.False(dictionary.TryAdd(1, "a", Value));
        Assert.False(dictionary.TryAdd(1, "b", Value));
        Assert.False(dictionary.TryAdd(2, "a", Value));

        Assert.True(dictionary.TryAdd(2, "b", Value));
        Assert.Equal(2, dictionary.Count);

        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void AddOrSetTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";

        dictionary.AddOrSet(1, "a", Value);
        Assert.Equal(Value, dictionary[1]);
        Assert.Equal(Value, dictionary["a"]);
        Assert.Single(dictionary);

        dictionary.AddOrSet(1, "a", "abc");
        Assert.Equal("abc", dictionary[1]);
        Assert.Equal("abc", dictionary["a"]);
        Assert.Single(dictionary);

        Assert.Throws<KeyNotFoundException>(() => dictionary.AddOrSet(1, "b", Value));

        Assert.Throws<KeyNotFoundException>(() => dictionary.AddOrSet(2, "a", Value));

        dictionary.AddOrSet(2, "b", Value);
        Assert.Equal(Value, dictionary[2]);
        Assert.Equal(Value, dictionary["b"]);
        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void TryGetValueTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";
        dictionary.Add(1, "a", Value);

        bool success = dictionary.TryGetByPrimaryKey(1, out string? retrievedValue);
        Assert.True(success);
        Assert.Equal(Value, retrievedValue);

        success = dictionary.TryGetBySecondaryKey("a", out retrievedValue);
        Assert.True(success);
        Assert.Equal(Value, retrievedValue);

        success = dictionary.TryGetByPrimaryKey(2, out retrievedValue);
        Assert.False(success);
        Assert.Null(retrievedValue);

        success = dictionary.TryGetBySecondaryKey("b", out retrievedValue);
        Assert.False(success);
        Assert.Null(retrievedValue);

        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void RemoveTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";
        dictionary.Add(1, "a", Value);
        dictionary.Add(2, "b", Value);
        dictionary.Add(3, "c", Value);

        Assert.False(dictionary.Remove(3, "b"));
        Assert.True(dictionary.ContainsPrimaryKey(3) && dictionary.ContainsSecondaryKey("b"));
        Assert.True(dictionary.Remove(3, "c"));

        Assert.Equal(2, dictionary.Count);
        Assert.Equal(Value, dictionary[1]);
        Assert.Equal(Value, dictionary["a"]);

        Assert.True(dictionary.TryAdd(3, "c", Value));

        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void ClearTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";
        dictionary.Add(1, "a", Value);
        dictionary.Add(2, "b", Value);
        dictionary.Add(3, "c", Value);

        dictionary.Clear();
        Assert.Empty(dictionary);

        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void ContainsTest()
    {
        DoubleDictionary<int, string, string> dictionary = [];
        const string Value = "xd";
        dictionary.Add(1, "a", Value);

        Assert.True(dictionary.ContainsPrimaryKey(1));
        Assert.False(dictionary.ContainsPrimaryKey(2));

        Assert.True(dictionary.ContainsSecondaryKey("a"));
        Assert.False(dictionary.ContainsSecondaryKey("b"));

        Assert.True(dictionary.ContainsValue(Value));
        Assert.False(dictionary.ContainsValue("abc"));

        Assert.True(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [Fact]
    public void EmptyDictionariesReturnSameEnumeratorTest()
    {
        DoubleDictionary<int, string, Type> dictionary1 = [];
        DoubleDictionary<char, nuint, Type> dictionary2 = [];

        using IEnumerator<Type> enumerator1 = dictionary1.GetEnumerator();
        using IEnumerator<Type> enumerator2 = dictionary2.GetEnumerator();

        Assert.Same(enumerator1, enumerator2);
    }
}
