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
            Assert.IsTrue(c >= 32 && c <= 126);
        }
    }

    [TestMethod]
    public void BoolTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsNotNull(Random.Bool());
        }
    }

    [TestMethod]
    public void StringTest()
    {
        const byte strLength = 100;
        for (int i = 0; i < 100_000; i++)
        {
            string s = Random.String(strLength);
            Assert.AreEqual(strLength, s.Length);
            Assert.IsTrue(s.All(c => 32 <= c && c <= 126));
        }
    }
}
