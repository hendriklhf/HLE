using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class RandomTest
{
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    private const int _loopIterations = 100_000;

    [TestMethod]
    public void CharTest()
    {
        const char min = (char)32;
        const char max = (char)127;
        for (int i = 0; i < _loopIterations; i++)
        {
            char c = Random.Shared.NextChar(min, max);
            Assert.IsTrue(c is >= min and < max);
        }
    }

    [TestMethod]
    public void NextStringTest()
    {
        const char min = (char)32;
        const char max = (char)127;
        const int strLength = 255;
        for (int i = 0; i < _loopIterations; i++)
        {
            string str = Random.Shared.NextString(strLength, min, max);
            Assert.AreEqual(strLength, str.Length);
            Assert.IsTrue(str.All(static c => c is >= min and < max));
        }
    }

    [TestMethod]
    public void NextString_FromChars_Test()
    {
        const int strLength = 255;
        const string chars = "hello";
        for (int i = 0; i < _loopIterations; i++)
        {
            string s = Random.Shared.NextString(strLength, chars);
            Assert.AreEqual(strLength, s.Length);
            Assert.IsTrue(s.All(static c => chars.Contains(c)));
        }
    }

    [TestMethod]
    public void ByteTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(Random.Shared.NextUInt8(20, 150) is >= 20 and < 150);
        }
    }

    [TestMethod]
    public void SByteTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(Random.Shared.NextInt8(-50, 120) is >= -50 and < 120);
        }
    }

    [TestMethod]
    public void ShortTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(Random.Shared.NextInt16(-50, 20200) is >= -50 and < 20200);
        }
    }

    [TestMethod]
    public void UShortTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(Random.Shared.NextUInt16(1000, 55555) is >= 1000 and < 55555);
        }
    }

    [TestMethod]
    public void UIntTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(Random.Shared.NextUInt32(100_000, 4_000_000) is >= 100_000 and < 4_000_000);
        }
    }

    [TestMethod]
    public void StrongByteTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetUInt8(55, 220) is >= 55 and < 220);
        }
    }

    [TestMethod]
    public void StrongSByteTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetInt8(-120, 5) is >= -120 and < 5);
        }
    }

    [TestMethod]
    public void StrongShortTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetInt16(-20_000, 20000) is >= -20_000 and < 20_000);
        }
    }

    [TestMethod]
    public void StrongUShortTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetUInt16(20_000, 60_000) is >= 20_000 and < 60_000);
        }
    }

    [TestMethod]
    public void StrongIntTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetInt32(-2_000_000, 1000) is >= -2_000_000 and < 1000);
        }
    }

    [TestMethod]
    public void StrongUIntTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetUInt32(20_000_000) is >= 20_000_000 and < uint.MaxValue);
        }
    }

    [TestMethod]
    public void StrongLongTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            long value = _rng.GetInt64(-500, 500);
            Assert.IsTrue(value is >= -500 and < 500);
        }
    }

    [TestMethod]
    public void StrongULongTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetUInt64(0, 50) < 50);
        }
    }

    [TestMethod]
    public void StrongCharTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            Assert.IsTrue(_rng.GetChar((char)32, (char)127) is >= (char)32 and < (char)127);
        }
    }

    [TestMethod]
    public void StrongStringTest()
    {
        for (int i = 0; i < _loopIterations; i++)
        {
            string str = _rng.GetString(50);
            Assert.AreEqual(50, str.Length);
        }
    }
}
