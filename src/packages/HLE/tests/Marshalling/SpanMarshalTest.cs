using System;
using HLE.Marshalling;
using HLE.Memory;
using HLE.TestUtilities;

namespace HLE.UnitTests.Marshalling;

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
    public void AsMemory_Empty_Test()
    {
        ReadOnlySpan<char> span = [];
        ReadOnlyMemory<char> memory = SpanMarshal.AsMemory(span);
        Assert.Equal(0, memory.Length);
    }

    [Fact]
    public void AsMemory_Mutable_Empty_Test()
    {
        Span<char> span = [];
        Memory<char> memory = SpanMarshal.AsMemory(span);
        Assert.Equal(0, memory.Length);
    }

    [Fact]
    public void AsMemory_String_Test()
    {
        ReadOnlySpan<char> span = "hello";
        ReadOnlyMemory<char> memory = SpanMarshal.AsMemory(span);
        Assert.True(span == memory.Span);
    }

    [Fact]
    public void AsMemory_CharArray_Test()
    {
        ReadOnlySpan<char> span = "hello".ToCharArray();
        ReadOnlyMemory<char> memory = SpanMarshal.AsMemory(span);
        Assert.True(span == memory.Span);
    }

    [Fact]
    public void AsMemory_IntArray_Test()
    {
        int[] array = [0, 1, 2, 3, 4];
        ReadOnlySpan<int> span = array;
        ReadOnlyMemory<int> memory = SpanMarshal.AsMemory(span);
        Assert.True(span == memory.Span);
    }

    [Fact]
    public unsafe void AsMemory_MemoryManager_Throws_Test() =>
        Assert.Throws<InvalidOperationException>(static () =>
        {
            byte* bytes = stackalloc byte[8];
            NativeMemoryManager<byte> memoryManager = new(bytes, 8);
            Memory<byte> memory = memoryManager.Memory;
            Span<byte> span = memory.Span;
            _ = SpanMarshal.AsMemory(span);
        });

    [Fact]
    public void AsArray_Span_Test()
    {
        int[] array = [0, 1, 2, 3, 4];
        Span<int> span = TestHelpers.NoInline.AsSpan(array);
        int[] a = SpanMarshal.AsArray(span);
        Assert.Same(array, a);
        Assert.True(span.SequenceEqual(array));
        Assert.True(span.SequenceEqual(a));
        Assert.True(array.AsSpan().SequenceEqual(a));
    }

    [Fact]
    public void AsArray_ReadOnlySpan_Test()
    {
        int[] array = [0, 1, 2, 3, 4];
        ReadOnlySpan<int> span = TestHelpers.NoInline.AsReadOnlySpan(array);
        int[] a = SpanMarshal.AsArray(span);
        Assert.Same(array, a);
        Assert.True(span.SequenceEqual(array));
        Assert.True(span.SequenceEqual(a));
        Assert.True(array.AsSpan().SequenceEqual(a));
    }
}
