using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class StringHelperTest
{
    private readonly string _str = $"{Str(50)} {Str(10)} {Str(25)} {Str(5)} {Str(100)} {Str(30)}";

    private static string Str(int count) => new('*', count);

    [TestMethod]
    public void SplitTest()
    {
        string[] split = _str.Split(30).ToArray();
        Assert.AreEqual(8, split.Length);
    }

    [TestMethod]
    public void SplitTestOnWhitespace()
    {
        string[] split = _str.Split(60, true).ToArray();
        Assert.AreEqual(5, split.Length);
    }

    [TestMethod]
    public void IndicesOfTest()
    {
        const string str = "test string";
        int[] indices = str.IndicesOf('s');
        Assert.IsTrue(indices is [2, 5]);
    }

    [TestMethod]
    public void InsertKDotsTest()
    {
        const int number1 = 1234567890;
        Assert.AreEqual("1.234.567.890", number1.InsertKDots());
        const int number2 = 123456789;
        Assert.AreEqual("123.456.789", number2.InsertKDots());
        const int number3 = 12345678;
        Assert.AreEqual("12.345.678", number3.InsertKDots());
        const int number4 = 1234567;
        Assert.AreEqual("1.234.567", number4.InsertKDots());
        const int number5 = 123;
        Assert.AreEqual("123", number5.InsertKDots());
    }

    [TestMethod]
    public void GetRangesOfSplitTest()
    {
        ReadOnlySpan<char> str = "hello world  test";
        var ranges = str.GetRangesOfSplit();
        Assert.IsTrue(ranges is [_, _, _, _]);
        Assert.AreEqual("hello", str[ranges[0]].ToString());
        Assert.AreEqual("world", str[ranges[1]].ToString());
        Assert.AreEqual(string.Empty, str[ranges[2]].ToString());
        Assert.AreEqual("test", str[ranges[3]].ToString());
    }
}
