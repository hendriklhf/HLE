using System;
using System.Collections.Generic;
using System.Linq;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Collections;

[TestClass]
public class CollectionHelperTest
{
    [TestMethod]
    public void RandomTest()
    {
        string[] array = Enumerable.Range(0, 1000).Select(static _ => Random.Shared.NextString(25)).ToArray();
        for (int i = 0; i < 1000; i++)
        {
            Assert.IsTrue(array.Contains(Random.Shared.GetItem(array)));
        }
    }

    [TestMethod]
    public void JoinToStringTest()
    {
        string[] arr =
        {
            "a",
            "b",
            "c"
        };
        Assert.AreEqual("a b c", arr.JoinToString(' '));
    }

    [TestMethod]
    public void ConcatToStringTest()
    {
        string[] arr =
        {
            "a",
            "b",
            "c"
        };

        Assert.AreEqual("abc", arr.ConcatToString());
    }

    [TestMethod]
    public void ReplaceTest()
    {
        int[] arr =
        {
            1,
            2,
            3,
            2,
            5,
            2,
            2,
            2,
            3,
            3
        };
        arr = arr.Replace(static i => i == 2, 4);
        Assert.AreEqual(5, arr.Count(static i => i == 4));
    }

    [TestMethod]
    public void IndicesOfTest()
    {
        const string str = "test string";
        int[] indices = str.IndicesOf(static c => c is 's');
        Assert.IsTrue(indices is [2, 5]);
    }

    [TestMethod]
    public void RangeEnumeratorTest()
    {
        List<int> items = new(101);
        Range r = ..100;
        foreach (int i in r)
        {
            items.Add(i);
        }

        Assert.AreEqual(101, items.Count);
        Assert.AreEqual(0, items[0]);
        Assert.AreEqual(100, items[^1]);

        items.Clear();
        r = 50..100;
        foreach (int i in r)
        {
            items.Add(i);
        }

        Assert.AreEqual(51, items.Count);
        Assert.AreEqual(50, items[0]);
        Assert.AreEqual(100, items[^1]);

        Assert.ThrowsException<InvalidOperationException>(static () =>
        {
            foreach (int _ in ..^100)
            {
            }
        });

        Assert.ThrowsException<InvalidOperationException>(static () =>
        {
            foreach (int _ in 50..)
            {
            }
        });
    }

    [TestMethod]
    public void TryGetReadOnlySpanTest()
    {
        IEnumerable<int> array = new[] { 0, 1, 2, 3, 4 };
        IEnumerable<int> list = new List<int> { 0, 1, 2, 3, 4 };
        IEnumerable<char> str = "hello";
        IEnumerable<int> enumerable = Enumerable.Range(0, 5).Select(static _ => Random.Shared.Next()).Where(static i => i > 0);

        bool succeeded = array.TryGetReadOnlySpan<int>(out ReadOnlySpan<int> arraySpan);
        Assert.IsTrue(succeeded && arraySpan is [0, 1, 2, 3, 4]);

        succeeded = list.TryGetReadOnlySpan<int>(out ReadOnlySpan<int> listSpan);
        Assert.IsTrue(succeeded && listSpan is [0, 1, 2, 3, 4]);

        succeeded = str.TryGetReadOnlySpan<char>(out ReadOnlySpan<char> stringSpan);
        Assert.IsTrue(succeeded && stringSpan is "hello");

        succeeded = enumerable.TryGetReadOnlySpan<int>(out ReadOnlySpan<int> enumerableSpan);
        Assert.IsFalse(succeeded && enumerableSpan.Length == 0);
    }

    [TestMethod]
    public void TryGetReadOnlyMemoryTest()
    {
        IEnumerable<int> array = new[] { 0, 1, 2, 3, 4 };
        IEnumerable<int> list = new List<int> { 0, 1, 2, 3, 4 };
        IEnumerable<char> str = "hello";
        IEnumerable<int> enumerable = Enumerable.Range(0, 5).Select(static _ => Random.Shared.Next()).Where(static i => i > 0);

        bool succeeded = array.TryGetReadOnlyMemory<int>(out ReadOnlyMemory<int> arrayMemory);
        Assert.IsTrue(succeeded && arrayMemory.Span is [0, 1, 2, 3, 4]);

        succeeded = list.TryGetReadOnlyMemory<int>(out ReadOnlyMemory<int> listMemory);
        Assert.IsTrue(succeeded && listMemory.Span is [0, 1, 2, 3, 4]);

        succeeded = str.TryGetReadOnlyMemory<char>(out ReadOnlyMemory<char> stringMemory);
        Assert.IsTrue(succeeded && stringMemory.Span is "hello");

        succeeded = enumerable.TryGetReadOnlyMemory<int>(out ReadOnlyMemory<int> enumerableMemory);
        Assert.IsFalse(succeeded && enumerableMemory.Length > 0);
    }

    [TestMethod]
    public void MoveTest()
    {
        Span<char> chars = "hello".ToCharArray();
        chars.MoveItem(1, 3);
        Assert.IsTrue(chars is ['h', 'l', 'l', 'e', 'o']);
        chars.MoveItem(3, 1);
        Assert.IsTrue(chars is ['h', 'e', 'l', 'l', 'o']);
    }
}
