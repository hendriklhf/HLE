using System;
using HLE.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Memory;

[TestClass]
public class MemoryHelperTest
{
    [TestMethod]
    public void AsMutableSpanTest()
    {
        ReadOnlySpan<char> str = "hello";
        Span<char> span = str.AsMutableSpan();
        Assert.IsTrue(span.SequenceEqual(str));
    }

    [TestMethod]
    public void AsMutableMemory()
    {
        ReadOnlyMemory<char> str = "hello".AsMemory();
        Memory<char> memory = str.AsMutableMemory();
        Assert.IsTrue(memory.Span is "hello");
    }

    [TestMethod]
    public void AsMemoryDangerousTest()
    {
        Span<char> span = "hello".ToCharArray();
        Memory<char> memory = span.AsMemoryDangerous();
        Assert.IsTrue(memory.Span is "hello");
    }

    [TestMethod]
    public unsafe void GetRawDataPointerTest()
    {
        const string str = "hello";
        int* rawData = (int*)MemoryHelper.GetRawDataPointer(str);
        rawData += 2;
        Assert.AreEqual(str.Length, *rawData);
        char* chars = (char*)++rawData;
        ReadOnlySpan<char> span = new(chars, str.Length);
        Assert.IsTrue(span is str);
    }
}
