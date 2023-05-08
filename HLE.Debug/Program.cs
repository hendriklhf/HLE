using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace HLE.Debug;

public static class Program
{
    private static void Main()
    {
    }
}

/*
ManualConfig config = new()
{
    SummaryStyle = new(default, true, SizeUnit.B, TimeUnit.GetBestTimeUnit())
};
config.AddLogger(ConsoleLogger.Default);
config.AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, BaselineRatioColumn.RatioMean, StatisticColumn.StdDev);
config.AddColumnProvider(DefaultColumnProviders.Metrics, DefaultColumnProviders.Params);
BenchmarkRunner.Run<Bench>(config);
*/

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Bench
{
}
