using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace HLE.Tests;

[SuppressMessage("Roslynator", "RCS1154:Sort enum members")]
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
