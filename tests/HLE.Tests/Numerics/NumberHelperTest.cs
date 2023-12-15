using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using HLE.Numerics;
using Xunit;

namespace HLE.Tests.Numerics;

[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
public sealed class NumberHelperTest
{
    public static object[][] BringIntoRangeParameters { get; } =
    [
        [(byte)0, (byte)0],
        [(byte)0, (byte)64],
        [(byte)16, (byte)32],
        [(byte)0, byte.MaxValue],
        [(sbyte)0, (sbyte)0],
        [(sbyte)-10, (sbyte)10],
        [(sbyte)-64, (sbyte)32],
        [(sbyte)-64, (sbyte)0],
        [(sbyte)-120, (sbyte)120],
        [sbyte.MinValue, sbyte.MaxValue],
        [(ushort)0, (ushort)0],
        [(ushort)0, (ushort)1000],
        [(ushort)1000, (ushort)5000],
        [(ushort)0, ushort.MaxValue],
        [(short)0, (short)0],
        [(short)0, (short)512],
        [(short)64, (short)512],
        [(short)-1000, (short)512],
        [(short)-1000, (short)0],
        [(short)-30_000, (short)30_000],
        [short.MinValue, short.MaxValue],
        [0U, 0U],
        [0U, 2048U],
        [1024U, 8192U],
        [0, 0],
        [0, 64],
        [64, 512],
        [-64, 512],
        [-64, 0],
        [0UL, 0UL],
        [0UL, 128UL],
        [128UL, 2048UL],
        [0L, 0L],
        [0L, 64L],
        [-64L, 64L],
        [-64L, 0L],
        [(UInt128)0, (UInt128)0],
        [(UInt128)0, (UInt128)64],
        [(UInt128)64, (UInt128)128],
        [(Int128)0, (Int128)0],
        [(Int128)0, (Int128)64],
        [(Int128)(-64), (Int128)(-64)],
        [(Int128)(-64), (Int128)0]
    ];

    [Fact]
    public void GetNumberLengthTest()
    {
        int[] numbers = new int[10_000];
        Random.Shared.Fill(numbers.AsSpan(1));
        for (int i = 0; i < numbers.Length; i++)
        {
            int number = numbers[i];
            bool isNegative = number < 0;
            Assert.Equal(number.ToString().Length - Unsafe.As<bool, byte>(ref isNegative), NumberHelpers.GetNumberLength(number));
        }
    }

    [Fact]
    public void GetDigitsTest()
    {
        Assert.True(NumberHelpers.GetDigits(1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.True(NumberHelpers.GetDigits(-1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.True(NumberHelpers.GetDigits(0) is [0]);
        Assert.True(NumberHelpers.GetDigits(1) is [1]);
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(5, "5")]
    [InlineData(20, "20")]
    [InlineData(95972, "95972")]
    [InlineData(345347853, "345347853")]
    public void ParsePositiveNumberTest(int number, string text)
        => Assert.Equal(number, NumberHelpers.ParsePositiveNumber<int>(text));

    [Fact]
    public void ParsePositiveNumberFromBytesTest()
        => Assert.Equal(7334687, NumberHelpers.ParsePositiveNumber<int>("7334687"u8));

    [Theory]
    [MemberData(nameof(BringIntoRangeParameters))]
    public void BringIntoRangeTest(object min, object max)
    {
        MethodInfo bringIntoRangeCore = typeof(NumberHelperTest)
            .GetMethod(nameof(BringIntoRangeTestCore), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(min.GetType());

        Assert.Equal(min.GetType(), max.GetType());

        bringIntoRangeCore.Invoke(null, [min, max]);
    }

    private static void BringIntoRangeTestCore<T>(T min, T max) where T : INumber<T>, IMinMaxValue<T>
    {
        T start = T.Max(T.CreateSaturating(-1_000_000), T.MinValue);
        T end = T.Min(T.CreateSaturating(1_000_000), T.MaxValue);
        for (T i = start; i < end; i++)
        {
            T value = NumberHelpers.BringIntoRange(i, min, max);
            if (min == max)
            {
                Assert.Equal(value, min);
                Assert.Equal(value, max);
                continue;
            }

            Assert.True(value >= min && value < max);
        }
    }
}
