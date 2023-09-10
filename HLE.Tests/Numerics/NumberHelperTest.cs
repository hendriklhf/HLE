using System;
using System.Runtime.CompilerServices;
using HLE.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Numerics;

[TestClass]
public class NumberHelperTest
{
    [TestMethod]
    public void GetNumberLengthTest()
    {
        int[] numbers = new int[10_000];
        Random.Shared.Fill(numbers.AsSpan(1));
        for (int i = 0; i < numbers.Length; i++)
        {
            int number = numbers[i];
            bool isNegative = number < 0;
            Assert.AreEqual(number.ToString().Length - Unsafe.As<bool, byte>(ref isNegative), NumberHelper.GetNumberLength(number));
        }
    }

    [TestMethod]
    public void GetDigitsTest()
    {
        Assert.IsTrue(NumberHelper.GetDigits(1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.IsTrue(NumberHelper.GetDigits(-1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.IsTrue(NumberHelper.GetDigits(0) is [0]);
        Assert.IsTrue(NumberHelper.GetDigits(1) is [1]);
    }

    [TestMethod]
    public void DigitToCharTest()
    {
        byte digit = 0;
        for (char c = '0'; c < '9' + 1; c++)
        {
            Assert.AreEqual(c, NumberHelper.DigitToChar(digit++));
        }
    }

    [TestMethod]
    public void CharToDigitTest()
    {
        char c = '0';
        for (byte i = 0; i < 10; i++)
        {
            Assert.AreEqual(i, NumberHelper.CharToDigit(c++));
        }
    }

    [TestMethod]
    public void ParsePositiveNumberTest()
    {
        Assert.AreEqual(7334687, NumberHelper.ParsePositiveNumber<int>("7334687"));
    }

    [TestMethod]
    public void ParsePositiveNumberFromBytesTest()
    {
        Assert.AreEqual(7334687, NumberHelper.ParsePositiveNumber<int>("7334687"u8));
    }

    [TestMethod]
    public void BringNumberIntoRangeTest()
    {
        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.IsTrue(NumberHelper.BringNumberIntoRange(i, 0, 500) is >= 0 and < 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.IsTrue(NumberHelper.BringNumberIntoRange(i, -500, 0) is >= -500 and < 0);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.IsTrue(NumberHelper.BringNumberIntoRange(i, -500, 500) is >= -500 and < 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.IsTrue(NumberHelper.BringNumberIntoRange(i, 0, 0) == 0);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.IsTrue(NumberHelper.BringNumberIntoRange(i, 500, 500) == 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.IsTrue(NumberHelper.BringNumberIntoRange(i, 500, 501) == 500);
        }

        for (int i = -100_000; i < 100_000; i++)
        {
            Assert.IsTrue(NumberHelper.BringNumberIntoRange(i, 500, 502) is >= 500 and < 502);
        }
    }
}
