using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Memory;

namespace HLE.UnitTests.Memory;

public sealed class ArrayPoolSettingsTests
{
    [Fact]
    public void IndexOffsetIsTrailingZeroCountOfMinimumArrayLength()
        => Assert.Equal(ArrayPoolSettings.TrailingZeroCountBucketIndexOffset, BitOperations.TrailingZeroCount(ArrayPoolSettings.MinimumArrayLength));

    [Fact]
    public void TrimmingIntervalIsNotZero()
        => Assert.NotEqual(TimeSpan.Zero, ArrayPoolSettings.TrimmingInterval);

    [Fact]
    public void MaximumLastAccessTimeIsNotZero()
        => Assert.NotEqual(TimeSpan.Zero, ArrayPoolSettings.MaximumLastAccessTime);

    [Fact]
    public void MinimumAndMaximumLengthArePow2()
    {
        Assert.Equal(1, BitOperations.PopCount(ArrayPoolSettings.MinimumArrayLength));
        Assert.Equal(1, BitOperations.PopCount(ArrayPoolSettings.MaximumArrayLength));
        Assert.Equal(1, BitOperations.PopCount(ArrayPoolSettings.MaximumPow2Length));
    }

    [Fact]
    public void MinimumArrayLength_IsLessThan_MaximumArrayLength()
        => Assert.True(ArrayPoolSettings.MinimumArrayLength < ArrayPoolSettings.MaximumArrayLength);

    [Fact]
    public void MaximumPow2Length_IsGreaterThanOrEqual_MaximumArrayLength()
        => Assert.True(ArrayPoolSettings.MaximumPow2Length >= ArrayPoolSettings.MaximumArrayLength);

    [Fact]
    public void TrimThresholdsAreLargeEnough()
    {
        Assert.True(ArrayPoolSettings.TrimThreshold > 0.5);
        Assert.True(ArrayPoolSettings.CommonlyPooledTypeTrimThreshold > 0.5);
    }

    [Fact]
    public void CommonlyPooledTypeTrimThreshold_IsLargerThan_TrimThreshold()
        => Assert.True(ArrayPoolSettings.CommonlyPooledTypeTrimThreshold > ArrayPoolSettings.TrimThreshold);

    [Fact]
    public void IsCommonlyPooledType_ReturnsTrue_ForPrimitives()
    {
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<byte>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<sbyte>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<short>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<ushort>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<int>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<uint>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<long>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<ulong>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<nint>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<nuint>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<bool>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<char>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<float>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<double>());
    }

    [Fact]
    public void IsCommonlyPooledType_ReturnsTrue_ForEnums()
    {
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<RegexOptions>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<StringComparison>());
        Assert.True(ArrayPoolSettings.IsCommonlyPooledType<MethodImplOptions>());
    }

    [Fact]
    public void IsCommonlyPooledType_ReturnsTrue_ForString()
        => Assert.True(ArrayPoolSettings.IsCommonlyPooledType<string>());

    [Fact]
    public void IsCommonlyPooledType_ReturnsFalse_OtherTypes()
    {
        Assert.False(ArrayPoolSettings.IsCommonlyPooledType<object>());
        Assert.False(ArrayPoolSettings.IsCommonlyPooledType<object>());
        Assert.False(ArrayPoolSettings.IsCommonlyPooledType<object>());
        Assert.False(ArrayPoolSettings.IsCommonlyPooledType<object>());
        Assert.False(ArrayPoolSettings.IsCommonlyPooledType<object>());
    }
}
