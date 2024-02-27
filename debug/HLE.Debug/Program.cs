using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace HLE.Debug;

[SuppressMessage("Style", "IDE0022:Use expression body for method")]
internal static class Program
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
[DisassemblyDiagnoser]
[Orderer(SummaryOrderPolicy.SlowestToFastest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "ClassCanBeSealed.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("Design", "CA1024:Use properties where appropriate")]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")]
#pragma warning disable CA1001, CA1707
public class Bench;
