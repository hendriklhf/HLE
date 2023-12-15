using System;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed class SlicerTest
{
    [Fact]
    public void CreateSpan_Start_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending<int>(array);

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.SliceSpan(5);
        Assert.Equal(ArrayLength - 5, span.Length);
        Assert.True(span is [5, .., 99]);
    }

    [Fact]
    public void CreateSpan_Start_Length_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending<int>(array);

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.SliceSpan(5, 50);
        Assert.Equal(50, span.Length);
        Assert.True(span is [5, .., 54]);
    }

    [Fact]
    public void CreateSpan_Range_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending<int>(array);

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.SliceSpan(5..50);
        Assert.Equal(45, span.Length);
        Assert.True(span is [5, .., 49]);
    }

    [Fact]
    public void CreateSpan_Start_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-1);
        });
    }

    [Fact]
    public void CreateSpan_Start_Length_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(0, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(101, 0);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(101, 100);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-1, 50);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(50, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-1, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(int.MinValue, int.MinValue);
        });
    }

    [Fact]
    public void CreateSpan_Range_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(..101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(101..);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(101..200);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(50..30);
        });

        // ReSharper disable NegativeIndex
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-5..30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }

    [Fact]
    public void CreateReadOnlySpan_Start_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending<int>(array);

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.SliceReadOnlySpan(5);
        Assert.Equal(ArrayLength - 5, span.Length);
        Assert.True(span is [5, .., 99]);
    }

    [Fact]
    public void CreateReadOnlySpan_Start_Length_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending<int>(array);

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.SliceReadOnlySpan(5, 50);
        Assert.Equal(50, span.Length);
        Assert.True(span is [5, .., 54]);
    }

    [Fact]
    public void CreateReadOnlySpan_Range_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending<int>(array);

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.SliceReadOnlySpan(5..50);
        Assert.Equal(45, span.Length);
        Assert.True(span is [5, .., 49]);
    }

    [Fact]
    public void CreateReadOnlySpan_Start_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceReadOnlySpan(101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-1);
        });
    }

    [Fact]
    public void CreateReadOnlySpan_Start_Length_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceReadOnlySpan(0, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceReadOnlySpan(101, 0);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceReadOnlySpan(101, 100);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-1, 50);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(50, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-1, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(int.MinValue, int.MinValue);
        });
    }

    [Fact]
    public void CreateReadOnlySpan_Range_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceReadOnlySpan(..101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceReadOnlySpan(101..);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceReadOnlySpan(101..200);
        });

        // ReSharper disable NegativeIndex
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-5..30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(-5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.SliceSpan(int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }
}
