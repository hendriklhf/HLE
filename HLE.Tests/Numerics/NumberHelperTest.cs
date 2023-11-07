using System;
using System.Runtime.CompilerServices;
using HLE.Numerics;
using Xunit;

namespace HLE.Tests.Numerics;

public sealed class NumberHelperTest
{
    [Fact]
    public void GetNumberLengthTest()
    {
        int[] numbers = new int[10_000];
        Random.Shared.Fill(numbers.AsSpan(1));
        for (int i = 0; i < numbers.Length; i++)
        {
            int number = numbers[i];
            bool isNegative = number < 0;
            Assert.Equal(number.ToString().Length - Unsafe.As<bool, byte>(ref isNegative), NumberHelper.GetNumberLength(number));
        }
    }

    [Fact]
    public void GetDigitsTest()
    {
        Assert.True(NumberHelper.GetDigits(1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.True(NumberHelper.GetDigits(-1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.True(NumberHelper.GetDigits(0) is [0]);
        Assert.True(NumberHelper.GetDigits(1) is [1]);
    }

    [Fact]
    public void DigitToCharTest()
    {
        byte digit = 0;
        for (char c = '0'; c < '9' + 1; c++)
        {
            Assert.Equal(c, NumberHelper.DigitToChar(digit++));
        }
    }

    [Fact]
    public void CharToDigitTest()
    {
        char c = '0';
        for (byte i = 0; i < 10; i++)
        {
            Assert.Equal(i, NumberHelper.CharToDigit(c++));
        }
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(5, "5")]
    [InlineData(20, "20")]
    [InlineData(95972, "95972")]
    [InlineData(345347853, "345347853")]
    public void ParsePositiveNumberTest(int number, string text)
        => Assert.Equal(number, NumberHelper.ParsePositiveNumber<int>(text));

    [Fact]
    public void ParsePositiveNumberFromBytesTest()
        => Assert.Equal(7334687, NumberHelper.ParsePositiveNumber<int>("7334687"u8));

    [Fact]
    public void BringNumberIntoRangeTest()
    {
        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.True(NumberHelper.BringNumberIntoRange(i, 0, 500) is >= 0 and < 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.True(NumberHelper.BringNumberIntoRange(i, -500, 0) is >= -500 and < 0);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.True(NumberHelper.BringNumberIntoRange(i, -500, 500) is >= -500 and < 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.True(NumberHelper.BringNumberIntoRange(i, 0, 0) == 0);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.True(NumberHelper.BringNumberIntoRange(i, 500, 500) == 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.True(NumberHelper.BringNumberIntoRange(i, 500, 501) == 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.True(NumberHelper.BringNumberIntoRange(i, 500, 502) is >= 500 and < 502);
        }
    }
}
