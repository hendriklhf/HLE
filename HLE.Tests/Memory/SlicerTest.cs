using System;
using HLE.Collections;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed class SlicerTest
{
    [Fact]
    public void CreateSpan_Start_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.CreateSpan(5);
        Assert.Equal(arrayLength - 5, span.Length);
        Assert.True(span is [5, .., 99]);
    }

    [Fact]
    public void CreateSpan_Start_Length_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.CreateSpan(5, 50);
        Assert.Equal(50, span.Length);
        Assert.True(span is [5, .., 54]);
    }

    [Fact]
    public void CreateSpan_Range_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.CreateSpan(5..50);
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
            _ = slicer.CreateSpan(101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1);
        });
    }

    [Fact]
    public void CreateSpan_Start_Length_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(0, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(101, 0);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(101, 100);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, 50);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(50, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue, int.MinValue);
        });
    }

    [Fact]
    public void CreateSpan_Range_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(..101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(101..);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(101..200);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(50..30);
        });

        // ReSharper disable NegativeIndex
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }

    [Fact]
    public void CreateReadOnlySpan_Start_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.CreateReadOnlySpan(5);
        Assert.Equal(arrayLength - 5, span.Length);
        Assert.True(span is [5, .., 99]);
    }

    [Fact]
    public void CreateReadOnlySpan_Start_Length_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.CreateReadOnlySpan(5, 50);
        Assert.Equal(50, span.Length);
        Assert.True(span is [5, .., 54]);
    }

    [Fact]
    public void CreateReadOnlySpan_Range_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.CreateReadOnlySpan(5..50);
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
            _ = slicer.CreateReadOnlySpan(101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1);
        });
    }

    [Fact]
    public void CreateReadOnlySpan_Start_Length_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(0, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(101, 0);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(101, 100);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, 50);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(50, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue, int.MinValue);
        });
    }

    [Fact]
    public void CreateReadOnlySpan_Range_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(..101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(101..);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(101..200);
        });

        // ReSharper disable NegativeIndex
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }
}
