using System;
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
#pragma warning disable CS8794
            Assert.IsTrue(Random.Bool() is true or false);
#pragma warning restore CS8794
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

    [TestMethod]
    public void ByteTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.Byte() is byte);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void SByteTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.SByte() is sbyte);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void ShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.Short() is short);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void UShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.UShort() is ushort);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void IntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.Int() is int);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void UIntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.UInt() is uint);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void LongTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.Long() is long);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void FloatTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.Float() is float);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void DoubleTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.Double() is double);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongBoolTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongBool() is bool);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongByteTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongByte() is byte);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongSByteTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongSByte() is sbyte);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongShort() is short);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongUShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongUShort() is ushort);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongIntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongInt() is int);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongUIntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongUInt() is uint);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongLongTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongLong() is long);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongULongTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongULong() is ulong);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongInt128Test()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongInt128() is Int128);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongUInt128Test()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongUInt128() is UInt128);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongFloatTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongFloat() is float);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongDoubleTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongDouble() is double);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongCharTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
#pragma warning disable CS0183
            Assert.IsTrue(Random.StrongChar() is char);
#pragma warning restore CS0183
        }
    }

    [TestMethod]
    public void StrongStringTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            string str = Random.StrongString(50);
            Assert.AreEqual(50, str.Length);
        }
    }
}
