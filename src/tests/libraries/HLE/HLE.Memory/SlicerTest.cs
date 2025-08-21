using System;
using System.Runtime.InteropServices;

namespace HLE.Memory.UnitTests;

public sealed class SlicerTest
{
    [Fact]
    public void Slice_Start_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending(array);

        Span<int> span = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5);
        Assert.Equal(ArrayLength - 5, span.Length);
        Assert.True(span is [5, .., 99]);
    }

    [Fact]
    public void Slice_Start_Length_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending(array);

        Span<int> span = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5, 50);
        Assert.Equal(50, span.Length);
        Assert.True(span is [5, .., 54]);
    }

    [Fact]
    public void Slice_Range_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending(array);

        Span<int> span = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5..50);
        Assert.Equal(45, span.Length);
        Assert.True(span is [5, .., 49]);
    }

    [Fact]
    public void Slice_Start_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -1);
        });
    }

    [Fact]
    public void Slice_Start_Length_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 0, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101, 0);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101, 100);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -1, 50);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 50, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -1, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, int.MinValue, int.MinValue);
        });
    }

    [Fact]
    public void Slice_Range_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, ..101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101..);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101..200);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 50..30);
        });

        // ReSharper disable NegativeIndex
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -5..30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.Slice(ref MemoryMarshal.GetArrayDataReference(array), array.Length, int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }

    [Fact]
    public void SliceReadOnly_Start_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending(array);

        ReadOnlySpan<int> span = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5);
        Assert.Equal(ArrayLength - 5, span.Length);
        Assert.True(span is [5, .., 99]);
    }

    [Fact]
    public void SliceReadOnly_Start_Length_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending(array);

        ReadOnlySpan<int> span = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5, 50);
        Assert.Equal(50, span.Length);
        Assert.True(span is [5, .., 54]);
    }

    [Fact]
    public void SliceReadOnly_Range_Test()
    {
        const int ArrayLength = 100;
        int[] array = new int[ArrayLength];
        SpanHelpers.FillAscending(array);

        ReadOnlySpan<int> span = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5..50);
        Assert.Equal(45, span.Length);
        Assert.True(span is [5, .., 49]);
    }

    [Fact]
    public void SliceReadOnly_Start_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -1);
        });
    }

    [Fact]
    public void SliceReadOnly_Start_Length_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 0, 101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101, 0);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101, 100);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -1, 50);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 50, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -1, -1);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, int.MinValue, int.MinValue);
        });
    }

    [Fact]
    public void SliceReadOnly_Range_OutOfRange_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, ..101);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101..);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 101..200);
        });

        // ReSharper disable NegativeIndex
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -5..30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, 5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, -5..-30);
        });

        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            _ = Slicer.SliceReadOnly(ref MemoryMarshal.GetArrayDataReference(array), array.Length, int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }
}
