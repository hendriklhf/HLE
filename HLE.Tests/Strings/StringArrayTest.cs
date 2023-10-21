using System;
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
