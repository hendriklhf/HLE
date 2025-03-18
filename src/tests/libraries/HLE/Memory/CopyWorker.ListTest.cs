using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;
using Xunit;

namespace HLE.UnitTests.Memory;

public sealed class CopyWorkerTest
{
    [Fact]
    public void CopyListToListWithoutOffsetTest()
    {
        const int SourceLength = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        List<int> destination = [];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.Equal(SourceLength, destination.Count);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(CollectionsMarshal.AsSpan(destination)));
    }

    [Fact]
    public void CopyListToListWithOffsetTest()
    {
        const int SourceLength = 50;
        const int Offset = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        List<int> destination = [];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination, Offset);
        Assert.Equal(SourceLength + Offset, destination.Count);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(CollectionsMarshal.AsSpan(destination)[Offset..]));
    }

    [Fact]
    public void CopyListToArrayWithoutOffsetTest()
    {
        const int SourceLength = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        int[] destination = new int[SourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(destination));
    }

    [Fact]
    public void CopyListToArrayWithOffsetTest()
    {
        const int SourceLength = 50;
        const int Offset = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        int[] destination = new int[Offset + SourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination, Offset);
        Assert.True(CollectionsMarshal.AsSpan(source).SequenceEqual(destination.AsSpan(Offset)));
    }

    [Fact]
    public void CopyListToMemoryTest()
    {
        const int SourceLength = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        Memory<int> destination = new int[SourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(destination.Span.SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [Fact]
    public void CopyListToSpanTest()
    {
        const int SourceLength = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        Span<int> destination = stackalloc int[50];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(destination.SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [Fact]
    public void CopyListToReferenceTest()
    {
        const int SourceLength = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        Span<int> destinationSpan = stackalloc int[50];
        ref int destination = ref MemoryMarshal.GetReference(destinationSpan);
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(ref destination);
        Assert.True(MemoryMarshal.CreateSpan(ref destination, SourceLength).SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [Fact]
    public unsafe void CopyListToPointerTest()
    {
        const int SourceLength = 50;

        List<int> source = Enumerable.Range(0, SourceLength).ToList();
        Span<int> destinationSpan = stackalloc int[50];
        int* destination = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destinationSpan));
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.True(new Span<int>(destination, SourceLength).SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }
}
