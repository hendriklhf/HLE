using System.Collections.Generic;
using HLE.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Numerics;

[TestClass]
public class NumberHelperTest
{
    [TestMethod]
    public void InsertThousandsSeparatorTest()
    {
        const int number1 = 1234567890;
        Assert.AreEqual("1.234.567.890", NumberHelper.InsertThousandSeparators(number1));
        const int number2 = 123456789;
        Assert.AreEqual("123.456.789", NumberHelper.InsertThousandSeparators(number2));
        const int number3 = 12345678;
        Assert.AreEqual("12.345.678", NumberHelper.InsertThousandSeparators(number3));
        const int number4 = 1234567;
        Assert.AreEqual("1.234.567", NumberHelper.InsertThousandSeparators(number4));
        const int number5 = 123;
        Assert.AreEqual("123", NumberHelper.InsertThousandSeparators(number5));

        const int nnumber1 = -1234567890;
        Assert.AreEqual("-1.234.567.890", NumberHelper.InsertThousandSeparators(nnumber1));
        const int nnumber2 = -123456789;
        Assert.AreEqual("-123.456.789", NumberHelper.InsertThousandSeparators(nnumber2));
        const int nnumber3 = -12345678;
        Assert.AreEqual("-12.345.678", NumberHelper.InsertThousandSeparators(nnumber3));
        const int nnumber4 = -1234567;
        Assert.AreEqual("-1.234.567", NumberHelper.InsertThousandSeparators(nnumber4));
        const int nnumber5 = -123;
        Assert.AreEqual("-123", NumberHelper.InsertThousandSeparators(nnumber5));
    }

    [TestMethod]
    public void GetNumberLengthTest()
    {
        for (int i = 0; i <= 10_000_000; i++)
        {
            Assert.AreEqual(i.ToString().Length, NumberHelper.GetNumberLength(i));
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
    public void IsOnlyOneBitSetTest()
    {
        HashSet<int> numbersWithOnlyBitSet = new(32);
        for (int i = 0; i <= 32; i++)
        {
            numbersWithOnlyBitSet.Add(1 << i);
        }

        for (int i = 0; i < int.MaxValue; i++)
        {
            if (numbersWithOnlyBitSet.Contains(i))
            {
                Assert.IsTrue(NumberHelper.IsOnlyOneBitSet(i));
            }
            else
            {
                Assert.IsFalse(NumberHelper.IsOnlyOneBitSet(i));
            }
        }
    }
}
