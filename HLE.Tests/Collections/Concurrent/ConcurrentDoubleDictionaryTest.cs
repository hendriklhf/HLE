using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HLE.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Collections.Concurrent;

[TestClass]
public class ConcurrentConcurrentDoubleDictionaryTest
{
    [TestMethod]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public void SetTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        Assert.AreEqual(value, dictionary[1]);
        Assert.AreEqual(value, dictionary["a"]);

        Assert.ThrowsException<KeyNotFoundException>(() => { dictionary[1, "b"] = value; });

        Assert.ThrowsException<KeyNotFoundException>(() => { dictionary[2, "a"] = value; });

        Assert.ThrowsException<KeyNotFoundException>(() => { dictionary[2, "b"] = value; });

        Assert.AreEqual(1, dictionary.Count);
        dictionary[1, "a"] = "abc";
        Assert.AreEqual("abc", dictionary[1]);
        Assert.AreEqual("abc", dictionary["a"]);

        Assert.IsTrue(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void TryAddTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        Assert.IsFalse(dictionary.TryAdd(1, "a", value));
        Assert.IsFalse(dictionary.TryAdd(1, "b", value));
        Assert.IsFalse(dictionary.TryAdd(2, "a", value));

        Assert.IsTrue(dictionary.TryAdd(2, "b", value));
        Assert.AreEqual(2, dictionary.Count);

        Assert.IsTrue(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public void AddOrSetTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";

        dictionary.AddOrSet(1, "a", value);
        Assert.AreEqual(dictionary[1], value);
        Assert.AreEqual(dictionary["a"], value);
        Assert.AreEqual(1, dictionary.Count);

        dictionary.AddOrSet(1, "a", "abc");
        Assert.AreEqual(dictionary[1], "abc");
        Assert.AreEqual(dictionary["a"], "abc");
        Assert.AreEqual(1, dictionary.Count);

        Assert.ThrowsException<KeyNotFoundException>(() => { dictionary.AddOrSet(1, "b", value); });

        Assert.ThrowsException<KeyNotFoundException>(() => { dictionary.AddOrSet(2, "a", value); });

        dictionary.AddOrSet(2, "b", value);
        Assert.AreEqual(dictionary[2], value);
        Assert.AreEqual(dictionary["b"], value);
        Assert.AreEqual(2, dictionary.Count);
        Assert.IsTrue(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void TryGetValueTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        bool success = dictionary.TryGetByPrimaryKey(1, out string? retrievedValue);
        Assert.IsTrue(success);
        Assert.AreEqual(value, retrievedValue);

        success = dictionary.TryGetBySecondaryKey("a", out retrievedValue);
        Assert.IsTrue(success);
        Assert.AreEqual(value, retrievedValue);

        success = dictionary.TryGetByPrimaryKey(2, out retrievedValue);
        Assert.IsFalse(success);
        Assert.IsNull(retrievedValue);

        success = dictionary.TryGetBySecondaryKey("b", out retrievedValue);
        Assert.IsFalse(success);
        Assert.IsNull(retrievedValue);

        Assert.IsTrue(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void RemoveTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);
        dictionary.AddOrSet(2, "b", value);
        dictionary.AddOrSet(3, "c", value);

        Assert.IsFalse(dictionary.Remove(3, "b"));
        Assert.IsTrue(dictionary.ContainsPrimaryKey(3) && dictionary.ContainsSecondaryKey("b"));
        Assert.IsTrue(dictionary.Remove(3, "c"));

        Assert.AreEqual(2, dictionary.Count);
        Assert.AreEqual(value, dictionary[1]);
        Assert.AreEqual(value, dictionary["a"]);

        Assert.IsTrue(dictionary.TryAdd(3, "c", value));

        Assert.IsTrue(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void ClearTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);
        dictionary.AddOrSet(2, "b", value);
        dictionary.AddOrSet(3, "c", value);

        dictionary.Clear();
        Assert.AreEqual(0, dictionary.Count);

        Assert.IsTrue(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void ContainsTest()
    {
        using ConcurrentDoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.AddOrSet(1, "a", value);

        Assert.IsTrue(dictionary.ContainsPrimaryKey(1));
        Assert.IsFalse(dictionary.ContainsPrimaryKey(2));

        Assert.IsTrue(dictionary.ContainsSecondaryKey("a"));
        Assert.IsFalse(dictionary.ContainsSecondaryKey("b"));

        Assert.IsTrue(dictionary._dictionary._values.Count == dictionary._dictionary._secondaryKeyTranslations.Count);
    }
}
