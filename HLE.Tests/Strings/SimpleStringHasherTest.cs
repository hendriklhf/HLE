using System;
using System.Linq;
using HLE.Strings;
using Xunit;
using Xunit.Abstractions;

namespace HLE.Tests.Strings;

public sealed class SimpleStringHasherTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Theory]
    [InlineData((char)0, (char)256)]
    [InlineData((char)0, (char)1024)]
    [InlineData(char.MinValue, char.MaxValue)]
    public void HashDistributionTest(char min, char max)
    {
        const int BucketCount = 256;
        const int LoopIterations = 4096 << 4;
        int[] counts = new int[BucketCount];
        for (int i = 0; i < LoopIterations; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(10, 1000), min, max);
            SimpleStringHasher hasher = new(str);
            uint hash = hasher.Hash();
            int index = (int)(hash % BucketCount);
            counts[index]++;
        }

        const int Average = LoopIterations / BucketCount;
        _testOutputHelper.WriteLine($"Average: {Average}");
        int minCount = counts.Min();
        _testOutputHelper.WriteLine($"Minimum: {minCount}");
        int maxCount = counts.Max();
        _testOutputHelper.WriteLine($"Maximum: {maxCount}");

        int lessThanAverageCount = counts.Count(static c => c < Average);
        int greaterThanAverageCount = counts.Count(static c => c >= Average);
        _testOutputHelper.WriteLine($"Less than average: {lessThanAverageCount}");
        _testOutputHelper.WriteLine($"Greater than average: {greaterThanAverageCount}");

        Assert.True(counts.All(static c => c > Average * 0.125));
        Assert.True(Math.Abs(greaterThanAverageCount - lessThanAverageCount) < Average * 0.075);
    }
}
