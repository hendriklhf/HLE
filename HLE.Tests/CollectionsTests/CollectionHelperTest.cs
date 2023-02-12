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
    public void ForEachTest()
    {
        int idx = 0;
        const int arraySize = 50;
        int[] arr = new int[arraySize];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = i;
        }

        arr.ForEach(_ => idx++);
        Assert.AreEqual(arraySize, idx);
        Assert.AreEqual(0, arr[0]);
        Assert.AreEqual(arraySize - 1, arr[^1]);

        for (int i = 0; i < arr.Length; i++)
        {
            Assert.AreEqual(i, arr[i]);
        }
    }

    [TestMethod]
    public void IsNullOrEmptyTest()
    {
        int[]? arrNull = null;
        Assert.IsTrue(arrNull.IsNullOrEmpty());
        int[] arr = new int[10];
        Assert.IsFalse(arr.IsNullOrEmpty());
        int[] arrFull =
        {
            1,
            2,
            3
        };
        Assert.IsFalse(arrFull.IsNullOrEmpty());
        List<int>? listNull = null;
        Assert.IsTrue(listNull.IsNullOrEmpty());
        // ReSharper disable once CollectionNeverUpdated.Local
        List<int> listEmpty = new();
        Assert.IsTrue(listEmpty.IsNullOrEmpty());
    }

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
    public void ContentEqualsTest()
    {
        int[] arr1 =
        {
            1,
            2,
            3
        };

        int[] arr2 =
        {
            1,
            2,
            3
        };
        Assert.IsTrue(arr1.SequenceEqual(arr2));
        arr2[1] = 1;
        Assert.IsFalse(arr1.SequenceEqual(arr2));

        string s1 = "hello";
        const string s2 = "hello";
        Assert.IsTrue(CollectionHelper.SequenceEqual(s1, s2));

        s1 = "hallo";
        Assert.IsFalse(CollectionHelper.SequenceEqual(s1, s2));
    }

    [TestMethod]
    public void ForEachByRangeFuncTest()
    {
        char[] arr = "hello".ToCharArray();

        arr = arr.ForRanges((..2, _ => 'x'), (2..4, _ => 'y'));
        Assert.AreEqual("xxyyo", arr.ConcatToString());
    }

    [TestMethod]
    public void ForEachByRangeActionTest()
    {
        char[] arr = "hello".ToCharArray();
        List<char> list = new();

        void Action(char c) => list.Add(c);

        arr.ForRanges((2..4, Action), (..2, Action));
        Assert.AreEqual(4, list.Count);
        Assert.IsTrue(list is ['l', 'l', 'h', 'e']);
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
