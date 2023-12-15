using System;
using System.Linq;
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
        => Assert.Equal(ArrayPool<int>.s_indexOffset, BitOperations.TrailingZeroCount(ArrayPool<int>.MinimumArrayLength));

    [Fact]
    public void MinimumAndMaximumLengthArePow2()
    {
        Assert.True(BitOperations.IsPow2(ArrayPool<int>.MinimumArrayLength));
        Assert.True(BitOperations.IsPow2(ArrayPool<int>.MaximumArrayLength));
    }

    [Fact]
    public void RentReturnsNonEmptyArrayForLengthZero()
    {
        int[] array = _integerArrayPool.Rent(0);
        Assert.True(array.Length > 0);
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
        for (int i = 0; i < 8; i++)
        {
            int[] array = _integerArrayPool.Rent(ArrayPool<int>.MinimumArrayLength << i);
            int[] array2 = _integerArrayPool.Rent(ArrayPool<int>.MinimumArrayLength << i);
            _integerArrayPool.Return(array);
            _integerArrayPool.Return(array2);
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.False(_integerArrayPool._buckets.All(static b => b._stack.All(static a => a is null)));

        _integerArrayPool.Clear();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.True(_integerArrayPool._buckets.All(static b => b._stack.All(static a => a is null)));
    }
}
