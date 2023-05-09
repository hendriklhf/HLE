using System;
using HLE.Collections.Concurrent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Collections.Concurrent;

[TestClass]
public class ConcurrentPoolBufferListTest
{
    [TestMethod]
    public void IndexerTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        list.AddRange("helXo".AsSpan());
        Assert.AreEqual('h', list[0]);
        Assert.AreEqual('o', list[4]);
        Assert.AreEqual('X', list[^2]);
        Assert.IsTrue(list[..3].SequenceEqual("hel".AsSpan()));
    }

    [TestMethod]
    public void CountTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        list.AddRange("hello".AsSpan());
        Assert.AreEqual(5, list.Count);
    }

    [TestMethod]
    public void AddTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        for (int i = 0; i < 1000; i++)
        {
            list.Add('x');
        }

        Assert.AreEqual(1000, list.Count);
        Assert.IsTrue(list is ['x', .., 'x']);
    }

    [TestMethod]
    public void ClearTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        for (int i = 0; i < 1000; i++)
        {
            list.Add('x');
        }

        list.Clear();
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void RemoveTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        list.AddRange("hello".AsSpan());
        list.Remove('l');
        Assert.AreEqual("helo", new(list.AsSpan()));
    }

    [TestMethod]
    public void RemoveAtTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        list.AddRange("hello".AsSpan());
        list.RemoveAt(2);
        Assert.AreEqual("helo", new(list.AsSpan()));
    }

    [TestMethod]
    public void InsertTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        list.AddRange("helo".AsSpan());
        list.Insert(2, 'l');
        Assert.AreEqual("hello", new(list.AsSpan()));
    }

    [TestMethod]
    public void EnumeratorTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        list.AddRange("hello".AsSpan());
        Span<char> result = stackalloc char[5];
        int resultLength = 0;
        foreach (char c in list)
        {
            result[resultLength++] = c;
        }

        Assert.AreEqual("hello", new(result));
    }

    [TestMethod]
    public void AddRemoveLoopTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        for (int i = 0; i < 100_000; i++)
        {
            list.Add('x');
            list.Remove('x');
        }

        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void AddLoopTest()
    {
        using ConcurrentPoolBufferList<char> list = new();
        for (int i = 0; i < 100_000; i++)
        {
            list.Add('x');
        }

        Assert.AreEqual(100_000, list.Count);
        Assert.IsTrue(list is ['x', .., 'x']);
    }
}
