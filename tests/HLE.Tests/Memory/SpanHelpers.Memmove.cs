using System;
using System.Runtime.InteropServices;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed partial class SpanHelpersTest
{
    [Fact]
    public void MemmoveTest()
    {
        const int SourceLength = 512;

        Span<int> source = new int[SourceLength];
        SpanHelpers.FillAscending(source);

        Span<int> destination = new int[SourceLength];

        SpanHelpers<int>.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (nuint)source.Length);
        Assert.True(destination.SequenceEqual(source));
    }
}
