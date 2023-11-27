using System;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Memory;
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
        Assert.True(str == span);
    }

    [Fact]
    public void AsMutableMemoryTest()
    {
        ReadOnlyMemory<char> str = "hello".AsMemory();
        Memory<char> memory = SpanMarshal.AsMutableMemory(str);
        Assert.True(memory.Span is "hello");
        Assert.True(str.Span == memory.Span);
    }

    [Fact]
    public void AsMemoryTest()
    {
        Span<char> span = "hello".ToCharArray();
        Memory<char> memory = SpanMarshal.AsMemory(span);
        Assert.True(memory.Span is "hello");
        Assert.True(span == memory.Span);
    }

    [Fact]
    public void UnsafeSliceTest()
    {
        Span<int> span = stackalloc int[50];
        SpanHelpers.FillAscending(span);
        Span<int> slice = span.SliceUnsafe(5, 10);
        Assert.Equal(10, slice.Length);
        Assert.True(slice[0] == 5 && slice[^1] == 14);

        slice = span.SliceUnsafe(5..15);
        Assert.Equal(10, slice.Length);
        Assert.True(slice[0] == 5 && slice[^1] == 14);
    }
}
