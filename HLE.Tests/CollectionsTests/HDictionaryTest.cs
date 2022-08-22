using System.Collections.Generic;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.CollectionsTests;

[TestClass]
public class HDictionaryTest
{
    [TestMethod]
    public void InitializationTest()
    {
        HDictionary<int, string> dic = new();
        Assert.IsTrue(dic.Count == 0);
        Assert.IsTrue(dic.Keys.Length == 0);
        Assert.IsTrue(dic.Values.Length == 0);

        KeyValuePair<int, string>[] kvp =
        {
            new(5, "hello"),
            new(150, "mmm"),
            new(375, "123")
        };
        dic = new(kvp);
        Assert.IsTrue(dic.Count == 3);
        Assert.IsTrue(dic.Keys.Length == 3);
        Assert.IsTrue(dic.Values.Length == 3);
    }

    [TestMethod]
    public void IndexerTest()
    {
        KeyValuePair<string, int>[] kvp =
        {
            new("hello", 5),
            new("mmm", 10),
            new("123", 20)
        };
        HDictionary<string, int> dic = new(kvp);
        
        Assert.AreEqual(5, dic["hello"]);
        Assert.AreEqual(10, dic["mmm"]);
        Assert.AreEqual(20, dic["123"]);
        Assert.AreEqual(0,  dic["xd"]);

        dic["xd"] = 12;
        Assert.AreEqual(12, dic["xd"]);
        Assert.IsTrue(dic.Count == 4);
    }
}
