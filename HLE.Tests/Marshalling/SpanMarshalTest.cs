using System;
using HLE.Collections;
using HLE.Marshalling;
using Xunit;

namespace HLE.Tests.Marshalling;

public sealed class SpanMarshalTest
{
    [Fact]
    public void AsMutableSpanTest()
    {
        ReadOnlySpan<char> str = "hello";
        Span<char> span = SpanMarshal.AsMutableSpan(str);
        Assert.True(span.SequenceEqual(str));
    }

    [Fact]
    public void AsMutableMemory()
    {
        ReadOnlyMemory<char> str = "hello".AsMemory();
        Memory<char> memory = SpanMarshal.AsMutableMemory(str);
        Assert.True(memory.Span is "hello");
    }

    [Fact]
    public void AsMemoryDangerousTest()
    {
        Span<char> span = "hello".ToCharArray();
        Memory<char> memory = SpanMarshal.AsMemoryUnsafe(span);
        Assert.True(memory.Span is "hello");
    }

    [Fact]
    public void UnsafeSliceTest()
    {
        Span<int> span = stackalloc int[50];
        span.FillAscending();
        Span<int> slice = span.SliceUnsafe(5, 10);
        Assert.Equal(10, slice.Length);
        Assert.True(slice[0] == 5 && slice[^1] == 14);

        slice = span.SliceUnsafe(5..15);
        Assert.Equal(10, slice.Length);
        Assert.True(slice[0] == 5 && slice[^1] == 14);
    }
}
