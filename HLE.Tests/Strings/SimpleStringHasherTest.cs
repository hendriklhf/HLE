using System;
using System.Linq;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public sealed class SimpleStringHasherTest
{
    [TestMethod]
    [DataRow((char)0, (char)256)]
    [DataRow((char)0, (char)1024)]
    [DataRow(char.MinValue, char.MaxValue)]
    public void HashDistributionTest(char min, char max)
    {
        const int bucketCount = 256;
        const int loopIterations = 4096 << 4;
        int[] counts = new int[bucketCount];
        for (int i = 0; i < loopIterations; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(10, 1000), min, max);
            SimpleStringHasher hasher = new(str);
            int hash = hasher.Hash();
            int index = (int)((uint)hash % bucketCount);
            counts[index]++;
        }

        const int average = loopIterations / bucketCount;
        Console.WriteLine($"Average: {average}");
        int minCount = counts.Min();
        Console.WriteLine($"Minimum: {minCount}");
        int maxCount = counts.Max();
        Console.WriteLine($"Maximum: {maxCount}");

        int lessThanAverageCount = counts.Count(static c => c < average);
        int greaterThanAverageCount = counts.Count(static c => c >= average);
        Console.WriteLine($"Less than average: {lessThanAverageCount}");
        Console.WriteLine($"Greater than average: {greaterThanAverageCount}");

        Assert.IsTrue(counts.All(static c => c > average * 0.125));
        Assert.IsTrue(Math.Abs(greaterThanAverageCount - lessThanAverageCount) < average * 0.075);
    }
}
