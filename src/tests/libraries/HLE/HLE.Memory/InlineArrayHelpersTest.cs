using System;
using System.Runtime.CompilerServices;

namespace HLE.Memory.UnitTests;

public sealed partial class InlineArrayHelpersTest
{
    [Fact]
    public void GetReference_Test()
    {
        SomeInlineArray<int> array = default;
        ref int reference = ref InlineArrayHelpers.GetReference<SomeInlineArray<int>, int>(ref array);
        for (int i = 0; i < SomeInlineArray<int>.Length; i++)
        {
            Unsafe.Add(ref reference, i) = i;
        }

        for (int i = 0; i < SomeInlineArray<int>.Length; i++)
        {
            Assert.Equal(i, array[i]);
        }
    }

    [Fact]
    public void AsSpan_WithExplicitLength_Test()
    {
        SomeInlineArray<int> array = default;
        Span<int> span = InlineArrayHelpers.AsSpan<SomeInlineArray<int>, int>(ref array, SomeInlineArray<int>.Length);
        Assert.Equal(SomeInlineArray<int>.Length, span.Length);
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = i;
        }

        for (int i = 0; i < SomeInlineArray<int>.Length; i++)
        {
            Assert.Equal(i, array[i]);
        }
    }

    [Fact]
    public void AsSpan_Test()
    {
        SomeInlineArray<int> array = default;
        Span<int> span = InlineArrayHelpers.AsSpan<SomeInlineArray<int>, int>(ref array);
        Assert.Equal(SomeInlineArray<int>.Length, span.Length);
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = i;
        }

        for (int i = 0; i < SomeInlineArray<int>.Length; i++)
        {
            Assert.Equal(i, array[i]);
        }
    }
}
