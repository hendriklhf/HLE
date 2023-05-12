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
    public unsafe void GetRawDataPointer_GetReferenceFromRawDataPointer_Test()
    {
        const string hello = "hello";
        nuint stringToRawPointer = MemoryHelper.GetRawDataPointer(hello);
        int* lengthPointer = (int*)(stringToRawPointer + (nuint)sizeof(nuint));
        Assert.AreEqual(hello.Length, *lengthPointer);
        char* chars = (char*)(stringToRawPointer + (nuint)sizeof(nuint) + sizeof(int));
        ReadOnlySpan<char> span = new(chars, hello.Length);
        Assert.IsTrue(span is hello);

        string? stringFromRawPointer = MemoryHelper.GetReferenceFromRawDataPointer<string>(stringToRawPointer);
        Assert.IsTrue(stringFromRawPointer is hello);
    }
}
