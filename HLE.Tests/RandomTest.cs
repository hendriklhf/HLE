using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class RandomTest
{
    [TestMethod]
    public void CharTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            char c = Random.Char();
            Assert.IsTrue(c >= 33 && c <= 126);
        }
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void CharTestWithParams()
    {
        const ushort min = 100;
        const ushort max = 1000;
        for (int i = 0; i < 100_000; i++)
        {
            char c = Random.Char(min, max);
            Assert.IsTrue(c >= min && c <= max);
        }
    }

    [TestMethod]
    public void BoolTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS8794
            Assert.IsTrue(Random.Bool() is true or false);
#pragma warning restore CS8794
        }
    }

    [TestMethod]
    public void StringTest()
    {
        const byte strLength = 100;
        const byte min = 33;
        const byte max = 126;
        for (int i = 0; i < 100_000; i++)
        {
            string s = Random.String(strLength);
            Assert.AreEqual(strLength, s.Length);
            Assert.IsTrue(s.All(c => min <= c && c <= max));
        }
    }
}
