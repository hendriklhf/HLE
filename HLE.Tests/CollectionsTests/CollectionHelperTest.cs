using System;
using System.Collections.Generic;
using System.Linq;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.CollectionsTests;

[TestClass]
public class CollectionHelperTest
{
    [TestMethod]
    public void RandomTest()
    {
        const int arrLength = 10;
        string[] arr = TestHelper.CreateStringArray(arrLength, 10);
        for (int i = 0; i < 1000; i++)
        {
            Assert.IsTrue(arr.Contains(arr.Random()));
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
        arr = arr.Replace(i => i == 2, 4);
        Assert.AreEqual(5, arr.Count(i => i == 4));
    }

    [TestMethod]
    public void SplitTest()
    {
        string[] arr =
        {
            ".",
            ".",
            "A",
            "B",
            "C",
            ".",
            "D",
            ".",
            ".",
            "E",
            "F",
            "G",
            "H",
            ".",
            "I",
            "J",
            ".",
            "."
        };

        var split = arr.Split(".");
        Assert.AreEqual(4, split.Length);
        byte[] lengths =
        {
            3,
            1,
            4,
            2
        };
        for (int i = 0; i < split.Length; i++)
        {
            Assert.AreEqual(lengths[i], split[i].Length);
        }
    }

    [TestMethod]
    public void RandomStringTest()
    {
        const int stringLength = 50;
        char[] arr = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        string word = arr.RandomString(stringLength);
        Assert.AreEqual(stringLength, word.Length);
        Assert.IsTrue(word.All(c => arr.Contains(c)));
    }

    [TestMethod]
    public void IndicesOfTest()
    {
        const string str = "test string";
        int[] indices = str.IndicesOf(c => c is 's');
        Assert.IsTrue(indices is [2, 5]);
    }

    [TestMethod]
    public void RandomizeTest()
    {
        int[] arr = TestHelper.CreateIntArray(50);
        int[][] arrArr = Enumerable.Range(0, 100_000).Select(_ => arr.Randomize()).ToArray();
        int count = arrArr.Count(a => arr.SequenceEqual(a));
        Console.WriteLine($"Content was equal {NumberHelper.InsertKDots(count)} times.");
        Assert.IsTrue(count <= 1);
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

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            foreach (int _ in ..^100)
            {
            }
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            foreach (int _ in 50..)
            {
            }
        });
    }
}
