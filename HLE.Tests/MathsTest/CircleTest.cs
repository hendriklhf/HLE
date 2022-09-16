using System;
using HLE.Maths;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.MathsTest;

[TestClass]
public class CircleTest
{
    [TestMethod]
    public void GeneralCircleTest()
    {
        const double radius = 2;
        Circle circle = new(radius);
        Assert.AreEqual(12.566, Math.Round(circle.Area, 3));
        Assert.AreEqual(12.566, Math.Round(circle.Circumference, 3));
        Assert.AreEqual(radius * 2, circle.Diameter);
        Assert.AreEqual(radius, circle.Radius);
    }
}
