using System;
using System.Collections.Generic;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Collections;

[TestClass]
public class DoubleDictionaryTest
{
    [TestMethod]
    public void AddTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

        Assert.AreEqual(value, dictionary[1]);
        Assert.AreEqual(value, dictionary["a"]);

        Assert.ThrowsException<ArgumentException>(() =>
        {
            dictionary.Add(1, "a", value);
        });

        Assert.ThrowsException<ArgumentException>(() =>
        {
            dictionary.Add(2, "a", value);
        });

        Assert.ThrowsException<ArgumentException>(() =>
        {
            dictionary.Add(1, "b", value);
        });

        Assert.AreEqual(1, dictionary.Count);

        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void SetTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

        Assert.AreEqual(value, dictionary[1]);
        Assert.AreEqual(value, dictionary["a"]);

        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            dictionary[1, "b"] = value;
        });

        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            dictionary[2, "a"] = value;
        });

        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            dictionary[2, "b"] = value;
        });

        Assert.AreEqual(1, dictionary.Count);
        dictionary[1, "a"] = "abc";
        Assert.AreEqual("abc", dictionary[1]);
        Assert.AreEqual("abc", dictionary["a"]);

        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void TryAddTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

        Assert.IsFalse(dictionary.TryAdd(1, "a", value));
        Assert.IsFalse(dictionary.TryAdd(1, "b", value));
        Assert.IsFalse(dictionary.TryAdd(2, "a", value));

        Assert.IsTrue(dictionary.TryAdd(2, "b", value));
        Assert.AreEqual(2, dictionary.Count);

        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void AddOrSetTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";

        dictionary.AddOrSet(1, "a", value);
        Assert.AreEqual(dictionary[1], value);
        Assert.AreEqual(dictionary["a"], value);
        Assert.AreEqual(1, dictionary.Count);

        dictionary.AddOrSet(1, "a", "abc");
        Assert.AreEqual(dictionary[1], "abc");
        Assert.AreEqual(dictionary["a"], "abc");
        Assert.AreEqual(1, dictionary.Count);

        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            dictionary.AddOrSet(1, "b", value);
        });

        Assert.ThrowsException<KeyNotFoundException>(() =>
        {
            dictionary.AddOrSet(2, "a", value);
        });

        dictionary.AddOrSet(2, "b", value);
        Assert.AreEqual(dictionary[2], value);
        Assert.AreEqual(dictionary["b"], value);
        Assert.AreEqual(2, dictionary.Count);
        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void TryGetValueTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

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

        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void RemoveTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);
        dictionary.Add(2, "b", value);
        dictionary.Add(3, "c", value);

        Assert.IsFalse(dictionary.Remove(3, "b"));
        Assert.IsTrue(dictionary.ContainsPrimaryKey(3) && dictionary.ContainsSecondaryKey("b"));
        Assert.IsTrue(dictionary.Remove(3, "c"));

        Assert.AreEqual(2, dictionary.Count);
        Assert.AreEqual(value, dictionary[1]);
        Assert.AreEqual(value, dictionary["a"]);

        Assert.IsTrue(dictionary.TryAdd(3, "c", value));

        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void ClearTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);
        dictionary.Add(2, "b", value);
        dictionary.Add(3, "c", value);

        dictionary.Clear();
        Assert.AreEqual(0, dictionary.Count);

        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }

    [TestMethod]
    public void ContainsTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

        Assert.IsTrue(dictionary.ContainsPrimaryKey(1));
        Assert.IsFalse(dictionary.ContainsPrimaryKey(2));

        Assert.IsTrue(dictionary.ContainsSecondaryKey("a"));
        Assert.IsFalse(dictionary.ContainsSecondaryKey("b"));

        Assert.IsTrue(dictionary.ContainsValue(value));
        Assert.IsFalse(dictionary.ContainsValue("abc"));

        Assert.IsTrue(dictionary._values.Count == dictionary._secondaryKeyTranslations.Count);
    }
}
