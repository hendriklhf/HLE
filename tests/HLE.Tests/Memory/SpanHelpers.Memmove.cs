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
        for (int i = 0; i < 1024; i++)
        {
            int elementCount = Random.Shared.Next(0, 524288);
            Span<int> source = new int[elementCount];
            Random.Shared.Fill(source);
            Span<int> destination = new int[elementCount];

            SpanHelpers<int>.Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);
            Assert.True(destination.SequenceEqual(source));
        }
    }
}
