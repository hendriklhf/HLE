using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Memory;

public partial class CopyWorkerTest
{
    [TestMethod]
    public void CopyListToListWithoutOffsetTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        List<int> destination = new();
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.AreEqual(sourceLength, destination.Count);
        Assert.IsTrue(CollectionsMarshal.AsSpan(source).SequenceEqual(CollectionsMarshal.AsSpan(destination)));
    }

    [TestMethod]
    public void CopyListToListWithOffsetTest()
    {
        const int sourceLength = 50;
        const int offset = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        List<int> destination = new();
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination, offset);
        Assert.AreEqual(sourceLength + offset, destination.Count);
        Assert.IsTrue(CollectionsMarshal.AsSpan(source).SequenceEqual(CollectionsMarshal.AsSpan(destination)[offset..]));
    }

    [TestMethod]
    public void CopyListToArrayWithoutOffsetTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        int[] destination = new int[sourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.IsTrue(CollectionsMarshal.AsSpan(source).SequenceEqual(destination));
    }

    [TestMethod]
    public void CopyListToArrayWithOffsetTest()
    {
        const int sourceLength = 50;
        const int offset = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        int[] destination = new int[offset + sourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination, offset);
        Assert.IsTrue(CollectionsMarshal.AsSpan(source).SequenceEqual(destination.AsSpan(offset)));
    }

    [TestMethod]
    public void CopyListToMemoryTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Memory<int> destination = new int[sourceLength];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.IsTrue(destination.Span.SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [TestMethod]
    public void CopyListToSpanTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Span<int> destination = stackalloc int[50];
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.IsTrue(destination.SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [TestMethod]
    public void CopyListToReferenceTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Span<int> destinationSpan = stackalloc int[50];
        ref int destination = ref MemoryMarshal.GetReference(destinationSpan);
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(ref destination);
        Assert.IsTrue(MemoryMarshal.CreateSpan(ref destination, sourceLength).SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }

    [TestMethod]
    public unsafe void CopyListToPointerTest()
    {
        const int sourceLength = 50;

        List<int> source = Enumerable.Range(0, sourceLength).ToList();
        Span<int> destinationSpan = stackalloc int[50];
        int* destination = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(destinationSpan));
        CopyWorker<int> copyWorker = new(source);
        copyWorker.CopyTo(destination);
        Assert.IsTrue(new Span<int>(destination, sourceLength).SequenceEqual(CollectionsMarshal.AsSpan(source)));
    }
}
