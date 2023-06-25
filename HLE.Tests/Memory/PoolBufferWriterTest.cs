using System;
using HLE.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Memory;

[TestClass]
public class PoolBufferWriterTest
{
    [TestMethod]
    public void WriteToSpanTest()
    {
        using PoolBufferWriter<char> writer = new();
        "hello".CopyTo(writer.GetSpan(5));
        writer.Advance(5);
        Assert.AreEqual(5, writer.Count);
        Assert.IsTrue(writer.WrittenSpan is "hello");
    }

    [TestMethod]
    public void WriteToMemoryTest()
    {
        using PoolBufferWriter<char> writer = new();
        "hello".CopyTo(writer.GetMemory(5).Span);
        writer.Advance(5);
        Assert.AreEqual(5, writer.Count);
        Assert.IsTrue(writer.WrittenSpan is "hello");
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

        Assert.AreEqual(5000, writer.Count);
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
        Assert.IsTrue(writer.Count == 0);
        Assert.IsTrue(writer.WrittenSpan is "");
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

            string str = Random.Shared.NextString(Random.Shared.Next(25, 2000), (char)32, (char)126);
            str.CopyTo(writer.GetSpan(str.Length));
            writer.Advance(str.Length);
        }

        Assert.IsTrue(writer.Count > 0);
    }
}
