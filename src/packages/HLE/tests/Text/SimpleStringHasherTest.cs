using System;
using System.Linq;
using HLE.Text;

namespace HLE.UnitTests.Text;

public sealed class SimpleStringHasherTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Theory]
    [InlineData((char)0, (char)128)]
    [InlineData((char)0, (char)256)]
    [InlineData((char)0, (char)1024)]
    [InlineData(char.MinValue, char.MaxValue)]
    public void HashDistributionTest(char min, char max)
    {
        const int BucketCount = 256;
        const int LoopIterations = 65536;

        Random rng = new(285);
        int[] counts = new int[BucketCount];
        for (int i = 0; i < LoopIterations; i++)
        {
            string str = rng.NextString(rng.Next(10, 1000), min, max);
            uint hash = SimpleStringHasher.Hash(str);
            int index = (int)(hash % BucketCount);
            counts[index]++;
        }

        int minCount = counts.Min();
        int maxCount = counts.Max();

        const int Average = LoopIterations / BucketCount;
        _output.WriteLine($"Average: {Average}");
        _output.WriteLine($"Minimum: {minCount}");
        _output.WriteLine($"Maximum: {maxCount}");

        int diff = Math.Abs(minCount - maxCount);
        Assert.True(diff < minCount * 0.5);
        Assert.True(diff < maxCount * 0.5);

        int lessThanAverageCount = counts.Count(static c => c < Average);
        int greaterThanAverageCount = counts.Count(static c => c >= Average);

        _output.WriteLine($"Less than average: {lessThanAverageCount}");
        _output.WriteLine($"Greater than average: {greaterThanAverageCount}");

        diff = Math.Abs(lessThanAverageCount - greaterThanAverageCount);
        Assert.True(diff < lessThanAverageCount * 0.2);
        Assert.True(diff < greaterThanAverageCount * 0.2);

        _output.WriteLine($"{Environment.NewLine}Graph:");
        for (int i = 0; i < counts.Length; i++)
        {
            _output.WriteLine($"{i:0000}: {new('*', counts[i] / 10)}");
            Assert.True(counts[i] > Average * 0.75);
        }
    }
}
