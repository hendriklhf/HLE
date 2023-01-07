using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CS8794
#pragma warning disable CS0183

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
            Assert.IsTrue(Random.Bool() is true or false);
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
            Assert.IsTrue(Random.Byte() is byte);
        }
    }

    [TestMethod]
    public void SByteTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.SByte() is sbyte);
        }
    }

    [TestMethod]
    public void ShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.Short() is short);
        }
    }

    [TestMethod]
    public void UShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.UShort() is ushort);
        }
    }

    [TestMethod]
    public void IntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.Int() is int);
        }
    }

    [TestMethod]
    public void UIntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.UInt() is uint);
        }
    }

    [TestMethod]
    public void LongTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.Long() is long);
        }
    }

    [TestMethod]
    public void FloatTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.Float() is float);
        }
    }

    [TestMethod]
    public void DoubleTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.Double() is double);
        }
    }

    [TestMethod]
    public void StrongBoolTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongBool() is bool);
        }
    }

    [TestMethod]
    public void StrongByteTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongByte() is byte);
        }
    }

    [TestMethod]
    public void StrongSByteTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongSByte() is sbyte);
        }
    }

    [TestMethod]
    public void StrongShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongShort() is short);
        }
    }

    [TestMethod]
    public void StrongUShortTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongUShort() is ushort);
        }
    }

    [TestMethod]
    public void StrongIntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongInt() is int);
        }
    }

    [TestMethod]
    public void StrongUIntTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongUInt() is uint);
        }
    }

    [TestMethod]
    public void StrongLongTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongLong() is long);
        }
    }

    [TestMethod]
    public void StrongULongTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongULong() is ulong);
        }
    }

    [TestMethod]
    public void StrongFloatTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongFloat() is float);
        }
    }

    [TestMethod]
    public void StrongDoubleTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongDouble() is double);
        }
    }

    [TestMethod]
    public void StrongCharTest()
    {
        for (int i = 0; i < 100_000; i++)
        {
            Assert.IsTrue(Random.StrongChar() is char);
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
