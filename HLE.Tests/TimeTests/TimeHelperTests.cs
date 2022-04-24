using System;
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
            (long)TimeSpan.FromDays(counts[0] * 365).TotalMilliseconds,
            (long)TimeSpan.FromDays(counts[1]).TotalMilliseconds,
            (long)TimeSpan.FromHours(counts[2]).TotalMilliseconds,
            (long)TimeSpan.FromMinutes(counts[3]).TotalMilliseconds,
            (long)TimeSpan.FromSeconds(counts[4]).TotalMilliseconds
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
