using System;
using System.Numerics;
using HLE.Memory;
using Xunit;

namespace HLE.Tests.Memory;

public sealed class ArrayPoolTest
{
    [Fact]
    public void IndexOffsetIsTrailingZeroCountOfMinimumArrayLength()
        => Assert.Equal(ArrayPool<int>.IndexOffset, BitOperations.TrailingZeroCount(ArrayPool<int>.MinimumArrayLength));

    [Fact]
    public void RentReturnsEmptyArrayForLength0()
    {
        int[] array = ArrayPool<int>.Shared.Rent(0);
        Assert.Empty(array);
        Assert.True(ReferenceEquals(array, Array.Empty<int>()));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-1024)]
    [InlineData(int.MinValue)]
    public void RentThrowsForNegativeLength(int negativeLength)
        => Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPool<int>.Shared.Rent(negativeLength));
}
