using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace HLE.Debug;

public static class Program
{
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "ClassCanBeSealed.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Bench
{
}
