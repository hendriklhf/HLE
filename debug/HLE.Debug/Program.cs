#pragma warning disable

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

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

#if NET8_0_OR_GREATER
        Job net8Job = Job.Default.WithToolchain(CsProjCoreToolchain.NetCoreApp80);
        config.AddJob(net8Job);
#endif

#if NET9_0_OR_GREATER
        Job net9Job = Job.Default.WithToolchain(CsProjCoreToolchain.NetCoreApp90);
        config.AddJob(net9Job);
#endif

#if NET10_0_OR_GREATER
        NetCoreAppSettings net10Settings = new("net10.0", "10.0.0-preview.4.25258.110", ".NET 10");
        IToolchain net10ToolChain = CsProjCoreToolchain.From(net10Settings);
        Job net10Job = Job.Default.WithToolchain(net10ToolChain);
        config.AddJob(net10Job);
#endif

        config.AddLogger(ConsoleLogger.Default);
        config.AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, BaselineRatioColumn.RatioMean);
        config.AddColumnProvider(DefaultColumnProviders.Job, DefaultColumnProviders.Metrics, DefaultColumnProviders.Params);

        BenchmarkRunner.Run<Bench>(config);
*/

[MemoryDiagnoser]
[DisassemblyDiagnoser]
[HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictions, HardwareCounter.TotalCycles)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class Bench;
