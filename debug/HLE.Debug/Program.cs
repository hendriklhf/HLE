using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

#pragma warning disable // RCS1060, S125, IDE0022, CA1515, CA1024, CA1822, IDE0032, CA1052, CA1707

namespace HLE.Debug;

internal static class Program
{
    private static void Main()
    {
    }
}

/*
ManualConfig config = new()
{
    SummaryStyle = new(null, true, SizeUnit.B, TimeUnit.GetBestTimeUnit())
};
config.AddLogger(ConsoleLogger.Default);
config.AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, BaselineRatioColumn.RatioMean, StatisticColumn.StdDev);
config.AddColumnProvider(DefaultColumnProviders.Metrics, DefaultColumnProviders.Params);
BenchmarkRunner.Run<Bench>(config);
*/

[MemoryDiagnoser]
[DisassemblyDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Bench;
