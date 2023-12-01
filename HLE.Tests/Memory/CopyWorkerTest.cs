using System;
using System.Runtime.InteropServices;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed partial class CopyWorkerTest
{
    [Fact]
    public unsafe void MemmoveTest()
    {
        const int SourceLength = 50;

        Span<int> source = stackalloc int[SourceLength];
        SpanHelpers.FillAscending(source);

        Span<int> destination = stackalloc int[SourceLength];

        CopyWorker<int>.s_memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (nuint)source.Length);
        Assert.True(destination.SequenceEqual(source));
    }
}
