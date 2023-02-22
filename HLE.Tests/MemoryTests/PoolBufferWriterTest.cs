using System;
using HLE.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.MemoryTests;

[TestClass]
public class PoolBufferWriterTest
{
    [TestMethod]
    public void WriteToSpanTest()
    {
        using PoolBufferWriter<char> writer = new();
        "hello".CopyTo(writer.GetSpan(5));
        writer.Advance(5);
        Assert.AreEqual(5, writer.Length);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello"));
    }

    [TestMethod]
    public void WriteToMemoryTest()
    {
        using PoolBufferWriter<char> writer = new();
        "hello".CopyTo(writer.GetMemory(5).Span);
        writer.Advance(5);
        Assert.AreEqual(5, writer.Length);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual("hello"));
    }

    [TestMethod]
    public void LoopWritingTest()
    {
        using PoolBufferWriter<char> writer = new();
        for (int i = 0; i < 1000; i++)
        {
            "hello".CopyTo(writer.GetSpan(5));
            writer.Advance(5);
        }

        Assert.AreEqual(5000, writer.Length);
        Assert.IsTrue(writer.WrittenSpan.StartsWith("hello"));
        Assert.IsTrue(writer.WrittenSpan.EndsWith("hello"));
    }

    [TestMethod]
    public void ClearTest()
    {
        using PoolBufferWriter<char> writer = new();
        for (int i = 0; i < 1000; i++)
        {
            "hello".CopyTo(writer.GetSpan(5));
            writer.Advance(5);
        }

        writer.Clear();
        Assert.IsTrue(writer.Length == 0);
        Assert.IsTrue(writer.WrittenSpan.SequenceEqual(""));
    }

    [TestMethod]
    public void WritingAndClearingTest()
    {
        using PoolBufferWriter<char> writer = new();
        for (int i = 0; i < 100_000; i++)
        {
            if (i > 0 && i % 100 == 0)
            {
                writer.Clear();
            }

            string str = Random.String(Random.Int(25, 2000));
            str.CopyTo(writer.GetSpan(str.Length));
            writer.Advance(str.Length);
        }

        Assert.IsTrue(writer.Length > 0);
    }
}
