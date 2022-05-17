using System;
using HLE.Maths;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.MathsTest;

[TestClass]
public class RectangleTest
{
    [TestMethod]
    public void GeneralRectangleTest()
    {
        double width = 10;
        double height = 5;
        Rectangle rec = new(width, height);
        Assert.AreEqual(50, rec.Area);
        Assert.AreEqual(30, rec.Circumference);
        Assert.AreEqual(11.18, Math.Round(rec.Diagonals, 2));
        Assert.AreEqual(height, rec.Height);
        Assert.AreEqual(width, rec.Width);
    }
}
