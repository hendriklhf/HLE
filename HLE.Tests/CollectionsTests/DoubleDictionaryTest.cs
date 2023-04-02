using System;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.CollectionsTests;

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
    }

    [TestMethod]
    public void TryGetValueTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

        bool success = dictionary.TryGetValue(1, out string? retrievedValue);
        Assert.IsTrue(success);
        Assert.AreEqual(value, retrievedValue);

        success = dictionary.TryGetValue("a", out retrievedValue);
        Assert.IsTrue(success);
        Assert.AreEqual(value, retrievedValue);

        success = dictionary.TryGetValue(2, out retrievedValue);
        Assert.IsFalse(success);
        Assert.IsNull(retrievedValue);

        success = dictionary.TryGetValue("b", out retrievedValue);
        Assert.IsFalse(success);
        Assert.IsNull(retrievedValue);
    }

    [TestMethod]
    public void RemoveTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);
        dictionary.Add(2, "b", value);
        dictionary.Add(3, "c", value);

        Assert.IsTrue(dictionary.Remove(3));
        Assert.IsFalse(dictionary.Remove(3));

        Assert.IsTrue(dictionary.Remove("b"));
        Assert.IsFalse(dictionary.Remove("b"));

        Assert.AreEqual(1, dictionary.Count);
        Assert.AreEqual(value, dictionary[1]);
        Assert.AreEqual(value, dictionary["a"]);
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
    }

    [TestMethod]
    public void ContainsTest()
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

        Assert.IsTrue(dictionary.ContainsPrimaryKey(1));
        Assert.IsFalse(dictionary.ContainsPrimaryKey(2));

        Assert.IsTrue(dictionary.ContainsValue(value));
        Assert.IsFalse(dictionary.ContainsValue("abc"));
    }

    [TestMethod]
    [DataRow(5)]
    [DataRow(10)]
    [DataRow(50)]
    public void CapacityTest(int capacity)
    {
        DoubleDictionary<int, string, string> dictionary = new();
        const string value = "xd";
        dictionary.Add(1, "a", value);

        Assert.IsTrue(dictionary.EnsureCapacity(capacity) >= capacity);
    }
}
