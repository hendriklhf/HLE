using System;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public class PooledStringTest
{
    [TestMethod]
    public void CreateFromLengthTest()
    {
        for (int length = 0; length <= 10_000; length *= 10)
        {
            using PooledString pooledString = PooledString.Create(length);

            Assert.AreEqual(length, pooledString.Length);
            Assert.AreEqual(length, pooledString.AsString().Length);
            Assert.AreEqual(length, pooledString.AsSpan().Length);

            Assert.IsTrue(pooledString.AsSpan().SequenceEqual(pooledString.AsString()));

            if (length == 0)
            {
                length = 1;
            }
        }
    }

    [TestMethod]
    public void CreateFromSpanTest()
    {
        for (int length = 0; length <= 10_000; length *= 10)
        {
            ReadOnlySpan<char> span = Random.Shared.NextString(length);
            using PooledString pooledString = PooledString.Create(span);

            Assert.AreEqual(span.Length, pooledString.Length);
            Assert.AreEqual(span.Length, pooledString.AsString().Length);
            Assert.AreEqual(span.Length, pooledString.AsSpan().Length);

            Assert.IsTrue(pooledString.AsSpan().SequenceEqual(pooledString.AsString()));
            Assert.IsTrue(span.SequenceEqual(pooledString.AsString()));

            if (length == 0)
            {
                length = 1;
            }
        }
    }
}
