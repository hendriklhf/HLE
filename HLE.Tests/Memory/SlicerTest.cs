using System;
using HLE.Collections;
using HLE.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Memory;

[TestClass]
public class SlicerTest
{
    [TestMethod]
    public void CreateSpan_Start_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.CreateSpan(5);
        Assert.AreEqual(arrayLength - 5, span.Length);
        Assert.IsTrue(span is [5, .., 99]);
    }

    [TestMethod]
    public void CreateSpan_Start_Length_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.CreateSpan(5, 50);
        Assert.AreEqual(50, span.Length);
        Assert.IsTrue(span is [5, .., 54]);
    }

    [TestMethod]
    public void CreateSpan_Range_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        Span<int> span = slicer.CreateSpan(5..50);
        Assert.AreEqual(45, span.Length);
        Assert.IsTrue(span is [5, .., 49]);
    }

    [TestMethod]
    public void CreateSpan_Start_OutOfRange_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(100);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1);
        });
    }

    [TestMethod]
    public void CreateSpan_Start_Length_OutOfRange_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(0, 101);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(100, 0);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(100, 100);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, 50);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(50, -1);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, -1);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue, int.MinValue);
        });
    }

    [TestMethod]
    public void CreateSpan_Range_OutOfRange_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(..101);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(100..);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(100..200);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(50..30);
        });

        // ReSharper disable NegativeIndex
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..30);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(5..-30);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..-30);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }

    [TestMethod]
    public void CreateReadOnlySpan_Start_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.CreateReadOnlySpan(5);
        Assert.AreEqual(arrayLength - 5, span.Length);
        Assert.IsTrue(span is [5, .., 99]);
    }

    [TestMethod]
    public void CreateReadOnlySpan_Start_Length_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.CreateReadOnlySpan(5, 50);
        Assert.AreEqual(50, span.Length);
        Assert.IsTrue(span is [5, .., 54]);
    }

    [TestMethod]
    public void CreateReadOnlySpan_Range_Test()
    {
        const int arrayLength = 100;
        int[] array = new int[arrayLength];
        array.FillAscending();

        Slicer<int> slicer = new(array);
        ReadOnlySpan<int> span = slicer.CreateReadOnlySpan(5..50);
        Assert.AreEqual(45, span.Length);
        Assert.IsTrue(span is [5, .., 49]);
    }

    [TestMethod]
    public void CreateReadOnlySpan_Start_OutOfRange_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(100);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1);
        });
    }

    [TestMethod]
    public void CreateReadOnlySpan_Start_Length_OutOfRange_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(0, 101);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(100, 0);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(100, 100);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, 50);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(50, -1);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-1, -1);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue, int.MinValue);
        });
    }

    [TestMethod]
    public void CreateReadOnlySpan_Range_OutOfRange_Test()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(..101);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(100..);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateReadOnlySpan(100..200);
        });

        // ReSharper disable NegativeIndex
        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..30);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(5..-30);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(-5..-30);
        });

        Assert.ThrowsException<ArgumentOutOfRangeException>(static () =>
        {
            int[] array = new int[100];
            Slicer<int> slicer = new(array);
            _ = slicer.CreateSpan(int.MinValue..int.MinValue);
        });
        // ReSharper restore NegativeIndex
    }
}
