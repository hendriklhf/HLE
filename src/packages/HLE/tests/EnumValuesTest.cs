using System;

namespace HLE.UnitTests;

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
    {
        Assert.NotEqual(sizeof(long), sizeof(TestEnum));
        Assert.Throws<InvalidOperationException>(static () => _ = EnumValues<TestEnum>.AsSpan<long>());
    }

    [Fact]
    public void CountTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();
        int valueCount = EnumValues<TestEnum>.Count;
        Assert.Equal(actualValues.Length, valueCount);
    }
}
