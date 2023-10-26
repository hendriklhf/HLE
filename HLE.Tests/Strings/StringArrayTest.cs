using System;
using System.Collections.Generic;
using HLE.Collections;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class StringArrayTest
{
    [TestMethod]
    public void Indexer_Int_Test()
    {
        StringArray array = new(4)
        {
            [0] = "hello",
            [1] = "abc",
            [2] = "xd",
            [3] = "555"
        };

        Assert.IsTrue(array is ["hello", "abc", "xd", "555"]);

        array[1] = "////";
        array[2] = string.Empty;

        Assert.IsTrue(array is ["hello", "////", "", "555"]);
    }

    [TestMethod]
    public void Indexer_Int_Throws_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            _ = new StringArray(5)
            {
                [-1] = ""
            };
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            _ = new StringArray(5)
            {
                [5] = ""
            };
        });
    }

    [TestMethod]
    public void Indexer_Index_Test()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        Assert.AreEqual("xd", array[^1]);
    }

    [TestMethod]
    public void Indexer_Index_Throws_Test() =>
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            _ = new StringArray(5)
            {
                [^6] = ""
            };
        });

    [TestMethod]
    public void Indexer_Range_Test()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        ReadOnlySpan<string> span = array[..2];
        Assert.IsTrue(span is ["hello", "abc"]);
    }

    [TestMethod]
    public void Indexer_Range_Throws_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array[..4];
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array[4..];
        });
    }

    [TestMethod]
    public void Length_Test()
    {
        StringArray array = new(5);
        Assert.AreEqual(5, array.Length);

        array = new(256);
        Assert.AreEqual(256, array.Length);

        array = new(ushort.MaxValue);
        Assert.AreEqual(ushort.MaxValue, array.Length);
    }

    [TestMethod]
    public void ICountable_Count_Test()
    {
        ICountable countable = new StringArray(16);
        Assert.AreEqual(16, countable.Count);
    }

    [TestMethod]
    public void ICollection_Count_Test()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        ICollection<string> countable = new StringArray(16);
        Assert.AreEqual(16, countable.Count);
    }

    [TestMethod]
    public void IReadOnlyCollection_Count_Test()
    {
        IReadOnlyCollection<string> countable = new StringArray(16);
        Assert.AreEqual(16, countable.Count);
    }

    [TestMethod]
    public void Initialization_List_Test()
    {
        List<string> list = ["hello", "abc", "xd"];
        StringArray array = new(list);
        Assert.IsTrue(array is ["hello", "abc", "xd"]);
    }

    [TestMethod]
    public void Initialization_Array_Test()
    {
        string[] array = ["hello", "abc", "xd"];
        StringArray stringArray = new(array);
        Assert.IsTrue(stringArray is ["hello", "abc", "xd"]);
    }

    [TestMethod]
    public void Initialization_Span_Test()
    {
        Span<string> span = ["hello", "abc", "xd"];
        StringArray array = new(span);
        Assert.IsTrue(array is ["hello", "abc", "xd"]);
    }

    [TestMethod]
    public void Initialization_ReadOnlySpan_Test()
    {
        ReadOnlySpan<string> span = ["hello", "abc", "xd"];
        StringArray array = new(span);
        Assert.IsTrue(array is ["hello", "abc", "xd"]);
    }

    [TestMethod]
    public void Initialization_IEnumerable_Test()
    {
        IEnumerable<string> enumerable = ["hello", "abc", "xd"];
        StringArray array = new(enumerable);
        Assert.IsTrue(array is ["hello", "abc", "xd"]);
    }

    [TestMethod]
    public void GetStringLengthTest()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        Assert.AreEqual("hello".Length, array.GetStringLength(0));
        Assert.AreEqual("abc".Length, array.GetStringLength(1));
        Assert.AreEqual("xd".Length, array.GetStringLength(2));

        array[1] = "////";
        Assert.AreEqual("////".Length, array.GetStringLength(1));
    }

    [TestMethod]
    public void GetStringLength_Throws_Test()
    {
        Assert.ThrowsException<IndexOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array.GetStringLength(-1);
        });

        Assert.ThrowsException<IndexOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array.GetStringLength(3);
        });
    }

    [TestMethod]
    public void GetStringLength_Loop_Test()
    {
        StringArray stringArray = new(16);
        Span<string?> array = new string[16];

        for (int i = 0; i < 1024; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32));
            int index = Random.Shared.Next(16);
            array[index] = str;
            stringArray[index] = str;

            for (int j = 0; j < 16; j++)
            {
                Assert.AreEqual(array[j]?.Length ?? 0, stringArray.GetStringLength(j));
            }
        }
    }

    [TestMethod]
    public void GetCharsTest()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        Assert.IsTrue(array.GetChars(0) is "hello");
        Assert.IsTrue(array.GetChars(1) is "abc");
        Assert.IsTrue(array.GetChars(2) is "xd");

        array[1] = "////";
        Assert.IsTrue(array.GetChars(1) is "////");
    }

    [TestMethod]
    public void GetChars_Loop_Test()
    {
        StringArray stringArray = new(16);
        Span<string?> array = new string[16];

        for (int i = 0; i < 1024; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32), StringConstants.AlphaNumerics);
            int index = Random.Shared.Next(16);
            array[index] = str;
            stringArray[index] = str;

            Console.WriteLine($"----- Actual vs Expected ----- Write at: {index}");
            for (int j = 0; j < 16; j++)
            {
                ReadOnlySpan<char> actualChars = stringArray.GetChars(j);
                string? expectedChars = array[j];
                Console.WriteLine($"({j}) {actualChars} == {expectedChars}");
                Assert.IsTrue(actualChars.SequenceEqual(expectedChars));
            }

        }
    }

    [TestMethod]
    public void AsSpanTest()
    {
        StringArray stringArray = new(16);
        Assert.AreEqual(16, stringArray.AsSpan().Length);
        Span<string> array = new string[16];

        for (int i = 0; i < 1024; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32));
            int index = Random.Shared.Next(16);
            array[index] = str;
            stringArray[index] = str;

            Assert.IsTrue(array.SequenceEqual(stringArray.AsSpan()));
        }
    }

    [TestMethod]
    public void MoveStringTest_LeftToRight()
    {
        StringArray array = new(5)
        {
            [0] = "aaa",
            [1] = "bb",
            [2] = "cccc",
            [3] = "d",
            [4] = "eeeee"
        };
        array.MoveString(1, 3);

        Assert.IsTrue(array._strings is ["aaa", "cccc", "d", "bb", "eeeee"]);
        Assert.IsTrue(array._stringLengths is [3, 4, 1, 2, 5]);
        Assert.IsTrue(array._stringStarts is [0, 3, 7, 8, 10]);
        Assert.IsTrue(array._stringChars.AsSpan().StartsWith("aaaccccdbbeeeee\0"));

        Assert.IsTrue(array.GetChars(0) is "aaa");
        Assert.IsTrue(array.GetChars(1) is "cccc");
        Assert.IsTrue(array.GetChars(2) is "d");
        Assert.IsTrue(array.GetChars(3) is "bb");
        Assert.IsTrue(array.GetChars(4) is "eeeee");
    }

    [TestMethod]
    public void MoveStringTest_RightToLeft()
    {
        StringArray array = new(5)
        {
            [0] = "aaa",
            [1] = "bb",
            [2] = "cccc",
            [3] = "d",
            [4] = "eeeee"
        };
        array.MoveString(3, 1);

        Assert.IsTrue(array._strings is ["aaa", "d", "bb", "cccc", "eeeee"]);
        Assert.IsTrue(array._stringLengths is [3, 1, 2, 4, 5]);
        Assert.IsTrue(array._stringStarts is [0, 3, 4, 6, 10]);
        Assert.IsTrue(array._stringChars.AsSpan().StartsWith("aaadbbcccceeeee\0"));

        Assert.IsTrue(array.GetChars(0) is "aaa");
        Assert.IsTrue(array.GetChars(1) is "d");
        Assert.IsTrue(array.GetChars(2) is "bb");
        Assert.IsTrue(array.GetChars(3) is "cccc");
        Assert.IsTrue(array.GetChars(4) is "eeeee");
    }
}
