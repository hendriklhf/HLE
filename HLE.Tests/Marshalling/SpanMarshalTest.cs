using System;
using HLE.Collections;
using HLE.Marshalling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Marshalling;

[TestClass]
public class SpanMarshalTest
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
