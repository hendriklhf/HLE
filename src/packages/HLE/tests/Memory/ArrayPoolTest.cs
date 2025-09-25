using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using HLE.Memory;
using HLE.TestUtilities;

namespace HLE.UnitTests.Memory;

public sealed class ArrayPoolTest
{
    public static TheoryData<int> Pow2LengthMinimumToMaximumLengthParameters { get; } = CreatePow2LengthMinimumToMaximumLengthParameters();

    public static TheoryData<int> ConsecutiveValues0To4096Parameters { get; } = TheoryDataHelpers.CreateRange(0, 4096);

    public static TheoryData<int> ConsecutiveValues0ToMinimumLengthMinus1 { get; } = TheoryDataHelpers.CreateRange(0, ArrayPoolSettings.MinimumArrayLength - 1);

    [Fact]
    public void IndexOffsetIsTrailingZeroCountOfMinimumArrayLength()
        => Assert.Equal(ArrayPoolSettings.TrailingZeroCountBucketIndexOffset, BitOperations.TrailingZeroCount(ArrayPoolSettings.MinimumArrayLength));

    [Fact]
    public void MinimumAndMaximumLengthArePow2()
    {
        Assert.Equal(1, BitOperations.PopCount(ArrayPoolSettings.MinimumArrayLength));
        Assert.Equal(1, BitOperations.PopCount(ArrayPoolSettings.MaximumArrayLength));
    }

    [Fact]
    public void RentReturnsEmptyArrayForLengthZero()
    {
        using ArrayPool<int> pool = new();

        int[] array = pool.Rent(0);
        Assert.True(array.Length >= ArrayPoolSettings.MinimumArrayLength);
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
        Assert.True(array.Length >= ArrayPoolSettings.MinimumArrayLength);
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

        for (int i = 0; i < 4096; i++)
        {
            int[] array1 = RentRandomArray(pool);
            int[] array2 = RentRandomArray(pool);
            int[] array3 = RentRandomArray(pool);
            int[] array4 = RentRandomArray(pool);
            int[] array5 = RentRandomArray(pool);

            pool.Return(array1);
            pool.Return(array2);
            pool.Return(array3);
            pool.Return(array4);
            pool.Return(array5);
        }

        bool allNull = true;
        for (int i = 0; i < pool._buckets.Length; i++)
        {
            ref ArrayPool<int>.Bucket bucket = ref pool._buckets[i];
            for (int j = 0; j < ArrayPool<int>.Bucket.Pool.Length; j++)
            {
                if (bucket._pool[j] is not null)
                {
                    allNull = false;
                }
            }
        }

        Assert.False(allNull);

        pool.Clear();

        allNull = true;
        for (int i = 0; i < pool._buckets.Length; i++)
        {
            ref ArrayPool<int>.Bucket bucket = ref pool._buckets[i];
            for (int j = 0; j < ArrayPool<int>.Bucket.Pool.Length; j++)
            {
                if (bucket._pool[j] is not null)
                {
                    allNull = false;
                }
            }
        }

        Assert.True(allNull);
    }

    [Fact]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure", Justification = "The pool is disposed after the parallel loop")]
    public void Parallel_RentReturn()
    {
        using ArrayPool<int> pool = new();

        ParallelLoopResult parallelResult = Parallel.For(0, Environment.ProcessorCount, _ =>
        {
            for (int i = 0; i < 4096; i++)
            {
                int[] a = RentRandomArray(pool);
                int[] b = RentRandomArray(pool);
                int[] c = RentRandomArray(pool);
                int[] d = RentRandomArray(pool);
                int[] e = RentRandomArray(pool);

                pool.Return(a);
                pool.Return(b);
                pool.Return(c);
                pool.Return(d);
                pool.Return(e);
            }
        });

        Assert.True(parallelResult.IsCompleted);

        for (int i = 0; i < pool._buckets.Length; i++)
        {
            ref ArrayPool<int>.Bucket bucket = ref pool._buckets[i];
            Span<int[]?> arrays = bucket._pool;
            Assert.Equal(ArrayPool<int>.Bucket.Pool.Length, arrays.Length);

            for (int j = 0; j < arrays.Length; j++)
            {
                // Check if the array is null or not based on the _positions bitmask
                // if the bit is set, the array in the pool should not be null
                bool arrayShouldNotBeNull = (bucket._positions & (1 << j)) != 0;
                Assert.False(arrayShouldNotBeNull ^ arrays[j] is not null);
            }
        }
    }

    [Pure]
    private static int[] RentRandomArray(ArrayPool<int> pool)
    {
        int arrayLength = Random.Shared.Next(ArrayPoolSettings.MinimumArrayLength, ArrayPoolSettings.MaximumArrayLength + 1);
        return pool.Rent(arrayLength);
    }

    private static TheoryData<int> CreatePow2LengthMinimumToMaximumLengthParameters()
    {
        TheoryData<int> data = new();
        for (int i = ArrayPoolSettings.MinimumArrayLength; i <= ArrayPoolSettings.MaximumArrayLength; i <<= 1)
        {
            data.Add(i);
        }

        return data;
    }
}
