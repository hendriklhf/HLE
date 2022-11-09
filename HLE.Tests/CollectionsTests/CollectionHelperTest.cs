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
        string[] arr =
        {
            "1",
            "2",
            "3",
            "4",
            "5"
        };
        for (int i = 0; i <= 50; i++)
        {
            Assert.IsTrue(arr.Contains(arr.Random()));
        }
    }

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

        for (int i = 0; i < arr.Length; i++)
        {
            switch (i)
            {
                case 0:
                    Assert.AreEqual(default, arr[i]);
                    break;
                default:
                    Assert.AreNotEqual(default, arr[i]);
                    break;
            }
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
    public void SwapTest()
    {
        int[] arr =
        {
            1,
            2,
            3
        };

        arr = arr.Swap(0, 2).ToArray();
        Assert.AreEqual(1, arr[2]);
        Assert.AreEqual(3, arr[0]);
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
        arr = arr.Replace(i => i == 2, 4).ToArray();
        Assert.AreEqual(5, arr.Count(i => i == 4));
    }

    [TestMethod]
    public void SelectManyTest()
    {
        int[][] arrArr =
        {
            new[]
            {
                0,
                1,
                2,
                3,
                4
            },
            new[]
            {
                5,
                6,
                7,
                8,
                9
            },
            new[]
            {
                10,
                11,
                12,
                13,
                14
            }
        };

        int[] arr = arrArr.SelectMany().ToArray();
        Assert.AreEqual(15, arr.Length);
        for (int i = 0; i < arr.Length; i++)
        {
            Assert.AreEqual(i, arr[i]);
        }
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

        var split = arr.Split(".").ToArray();
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
    public void WherePTest()
    {
        static bool Longer3(string s) => s.Length > 3;
        static bool Shorter5(string s) => s.Length < 5;

        string[] arr =
        {
            "aaaaaaaa",
            "aaa",
            "aaaa",
            "aaaaa",
            "a",
            "aaaa",
            "aaaaaaaaa",
            "aaaa"
        };

        arr = arr.WhereP(Longer3, Shorter5).ToArray();
        Assert.AreEqual(3, arr.Length);
        Assert.IsTrue(arr.All(s => s.Length == 4));
    }

    [TestMethod]
    public void RandomWordTest()
    {
        const int stringLength = 50;
        char[] arr = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        string word = arr.RandomString(stringLength);
        Assert.AreEqual(stringLength, word.Length);
        Assert.IsTrue(word.All(c => arr.Contains(c)));
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
        Assert.IsTrue(arr1.ContentEquals(arr2));
        arr2[1] = 1;
        Assert.IsFalse(arr1.ContentEquals(arr2));

        string s1 = "hello";
        const string s2 = "hello";
        Assert.IsTrue(s1.ContentEquals(s2));

        s1 = "hallo";
        Assert.IsFalse(s1.ContentEquals(s2));
    }

    [TestMethod]
    public void ForEachByRangeActionTest()
    {
        char[] arr =
        {
            'h',
            'e',
            'l',
            'l',
            'o'
        };

        arr = arr.ForEachByRange((..2, _ => 'x'), (2..4, _ => 'y')).ToArray();
        Assert.AreEqual("xxyyy", arr.ConcatToString());
    }

    [TestMethod]
    public void RandomizeTest()
    {
        int[] arr = Enumerable.Range(0, 50).Select(_ => Random.Int()).ToArray();
        int[][] arrArr = Enumerable.Range(0, 100_000).Select(_ => arr.Randomize().ToArray()).ToArray();
        Assert.IsTrue(arrArr.Count(a => a.ContentEquals(arr)) <= 1);
    }

    [TestMethod]
    public void RangeEnumeratorTest()
    {
        List<int> items = new();
        foreach (int i in ..100)
        {
            items.Add(i);
        }

        Assert.AreEqual(101, items.Count);
        Assert.AreEqual(0, items[0]);
        Assert.AreEqual(100, items[100]);
    }

    [TestMethod]
    public void IndicesOfTest()
    {
        const string str = "test string";
        int[] indices = str.IndicesOf(c => c is 's').ToArray();
        Assert.IsTrue(indices is [2, 5]);
    }
}
