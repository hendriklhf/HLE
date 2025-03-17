using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using HLE.Memory;
using HLE.TestUtilities;
using Xunit;

namespace HLE.UnitTests.Memory;

public sealed class ArrayPoolTest
{
    public static TheoryData<int> Pow2LengthMinimumToMaximumLengthParameters { get; } = CreatePow2LengthMinimumToMaximumLengthParameters();

    public static TheoryData<int> ConsecutiveValues0To4096Parameters { get; } = TheoryDataHelpers.CreateRange(0, 4096);

    public static TheoryData<int> ConsecutiveValues0ToMinimumLengthMinus1 { get; } = TheoryDataHelpers.CreateRange(0, ArrayPool.MinimumArrayLength - 1);

    [Fact]
    public void IndexOffsetIsTrailingZeroCountOfMinimumArrayLength()
        => Assert.Equal(ArrayPool.TrailingZeroCountBucketIndexOffset, BitOperations.TrailingZeroCount(ArrayPool.MinimumArrayLength));

    [Fact]
    public void MinimumAndMaximumLengthArePow2()
    {
        Assert.Equal(1, BitOperations.PopCount(ArrayPool.MinimumArrayLength));
        Assert.Equal(1, BitOperations.PopCount(ArrayPool.MaximumArrayLength));
    }

    [Fact]
    public void RentReturnsEmptyArrayForLengthZero()
    {
        using ArrayPool<int> pool = new();

        int[] array = pool.Rent(0);
        Assert.True(array.Length >= ArrayPool.MinimumArrayLength);
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
            using ArrayPool<int> pool = new();
            return pool.Rent(negativeLength);
        });

    [Theory]
    [MemberData(nameof(ConsecutiveValues0ToMinimumLengthMinus1))]
    public void RentArrayShorterThanMinimumLength(int length)
    {
        using ArrayPool<int> pool = new();
        int[] array = pool.Rent(length);
        Assert.True(array.Length >= ArrayPool.MinimumArrayLength);
    }

    [Theory]
    [MemberData(nameof(ConsecutiveValues0To4096Parameters))]
    public void RentArrayMinimumLength(int minimumLength)
    {
        using ArrayPool<int> pool = new();

        int[] array = pool.Rent(minimumLength);
        int[] previousArray = array;
        pool.Return(array);
        for (int i = 0; i < 1024; i++)
        {
            array = pool.Rent(minimumLength);

            Assert.True(array.Length >= minimumLength);
            Assert.Same(previousArray, array);

            pool.Return(array);
            previousArray = array;
        }
    }

    [Theory]
    [MemberData(nameof(Pow2LengthMinimumToMaximumLengthParameters))]
    [SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations")]
    public void RentArrayOfPow2Length(int minimumLength)
    {
        using ArrayPool<int> pool = new();

        int[] array = pool.Rent(minimumLength);
        int[] previousArray = array;
        pool.Return(array);
        for (int i = 0; i < 1024; i++)
        {
            array = pool.Rent(minimumLength);

            Assert.True(array.Length >= minimumLength);
            Assert.Same(previousArray, array);

            pool.Return(array);
            previousArray = array;
        }
    }

    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions")]
    public void ReturnNullArrayDoesntThrow()
    {
        using ArrayPool<int> pool = new();
        pool.Return(null);
    }

    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions")]
    public void ReturnEmptyArrayDoesntThrow()
    {
        using ArrayPool<int> pool = new();
        pool.Return([]);
    }

    [Fact]
    public void DontClearArray_ValueType_Test()
    {
        using ArrayPool<int> pool = new();

        int[] array = pool.Rent(32);
        Array.Fill(array, int.MaxValue);
        pool.Return(array);
        Assert.All(array, static i => Assert.Equal(int.MaxValue, i));
    }

    [Fact]
    public void ClearPoolTest()
    {
        using ArrayPool<int> pool = new();

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

    private static TheoryData<int> CreatePow2LengthMinimumToMaximumLengthParameters()
    {
        TheoryData<int> data = new();
        for (int i = ArrayPool.MinimumArrayLength; i <= ArrayPool.MaximumArrayLength; i <<= 1)
        {
            data.Add(i);
        }

        return data;
    }
}
