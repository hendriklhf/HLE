using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests;

[TestClass]
public class NumberHelperTest
{
    [TestMethod]
    public void InsertKDotsTest()
    {
        const int number1 = 1234567890;
        Assert.AreEqual("1.234.567.890", NumberHelper.InsertKDots(number1));
        const int number2 = 123456789;
        Assert.AreEqual("123.456.789", NumberHelper.InsertKDots(number2));
        const int number3 = 12345678;
        Assert.AreEqual("12.345.678", NumberHelper.InsertKDots(number3));
        const int number4 = 1234567;
        Assert.AreEqual("1.234.567", NumberHelper.InsertKDots(number4));
        const int number5 = 123;
        Assert.AreEqual("123", NumberHelper.InsertKDots(number5));

        const int nnumber1 = -1234567890;
        Assert.AreEqual("-1.234.567.890", NumberHelper.InsertKDots(nnumber1));
        const int nnumber2 = -123456789;
        Assert.AreEqual("-123.456.789", NumberHelper.InsertKDots(nnumber2));
        const int nnumber3 = -12345678;
        Assert.AreEqual("-12.345.678", NumberHelper.InsertKDots(nnumber3));
        const int nnumber4 = -1234567;
        Assert.AreEqual("-1.234.567", NumberHelper.InsertKDots(nnumber4));
        const int nnumber5 = -123;
        Assert.AreEqual("-123", NumberHelper.InsertKDots(nnumber5));
    }

    [TestMethod]
    public void GetNumberLengthTest()
    {
        for (int i = 1; i <= 1000000000; i *= 10)
        {
            Assert.AreEqual(i.ToString().Length, NumberHelper.GetNumberLength(i));
        }
    }

    [TestMethod]
    public void NumberToDigitArrayTest()
    {
        Assert.IsTrue(NumberHelper.GetDigits(1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.IsTrue(NumberHelper.GetDigits(-1234567) is [1, 2, 3, 4, 5, 6, 7]);
        Assert.IsTrue(NumberHelper.GetDigits(0) is [0]);
        Assert.IsTrue(NumberHelper.GetDigits(1) is [1]);
    }

    [TestMethod]
    public void CharToDigitTest()
    {
        byte digit = 0;
        for (char c = '0'; c < '9' + 1; c++)
        {
            Assert.AreEqual(c, NumberHelper.DigitToChar(digit++));
        }
    }

    [TestMethod]
    public void DigitToCharTest()
    {
        char c = '0';
        for (byte i = 0; i < 10; i++)
        {
            Assert.AreEqual(i, NumberHelper.CharToDigit(c++));
        }
    }
}
