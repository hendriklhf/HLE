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
}
