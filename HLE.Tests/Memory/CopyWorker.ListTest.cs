using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed partial class CopyWorkerTest
{
    [Fact]
    public void CopyListToListWithoutOffsetTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        List<int> destination = [];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.Equal(sourceLength, destination.Count);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(CollectionsMarshal.AsSpan(destination)));
    }

    [Fact]
    public void CopyListToListWithOffsetTest()
    {
        const int sourceLength = 50;
        const int offset = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        List<int> destination = [];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination, offset);
        Assert.Equal(sourceLength + offset, destination.Count);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(CollectionsMarshal.AsSpan(destination)[offset..]));
    }

    [Fact]
    public void CopyListToArrayWithoutOffsetTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        int[] destination = new int[sourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(destination));
    }

    [Fact]
    public void CopyListToArrayWithOffsetTest()
    {
        const int sourceLength = 50;
        const int offset = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        int[] destination = new int[offset + sourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination, offset);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(destination.AsSpan(offset)));
    }

    [Fact]
    public void CopyListToMemoryTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Memory<int> destination = new int[sourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(destination.Span.SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [Fact]
    public void CopyListToSpanTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Span<int> destination = stackalloc int[50];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(destination.SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [Fact]
    public void CopyListToReferenceTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Span<int> destinationSpan = stackalloc int[50];
        ref int destination = ref MemoryMarshal.GetReference(destinationSpan);
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(ref destination);
        Assert.True(MemoryMarshal.CreateSpan(ref destination, sourceLength).SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [Fact]
    public unsafe void CopyListToPointerTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Span<int> destinationSpan = stackalloc int[50];
        int* destination = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destinationSpan));
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(new Span<int>(destination, sourceLength).SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }
}
