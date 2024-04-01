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
        => Assert.Equal(ArrayPool.BucketIndexOffset, BitOperations.TrailingZeroCount(ArrayPool.MinimumArrayLength));

    [Fact]
    public void MinimumAndMaximumLengthArePow2()
    {
        Assert.Equal(1, BitOperations.PopCount(ArrayPool.MinimumArrayLength));
        Assert.Equal(1, BitOperations.PopCount(ArrayPool.MaximumArrayLength));
    }

    [Fact]
    public void RentReturnsNonEmptyArrayForLengthZero()
    {
        int[] array = _integerArrayPool.Rent(0);
        Assert.NotEmpty(array);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-4)]
    [InlineData(-1024)]
    [InlineData(-34534765)]
    [InlineData(int.MinValue)]
    public void RentThrowsForNegativeLength(int negativeLength)
        => Assert.Throws<ArgumentOutOfRangeException>(() => _integerArrayPool.Rent(negativeLength));

    [Theory]
    [InlineData(1)]
    [InlineData(ArrayPool.MinimumArrayLength - 1)]
    public void RentArrayShorterThanMinimumLength(int minimumLength)
    {
        int[] array = _integerArrayPool.Rent(minimumLength);
        Assert.Equal(ArrayPool.MinimumArrayLength, array.Length);
    }

    [Theory]
    [InlineData(ArrayPool.MinimumArrayLength)]
    [InlineData(256)]
    [InlineData(2048)]
    [InlineData(4096)]
    [InlineData(8192)]
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
    [InlineData(ArrayPool.MinimumArrayLength)]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(2048)]
    [InlineData(4096)]
    public void RentAsRentedArrayTest(int minimumLength)
    {
        using RentedArray<int> rentedArray = _integerArrayPool.RentAsRentedArray(minimumLength);
        Assert.True(rentedArray.Length >= minimumLength);
        Assert.Same(_integerArrayPool, rentedArray._pool);
    }

    [Fact]
    public void ReturnNullArrayDoesntThrow() => _integerArrayPool.Return(null);

    [Fact]
    public void ReturnEmptyArrayDoesntThrow() => _integerArrayPool.Return([]);

    [Fact]
    public void ClearArray_WithClearOnlyIfManagedType_ManagedType()
    {
        string[] array = _stringArrayPool.Rent(32);
        Array.Fill(array, "hello");
        _stringArrayPool.Return(array, ArrayReturnOptions.ClearOnlyIfManagedType);
        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void DontClearArray_WithClearOnlyIfManagedType_ValueType()
    {
        int[] array = _integerArrayPool.Rent(32);
        Array.Fill(array, int.MaxValue);
        _integerArrayPool.Return(array, ArrayReturnOptions.ClearOnlyIfManagedType);
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
            int[] array = _integerArrayPool.Rent(ArrayPool.MinimumArrayLength << i);
            int[] array2 = _integerArrayPool.Rent(ArrayPool.MinimumArrayLength << i);
            _integerArrayPool.Return(array);
            _integerArrayPool.Return(array2);
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.False(_integerArrayPool._buckets.All(static b => b._stack.All(static a => a is null)));

        _integerArrayPool.Clear();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.True(_integerArrayPool._buckets.All(static b => b._stack.All(static a => a is null)));
    }

    [Theory]
    [InlineData(44)]
    [InlineData(474)]
    [InlineData(1536)]
    [InlineData(8461)]
    public void RentExactTest(int length)
    {
        int[] firstArray = _integerArrayPool.RentExact(length);
        _integerArrayPool.Return(firstArray);
        int[] secondArray = _integerArrayPool.RentExact(length);
        _integerArrayPool.Return(secondArray);

        // initializes the thread local bucket
        _ = _integerArrayPool.Rent((int)(BitOperations.RoundUpToPowerOf2((uint)length) >> 1));

        int[] thirdArray = _integerArrayPool.Rent((int)(BitOperations.RoundUpToPowerOf2((uint)length) >> 1));

        Assert.Equal(length, firstArray.Length);
        Assert.Equal(length, secondArray.Length);
        Assert.Equal(length, thirdArray.Length);

        Assert.Same(firstArray, secondArray);
        Assert.Same(firstArray, thirdArray);
        Assert.Same(thirdArray, secondArray);
    }
}
