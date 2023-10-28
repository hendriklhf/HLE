using System;
using Xunit;

namespace HLE.Tests;

public enum TestEnum
{
    A = 50,
    B = 35,
    C = 0,
    D = 12345,
    E = 55,
    F = 1,
    G = 3,
    H = 8
}

public sealed class EnumValuesTest
{
    [Fact]
    public void GetValuesTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();

        ReadOnlySpan<TestEnum> values = EnumValues<TestEnum>.GetValues();
        Assert.True(values.SequenceEqual(actualValues));

        values = EnumValues<TestEnum>.GetValues();
        Assert.True(values.SequenceEqual(actualValues));
    }

    [Fact]
    public void GetValuesAsUnderlyingType()
    {
        int[] actualValues = (int[])Enum.GetValuesAsUnderlyingType<TestEnum>();

        ReadOnlySpan<int> values = EnumValues<TestEnum>.GetValuesAs<int>();
        Assert.True(values.SequenceEqual(actualValues));

        values = EnumValues<TestEnum>.GetValuesAs<int>();
        Assert.True(values.SequenceEqual(actualValues));
    }

    [Fact]
    public void GetValuesAsInvalidUnderlyingType()
        => Assert.Throws<InvalidOperationException>(static () => _ = EnumValues<TestEnum>.GetValuesAs<long>());

    [Fact]
    public void ValuesAreSortedTest()
    {
        ReadOnlySpan<TestEnum> values = EnumValues<TestEnum>.GetValues();
        Span<TestEnum> sortedValues = stackalloc TestEnum[values.Length];
        values.CopyTo(sortedValues);
        sortedValues.Sort();

        Assert.True(values.SequenceEqual(sortedValues));
    }

    [Fact]
    public void ValuesAsUnderlyingTypeAreSortedTest()
    {
        ReadOnlySpan<int> values = EnumValues<TestEnum>.GetValuesAs<int>();
        Span<int> sortedValues = stackalloc int[values.Length];
        values.CopyTo(sortedValues);
        sortedValues.Sort();

        Assert.True(values.SequenceEqual(sortedValues));
    }

    [Fact]
    public void GetValueCountTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();
        int valueCount = EnumValues<TestEnum>.GetValueCount();
        Assert.Equal(actualValues.Length, valueCount);
    }

    [Fact]
    public void GetMaxValueTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();
        TestEnum maxValue = EnumValues<TestEnum>.GetMaxValue();
        Assert.Equal(12345, (int)maxValue);
        Assert.Equal(actualValues[^1], maxValue);
    }
}
