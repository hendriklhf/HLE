using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed class ArrayPoolTest
{
    public static TheoryData<int> Pow2LengthMinimumToMaximumLengthParameters { get; } = CreatePow2LengthMinimumToMaximumLengthParameters();

    public static TheoryData<int> ConsecutiveValues0To4096Parameters { get; } = CreateConsecutiveValues0To4096Parameters();

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
        ArrayPool<int> pool = new();

        int[] array = pool.Rent(0);
        Assert.NotEmpty(array);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-4)]
    [InlineData(-1024)]
    [InlineData(-34_534_765)]
    [InlineData(int.MinValue)]
    public void RentThrowsForNegativeLength(int negativeLength)
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ArrayPool<int> pool = new();
            return pool.Rent(negativeLength);
        });

    [Theory]
    [InlineData(1)]
    [InlineData(ArrayPool.MinimumArrayLength - 1)]
    public void RentArrayShorterThanMinimumLength(int minimumLength)
    {
        ArrayPool<int> pool = new();

        int[] array = pool.Rent(minimumLength);
        Assert.Equal(ArrayPool.MinimumArrayLength, array.Length);
    }

    [Theory]
    [MemberData(nameof(ConsecutiveValues0To4096Parameters))]
    public void RentArrayMinimumLength(int minimumLength)
    {
        ArrayPool<int> pool = new();

        int[] array = pool.Rent(minimumLength);
        int[] previousArray = array;
        pool.Return(array);
        for (int i = 0; i < 1024; i++)
        {
            array = pool.Rent(minimumLength);

            Assert.True(array.Length >= minimumLength);
            Assert.Same(previousArray, array);
            Assert.Equal(GC.MaxGeneration, GC.GetGeneration(array)); // array is pinned

            pool.Return(array);
            previousArray = array;
        }
    }

    [Theory]
    [MemberData(nameof(Pow2LengthMinimumToMaximumLengthParameters))]
    [SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations")]
    public void RentArrayOfPow2Length(int minimumLength)
    {
        ArrayPool<int> pool = new();

        int[] array = pool.Rent(minimumLength);
        int[] previousArray = array;
        pool.Return(array);
        for (int i = 0; i < 1024; i++)
        {
            array = pool.Rent(minimumLength);

            Assert.True(array.Length >= minimumLength);
            Assert.Same(previousArray, array);
            Assert.Equal(GC.MaxGeneration, GC.GetGeneration(array)); // array is pinned

            pool.Return(array);
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
        ArrayPool<int> pool = new();

        using RentedArray<int> rentedArray = pool.RentAsRentedArray(minimumLength);
        Assert.True(rentedArray.Length >= minimumLength);
        Assert.Same(pool, rentedArray._pool);
    }

    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions")]
    public void ReturnNullArrayDoesntThrow()
    {
        ArrayPool<int> pool = new();
        pool.Return(null);
    }

    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions")]
    public void ReturnEmptyArrayDoesntThrow()
    {
        ArrayPool<int> pool = new();
        pool.Return([]);
    }

    [Fact]
    public void ClearArray_WithClearOnlyIfManagedType_ManagedType()
    {
        ArrayPool<string> pool = new();

        string[] array = pool.Rent(32);
        Array.Fill(array, "hello");
        pool.Return(array, ArrayReturnOptions.ClearOnlyIfManagedType);
        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void DontClearArray_WithClearOnlyIfManagedType_ValueType()
    {
        ArrayPool<int> pool = new();

        int[] array = pool.Rent(32);
        Array.Fill(array, int.MaxValue);
        pool.Return(array, ArrayReturnOptions.ClearOnlyIfManagedType);
        Assert.All(array, static i => Assert.Equal(int.MaxValue, i));
    }

    [Fact]
    public void DontClearArray_ValueType_ReturnOptionNone()
    {
        ArrayPool<int> pool = new();

        int[] array = pool.Rent(32);
        Array.Fill(array, int.MaxValue);
        pool.Return(array, ArrayReturnOptions.None);
        Assert.All(array, static i => Assert.Equal(int.MaxValue, i));
    }

    [Fact]
    public void DontClearArray_ManagedType_ReturnOptionNone()
    {
        ArrayPool<string> pool = new();

        string[] array = pool.Rent(32);
        Array.Fill(array, "hello");
        pool.Return(array, ArrayReturnOptions.None);
        Assert.All(array, static s => Assert.Same("hello", s));
    }

    [Fact]
    public void ClearArray_ValueType_ReturnOptionClear()
    {
        ArrayPool<int> pool = new();

        int[] array = pool.Rent(32);
        Array.Fill(array, int.MaxValue);
        pool.Return(array, ArrayReturnOptions.Clear);
        Assert.All(array, static i => Assert.Equal(0, i));
    }

    [Fact]
    public void ClearArray_ManagedType_ReturnOptionClear()
    {
        ArrayPool<string> pool = new();

        string[] array = pool.Rent(32);
        Array.Fill(array, "hello");
        pool.Return(array, ArrayReturnOptions.Clear);
        Assert.All(array, static s => Assert.Null(s));
    }

    [Fact]
    public void ClearPoolTest()
    {
        ArrayPool<int> pool = new();

        for (int i = 0; i < 8; i++)
        {
            int[] array = pool.Rent(ArrayPool.MinimumArrayLength << i);
            int[] array2 = pool.Rent(ArrayPool.MinimumArrayLength << i);
            pool.Return(array);
            pool.Return(array2);
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.False(Array.TrueForAll(pool._buckets, static b => Array.TrueForAll(b._stack, static a => a is null)));

        pool.Clear();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Assert.True(Array.TrueForAll(pool._buckets, static b => Array.TrueForAll(b._stack, static a => a is null)));
    }

    [Theory]
    [MemberData(nameof(ConsecutiveValues0To4096Parameters))]
    public void RentExactTest(int length)
    {
        ArrayPool<int> pool = new();

        int[] firstArray = pool.RentExact(length);
        pool.Return(firstArray);
        int[] secondArray = pool.RentExact(length);
        pool.Return(secondArray);

        int roundedLength = (int)BitOperations.RoundUpToPowerOf2((uint)length);
        if (roundedLength != length) // basically a !IsPow2 check
        {
            roundedLength >>>= 1;

            // initializes the thread local bucket
            // only if "length" is not pow2, the bucket hasn't been initialized
            _ = pool.Rent(roundedLength);
        }

        int[] thirdArray = pool.Rent(roundedLength);

        Assert.Equal(length, firstArray.Length);
        Assert.Equal(length, secondArray.Length);
        Assert.True(thirdArray.Length >= length);

        if (length >= ArrayPool.MinimumArrayLength)
        {
            Assert.Same(firstArray, secondArray);
            Assert.Same(firstArray, thirdArray);
            Assert.Same(thirdArray, secondArray);
            Assert.Equal(GC.MaxGeneration, GC.GetGeneration(firstArray)); // array is pinned
        }
    }

    private static TheoryData<int> CreatePow2LengthMinimumToMaximumLengthParameters()
    {
        TheoryData<int> data = new();
        for (int i = ArrayPool.MinimumArrayLength; i <= ArrayPool.MaximumArrayLength; i <<= 1)
        {
            data.Add(i);
        }

        return data;
    }

    private static TheoryData<int> CreateConsecutiveValues0To4096Parameters()
    {
        TheoryData<int> data = new();
        for (int i = 0; i <= 4096; i++)
        {
            data.Add(i);
        }

        return data;
    }
}
