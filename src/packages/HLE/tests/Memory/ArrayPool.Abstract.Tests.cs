using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE.UnitTests.Memory;

public abstract class ArrayPoolAbstractTests<T>
{
    [Fact]
    public void RentReturnsNonEmptyArrayForLengthZero()
    {
        using ArrayPool<T> pool = new();

        T[] array = pool.Rent(0);
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
            using ArrayPool<T> pool = new();
            return pool.Rent(negativeLength);
        });

    [Theory]
    [MemberData(nameof(ArrayPoolTheoryData.ConsecutiveValues0ToMinimumLengthMinus1), MemberType = typeof(ArrayPoolTheoryData))]
    public void RentArrayShorterThanMinimumLength(int length)
    {
        using ArrayPool<T> pool = new();
        T[] array = pool.Rent(length);
        Assert.True(array.Length >= ArrayPoolSettings.MinimumArrayLength);
    }

    [Theory]
    [MemberData(nameof(ArrayPoolTheoryData.ConsecutiveValues0To4096Parameters), MemberType = typeof(ArrayPoolTheoryData))]
    public void RentArrayMinimumLength(int minimumLength)
    {
        using ArrayPool<T> pool = new();

        T[] array = pool.Rent(minimumLength);
        T[] previousArray = array;
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
    [MemberData(nameof(ArrayPoolTheoryData.Pow2LengthMinimumToMaximumLengthParameters), MemberType = typeof(ArrayPoolTheoryData))]
    [SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations")]
    public void RentArrayOfPow2Length(int minimumLength)
    {
        using ArrayPool<T> pool = new();

        T[] array = pool.Rent(minimumLength);
        T[] previousArray = array;
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
        using ArrayPool<T> pool = new();
        pool.Return(null);
    }

    [Fact]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions")]
    public void ReturnEmptyArrayDoesntThrow()
    {
        using ArrayPool<T> pool = new();
        pool.Return([]);
    }

    [Fact]
    public void ClearPoolTest()
    {
        using ArrayPool<T> pool = new();

        for (int i = 0; i < 4096; i++)
        {
            T[] array1 = RentRandomArray(pool);
            T[] array2 = RentRandomArray(pool);
            T[] array3 = RentRandomArray(pool);
            T[] array4 = RentRandomArray(pool);
            T[] array5 = RentRandomArray(pool);

            pool.Return(array1);
            pool.Return(array2);
            pool.Return(array3);
            pool.Return(array4);
            pool.Return(array5);
        }

        bool allNull = true;
        for (int i = 0; i < pool._buckets.Length; i++)
        {
            ref ArrayPool<T>.Bucket bucket = ref pool._buckets[i];
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
            ref ArrayPool<T>.Bucket bucket = ref pool._buckets[i];
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
        using ArrayPool<T> pool = new();

        ParallelLoopResult parallelResult = Parallel.For(0, Environment.ProcessorCount, _ =>
        {
            for (int i = 0; i < 4096; i++)
            {
                T[] a = RentRandomArray(pool);
                T[] b = RentRandomArray(pool);
                T[] c = RentRandomArray(pool);
                T[] d = RentRandomArray(pool);
                T[] e = RentRandomArray(pool);

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
            ref ArrayPool<T>.Bucket bucket = ref pool._buckets[i];
            Span<T[]?> arrays = bucket._pool;
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
    private static T[] RentRandomArray(ArrayPool<T> pool)
    {
        int arrayLength = Random.Shared.Next(ArrayPoolSettings.MinimumArrayLength, ArrayPoolSettings.MaximumArrayLength + 1);
        return pool.Rent(arrayLength);
    }
}
