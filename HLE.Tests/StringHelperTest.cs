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
    public void PartTest()
    {
        const byte charCount = 30;
        string[] part = _str.Part(charCount).ToArray();
        Assert.AreEqual(8, part.Length);
        Assert.IsTrue(part[..^1].All(p => p.Length == charCount));
        Assert.IsTrue(part[^1].Length <= charCount);
    }

    [TestMethod]
    public void PartTestOnWhitespace()
    {
        const byte charCount = 60;
        string[] part = _str.Part(charCount, ' ').ToArray();
        Assert.AreEqual(4, part.Length);
        Assert.IsTrue(part.All(p =>
        {
            if (p.Contains(' '))
            {
                return p.Split()[0].Length <= charCount;
            }

            return true;
        }));
    }

    [TestMethod]
    public void IndicesOfTest()
    {
        const string str = "test string";
        int[] indices = str.IndicesOf('s');
        Assert.IsTrue(indices is [2, 5]);
    }

    [TestMethod]
    public void IndicesOfSpan_CharSeparator_Test()
    {
        ReadOnlySpan<char> str = "hello world  test";
        var indices = str.IndicesOf(' ');
        Assert.IsTrue(indices is [5, 11, 12]);
    }

    [TestMethod]
    public void GetRangesOfSplit_CharSeparator_Test()
    {
        ReadOnlySpan<char> str = "hello world  test";
        var ranges = str.GetRangesOfSplit();
        Assert.IsTrue(ranges is [_, _, _, _]);
        Assert.AreEqual("hello", str[ranges[0]].ToString());
        Assert.AreEqual("world", str[ranges[1]].ToString());
        Assert.AreEqual(string.Empty, str[ranges[2]].ToString());
        Assert.AreEqual("test", str[ranges[3]].ToString());

        const string s = "this is a message";
        str = s;
        ranges = str.GetRangesOfSplit('\n');
        Assert.IsTrue(ranges is [..]);
        Assert.AreEqual(s, str[ranges[0]].ToString());

        str = string.Empty;
        ranges = str.GetRangesOfSplit();
        Assert.IsTrue(ranges is [..]);
    }

    [TestMethod]
    public void IndicesOfSpan_StringSeparator_Test()
    {
        ReadOnlySpan<char> str = "hello    world  test";
        var indices = str.IndicesOf("  ");
        Assert.IsTrue(indices is [5, 7, 14]);
    }

    [TestMethod]
    public void GetRangesOfSplit_StringSeparator_Test()
    {
        ReadOnlySpan<char> str = "hello    world  test";
        var ranges = str.GetRangesOfSplit("  ");
        Assert.IsTrue(ranges is [_, _, _, _]);
        Assert.AreEqual("hello", str[ranges[0]].ToString());
        Assert.AreEqual(string.Empty, str[ranges[1]].ToString());
        Assert.AreEqual("world", str[ranges[2]].ToString());
        Assert.AreEqual("test", str[ranges[3]].ToString());

        const string s = "this is a message";
        str = s;
        ranges = str.GetRangesOfSplit("\r\n");
        Assert.IsTrue(ranges is [_]);
        Assert.AreEqual(s, str[ranges[0]].ToString());
    }

    [TestMethod]
    public void AsSpanTest()
    {
        const string str = "hello";
        Span<char> span = str.AsMutableSpan();
        span[0] = 'H';
        Assert.IsTrue(char.IsUpper(str[0]));
        Assert.AreEqual("Hello", str);
    }

    [TestMethod]
    public void ToLowerTest()
    {
        const string str = "HELLO";
        StringHelper.ToLower(str);
        Assert.AreEqual("hello", str);
    }

    [TestMethod]
    public void ToUpperTest()
    {
        const string str = "hello";
        StringHelper.ToUpper(str);
        Assert.AreEqual("HELLO", str);
    }

    [TestMethod]
    public void CharCountTest()
    {
        const string str = "hello";
        int count = str.CharCount('l');
        Assert.AreEqual(2, count);
    }
}
