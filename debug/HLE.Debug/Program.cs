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
    SummaryStyle = new(null, true, SizeUnit.GetBestSizeUnit(), TimeUnit.GetBestTimeUnit())
};

Job net9Job = Job.Default.With(CsProjCoreToolchain.NetCoreApp90);
config.AddJob(net9Job);

NetCoreAppSettings net10Settings = new("net10.0", "10.0.0-preview.1.25080.5", ".NET 10");
IToolchain net10ToolChain = CsProjCoreToolchain.From(net10Settings);
Job net10Job = Job.Default.With(net10ToolChain);
config.AddJob(net10Job);

config.AddLogger(ConsoleLogger.Default);
config.AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, BaselineRatioColumn.RatioMean, StatisticColumn.StdDev);
config.AddColumnProvider(DefaultColumnProviders.Job, DefaultColumnProviders.Metrics, DefaultColumnProviders.Params);
BenchmarkRunner.Run<Bench>(config);
*/

[MemoryDiagnoser]
[DisassemblyDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Bench;
