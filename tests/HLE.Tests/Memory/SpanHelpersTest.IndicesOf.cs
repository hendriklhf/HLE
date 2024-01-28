using System;
using HLE.Collections;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed partial class SpanHelpersTest
{
    [Fact]
    public void IndicesOfTest()
    {
        ReadOnlySpan<char> values = Random.Shared.NextString(4096, "abc");
        ReadOnlySpan<int> indicesOf = values.IndicesOf('a');

        using ValueList<int> loopIndices = new(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 'a')
            {
                loopIndices.Add(i);
            }
        }

        Assert.True(indicesOf.SequenceEqual(loopIndices.AsSpan()));
    }
}
