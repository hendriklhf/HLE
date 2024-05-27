using System;
using Xunit;

#pragma warning disable RCS1060 // Declare each type in separate file

namespace HLE.Tests;

public sealed partial class EnumValuesTest
{
    [Fact]
    public void GetValuesAsSpanTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();

        ReadOnlySpan<TestEnum> values = EnumValues<TestEnum>.AsSpan();
        Assert.True(values.SequenceEqual(actualValues));

        values = EnumValues<TestEnum>.AsSpan();
        Assert.True(values.SequenceEqual(actualValues));
    }

    [Fact]
    public void GetValuesAsUnderlyingType()
    {
        int[] actualValues = (int[])Enum.GetValuesAsUnderlyingType<TestEnum>();

        ReadOnlySpan<int> values = EnumValues<TestEnum>.AsSpan<int>();
        Assert.True(values.SequenceEqual(actualValues));

        values = EnumValues<TestEnum>.AsSpan<int>();
        Assert.True(values.SequenceEqual(actualValues));
    }

    [Fact]
    public void GetValuesAsInvalidUnderlyingType()
        => Assert.Throws<InvalidOperationException>(static () => _ = EnumValues<TestEnum>.AsSpan<long>());

    [Fact]
    public void ValuesAreSortedTest()
    {
        ReadOnlySpan<TestEnum> values = EnumValues<TestEnum>.AsSpan();
        Span<TestEnum> sortedValues = stackalloc TestEnum[values.Length];
        values.CopyTo(sortedValues);
        sortedValues.Sort();

        Assert.True(values.SequenceEqual(sortedValues));
    }

    [Fact]
    public void ValuesAsUnderlyingTypeAreSortedTest()
    {
        ReadOnlySpan<int> values = EnumValues<TestEnum>.AsSpan<int>();
        Span<int> sortedValues = stackalloc int[values.Length];
        values.CopyTo(sortedValues);
        sortedValues.Sort();

        Assert.True(values.SequenceEqual(sortedValues));
    }

    [Fact]
    public void CountTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();
        int valueCount = EnumValues<TestEnum>.Count;
        Assert.Equal(actualValues.Length, valueCount);
    }

    [Fact]
    public void MaximumValueTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();
        TestEnum maxValue = EnumValues<TestEnum>.MaximumValue;
        Assert.Equal(12345, (int)maxValue);
        Assert.Equal(actualValues[^1], maxValue);
    }
}
