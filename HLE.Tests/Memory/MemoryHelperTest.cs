using System;
using HLE.Collections;
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
        Memory<char> memory = span.AsMemoryUnsafe();
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
        Assert.AreEqual(typeof(string), stringFromRawPointer.GetType());
    }

    [TestMethod]
    public void AsStringDangerousTest()
    {
        ReadOnlySpan<char> str = "hello";
        string actualString = str.AsStringUnsafe();
        Assert.AreEqual("hello", actualString);
        Assert.AreEqual(typeof(string), actualString.GetType());
    }

    [TestMethod]
    public void UnsafeSliceTest()
    {
        Span<int> span = stackalloc int[50];
        span.FillAscending();
        Span<int> slice = span.SliceUnsafe(5, 10);
        Assert.AreEqual(10, slice.Length);
        Assert.IsTrue(slice[0] == 5 && slice[^1] == 14);

        slice = span.SliceUnsafe(5..15);
        Assert.AreEqual(10, slice.Length);
        Assert.IsTrue(slice[0] == 5 && slice[^1] == 14);
    }
}
