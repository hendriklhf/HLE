using System;
using System.Linq;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class HStringTest
{
    private const string _testString = "Hello World!";

    [DataRow(0)]
    [DataRow(5)]
    [DataRow(11)]
    [TestMethod]
    public void GetIndexerIntTest(int idx)
    {
        HString s = _testString;
        Assert.AreEqual(_testString[idx], s[idx]);
    }

    [DataRow(1, 'h')]
    [DataRow(6, 'w')]
    [DataRow(10, 't')]
    [TestMethod]
    public void SetIndexerIntTest(int idx, char c)
    {
        HString s = _testString;
        s[idx] = c;
        Assert.AreEqual(c, ((string)s)[idx]);
    }

    [TestMethod]
    public void IndexerIndexTest()
    {
        HString s = _testString;
        Assert.AreEqual(_testString[^1], s[^1]);
    }

    [TestMethod]
    public void IndexerGetRangeTest()
    {
        Range[] ranges =
        {
            1..3,
            3..7,
            3..^2
        };
        foreach (Range r in ranges)
        {
            HString s = _testString;
            Assert.AreEqual(_testString[r], new(s[r]));
        }
    }

    [TestMethod]
    public void IndexerSetRangeTest()
    {
        HString str = "hello";
        Range r = 1..3;
        str[r] = "xx";
        Assert.AreEqual("hxxlo", str.ToString());
    }

    [TestMethod]
    public void LengthTest()
    {
        HString s = _testString;
        Assert.AreEqual(_testString.Length, s.Length);
    }

    [TestMethod]
    public void IndecesOfTest()
    {
        HString s = _testString;
        int[] indices = s.IndicesOf(c => c == 'l').ToArray();
        Assert.IsTrue(indices is [2, 3, 9]);
    }

    [TestMethod]
    public void EqualsTest()
    {
        HString s = _testString;
        HString t = _testString;
        Assert.IsTrue(s == _testString);
        Assert.IsTrue(s == t);
    }

    [TestMethod]
    public void EmptyTest()
    {
        Assert.IsTrue(HString.Empty.Length == 0);
        Assert.IsTrue(string.Empty == HString.Empty);
    }
}
