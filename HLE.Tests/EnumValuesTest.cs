using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

[TestClass]
public class EnumValuesTest
{
    [TestMethod]
    public void GetValuesTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();

        ReadOnlySpan<TestEnum> values = EnumValues.GetValues<TestEnum>();
        Assert.IsTrue(values.SequenceEqual(actualValues));

        values = EnumValues.GetValues<TestEnum>();
        Assert.IsTrue(values.SequenceEqual(actualValues));
    }

    [TestMethod]
    public void GetValuesAsUnderlyingType()
    {
        int[] actualValues = (int[])Enum.GetValuesAsUnderlyingType<TestEnum>();

        ReadOnlySpan<int> values = EnumValues.GetValuesAsUnderlyingType<TestEnum, int>();
        Assert.IsTrue(values.SequenceEqual(actualValues));

        values = EnumValues.GetValuesAsUnderlyingType<TestEnum, int>();
        Assert.IsTrue(values.SequenceEqual(actualValues));

        Span<int> sortedValues = values.ToArray().AsSpan();
        sortedValues.Sort();
        Assert.IsTrue(sortedValues.SequenceEqual(values));
        Assert.IsTrue(sortedValues.SequenceEqual(actualValues));
    }

    [TestMethod]
    public void GetValueCountTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();
        int valueCount = EnumValues.GetValueCount<TestEnum>();
        Assert.AreEqual(actualValues.Length, valueCount);
    }

    [TestMethod]
    public void GetMaxValueTest()
    {
        TestEnum[] actualValues = Enum.GetValues<TestEnum>();
        TestEnum maxValue = EnumValues.GetMaxValue<TestEnum>();
        Assert.AreEqual(12345, (int)maxValue);
        Assert.AreEqual(actualValues[^1], maxValue);
    }
}
