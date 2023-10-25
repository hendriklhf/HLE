using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Memory;

[TestClass]
public partial class CopyWorkerTest
{
    [TestMethod]
    public unsafe void MemmoveTest()
    {
        const int sourceLength = 50;

        Span<int> source = stackalloc int[sourceLength];
        source.FillAscending();
        Span<int> destination = stackalloc int[sourceLength];
        Debug.Assert(source.Length == destination.Length);

        CopyWorker<int>.s_memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (nuint)source.Length);
        Assert.IsTrue(destination.SequenceEqual(source));
    }
}
