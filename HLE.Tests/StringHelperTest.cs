using System;
using System.Linq;
using HLE.Strings;
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
        ReadOnlyMemory<char>[] part = _str.Part(charCount).ToArray();
        Assert.AreEqual(8, part.Length);
        Assert.IsTrue(part[..^1].All(p => p.Length == charCount));
        Assert.IsTrue(part[^1].Length <= charCount);
    }

    [TestMethod]
    public void PartTestOnWhitespace()
    {
        const byte charCount = 60;
        ReadOnlyMemory<char>[] part = _str.Part(charCount, ' ').ToArray();
        Assert.AreEqual(4, part.Length);
        Assert.IsTrue(part.All(p =>
        {
            if (p.Span.Contains(' '))
            {
                return new string(p.Span).Split()[0].Length <= charCount;
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
        Range[] ranges = str.GetRangesOfSplit();
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
        Assert.IsTrue(ranges is [..]);
        Assert.AreEqual(s, str[ranges[0]].ToString());
    }

    [TestMethod]
    public void CharCountTest()
    {
        const string str = "hello";
        int count = str.CharCount('l');
        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void RegexEscapeTest()
    {
        const string str = "\\*+?|{[()^$. awdawdawdawd";
        Span<char> buffer = stackalloc char[str.Length << 1];
        int length = StringHelper.RegexEscape(str, buffer);
        Assert.AreEqual(@"\\\*\+\?\|\{\[\(\)\^\$\.\sawdawdawdawd", new(buffer[..length]));
    }

    [TestMethod]
    public void JoinTest()
    {
        string[] strings = { "h", "e", "l", "l", "o" };
        Span<char> result = stackalloc char[30];
        int length = StringHelper.Join(strings, ' ', result);
        string str = new(result[..length]);
        Assert.AreEqual("h e l l o", str);
    }

    [TestMethod]
    public void ConcatTest()
    {
        string[] strings = { "h", "e", "l", "l", "o" };
        Span<char> result = stackalloc char[30];
        int length = StringHelper.Concat(strings, result);
        string str = new(result[..length]);
        Assert.AreEqual("hello", str);
    }
}
