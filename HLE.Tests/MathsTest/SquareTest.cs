using System;
using HLE.Maths;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.MathsTest;

[TestClass]
public class SquareTest
{
    [TestMethod]
    public void GeneralSquareTest()
    {
        const double length = 5;
        Square sq = new(length);
        Assert.AreEqual(25, sq.Area);
        Assert.AreEqual(20, sq.Circumference);
        Assert.AreEqual(7.071, Math.Round(sq.Diagonal, 3));
        Assert.AreEqual(length, sq.SideLength);
    }
}
