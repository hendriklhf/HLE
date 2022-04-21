using System.Linq;
using HLE.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.TimeTests;

[TestClass]
public class TimeHelperTests
{
    [TestMethod]
    public void GetUnixDifferenceTest()
    {
        byte[] counts =
        {
            2,
            5,
            10,
            45,
            20
        };

        long[] times =
        {
            new Year(counts[0]).Milliseconds,
            new Day(counts[1]).Milliseconds,
            new Hour(counts[2]).Milliseconds,
            new Minute(counts[3]).Milliseconds,
            new Second(counts[4]).Milliseconds
        };

        UnixDiffSpan d = TimeHelper.GetUnixDifference(TimeHelper.Now() + times.Sum() + 250);
        Assert.AreEqual(counts[0], d.Years);
        Assert.AreEqual(counts[1], d.Days);
        Assert.AreEqual(counts[2], d.Hours);
        Assert.AreEqual(counts[3], d.Minutes);
        Assert.AreEqual(counts[4], d.Seconds);

        d = TimeHelper.GetUnixDifference(TimeHelper.Now() - times.Sum() - 250);
        Assert.AreEqual(counts[0], d.Years);
        Assert.AreEqual(counts[1], d.Days);
        Assert.AreEqual(counts[2], d.Hours);
        Assert.AreEqual(counts[3], d.Minutes);
        Assert.AreEqual(counts[4], d.Seconds);

        d = TimeHelper.GetUnixDifference(TimeHelper.Now() + times.Sum() + 750);
        Assert.AreEqual(counts[0], d.Years);
        Assert.AreEqual(counts[1], d.Days);
        Assert.AreEqual(counts[2], d.Hours);
        Assert.AreEqual(counts[3], d.Minutes);
        Assert.AreEqual(counts[4] + 1, d.Seconds);

        d = TimeHelper.GetUnixDifference(TimeHelper.Now() - times.Sum() - 750);
        Assert.AreEqual(counts[0], d.Years);
        Assert.AreEqual(counts[1], d.Days);
        Assert.AreEqual(counts[2], d.Hours);
        Assert.AreEqual(counts[3], d.Minutes);
        Assert.AreEqual(counts[4] + 1, d.Seconds);
    }
}
