using System;
using System.Numerics;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed class ArrayPoolTest
{
    private readonly ArrayPool<int> _integerArrayPool = new();
    private readonly ArrayPool<string> _stringArrayPool = new();

    [Fact]
    public void IndexOffsetIsTrailingZeroCountOfMinimumArrayLength()
        => Assert.Equal(ArrayPool<int>.IndexOffset, BitOperations.TrailingZeroCount(ArrayPool<int>.MinimumArrayLength));

    [Fact]
    public void RentReturnsEmptyArrayForLengthZero()
    {
        int[] array = _integerArrayPool.Rent(0);
        Assert.Empty(array);
        Assert.Same(array, Array.Empty<int>());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1024)]
    [InlineData(int.MinValue)]
    public void RentThrowsForNegativeLength(int negativeLength)
        => Assert.Throws<ArgumentOutOfRangeException>(() => _integerArrayPool.Rent(negativeLength));

    [Theory]
    [InlineData(1)]
    [InlineData(ArrayPool<int>.MinimumArrayLength - 1)]
    public void RentArrayShorterThanMinimumLength(int minimumLength)
    {
        int[] array = _integerArrayPool.Rent(minimumLength);
        Assert.Equal(ArrayPool<int>.MinimumArrayLength, array.Length);
    }

    [Theory]
    [InlineData(ArrayPool<int>.MinimumArrayLength)]
    [InlineData(256)]
    [InlineData(2048)]
    public void RentArrayOfPow2Length(int arrayLength)
    {
        int[] array = _integerArrayPool.Rent(arrayLength);
        int[] previousArray = array;
        _integerArrayPool.Return(array);
        for (int i = 0; i < 1024; i++)
        {
            array = _integerArrayPool.Rent(arrayLength);
            Assert.Equal(arrayLength, array.Length);
            Assert.Same(previousArray, array);
            _integerArrayPool.Return(array);
            previousArray = array;
        }
    }

    [Theory]
    [InlineData(ArrayPool<int>.MinimumArrayLength)]
    [InlineData(256)]
    [InlineData(2048)]
    public void RentArrayOfLargerSizeAndThanSmallerSize(int arrayLength)
    {
        int[] array = _integerArrayPool.Rent(arrayLength << 1);
        int[] previousArray = array;
        _integerArrayPool.Return(array);
        for (int i = 0; i < 1024; i++)
        {
            array = _integerArrayPool.Rent(arrayLength);
            Assert.Equal(arrayLength << 1, array.Length);
            Assert.Same(previousArray, array);
            _integerArrayPool.Return(array);
            previousArray = array;
        }
    }

    [Theory]
    [InlineData(ArrayPool<int>.MinimumArrayLength)]
    [InlineData(256)]
    [InlineData(2048)]
    public void RentAsRentedArrayTest(int minimumLength)
    {
        using RentedArray<int> rentedArray = _integerArrayPool.RentAsRentedArray(minimumLength);
        Assert.Same(_integerArrayPool, rentedArray._pool);
    }

    [Fact]
    public void ReturnNullArrayDoesntThrow() => _integerArrayPool.Return(null);

    [Fact]
    public void ClearArray_ManagedType()
    {
        string[] array = _stringArrayPool.Rent(32);
        Array.Fill(array, "hello");
        _stringArrayPool.Return(array);
        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void DontClearArray_ValueType()
    {
        int[] array = _integerArrayPool.Rent(32);
        Array.Fill(array, int.MaxValue);
        _integerArrayPool.Return(array);
        Assert.All(array, static i => Assert.Equal(int.MaxValue, i));
    }

    [Fact]
    public void DontClearArray_ValueType_ReturnOptionNone()
    {
        int[] array = _integerArrayPool.Rent(32);
        Array.Fill(array, int.MaxValue);
        _integerArrayPool.Return(array, ArrayReturnOptions.None);
        Assert.All(array, static i => Assert.Equal(int.MaxValue, i));
    }

    [Fact]
    public void DontClearArray_ManagedType_ReturnOptionNone()
    {
        string[] array = _stringArrayPool.Rent(32);
        Array.Fill(array, "hello");
        _stringArrayPool.Return(array, ArrayReturnOptions.None);
        Assert.All(array, static s => Assert.Same("hello", s));
    }

    [Fact]
    public void ClearArray_ValueType_ReturnOptionClear()
    {
        int[] array = _integerArrayPool.Rent(32);
        Array.Fill(array, int.MaxValue);
        _integerArrayPool.Return(array, ArrayReturnOptions.Clear);
        Assert.All(array, static i => Assert.Equal(0, i));
    }

    [Fact]
    public void ClearArray_ManagedType_ReturnOptionClear()
    {
        string[] array = _stringArrayPool.Rent(32);
        Array.Fill(array, "hello");
        _stringArrayPool.Return(array, ArrayReturnOptions.Clear);
        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void ClearPoolTest()
    {
        int[][] arrays = new int[8][];
        for (int i = 0; i < 8; i++)
        {
            int[] array = _integerArrayPool.Rent(ArrayPool<int>.MinimumArrayLength << i);
            arrays[i] = array;
            _integerArrayPool.Return(array);
        }

        _integerArrayPool.Clear();

        for (int i = 0; i < 8; i++)
        {
            int[] array = _integerArrayPool.Rent(ArrayPool<int>.MinimumArrayLength << i);
            Assert.Equal(arrays[i].Length, array.Length);
            Assert.NotSame(arrays[i], array);
        }
    }
}
