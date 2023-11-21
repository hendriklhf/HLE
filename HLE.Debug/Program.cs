using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using HLE.Marshalling;
using HLE.Memory;
using Perfolizer.Horology;

namespace HLE.Debug;

// TODO: check Pure attribute on public non-void methods: public\s+(\w+(<.*>)?\s+)*[^v]\w+(<.*>)?\s+\w+<.*>\(
// TODO: more types to cache computation
// TODO: ICollection<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ... implementation for more types
// TODO: non-synchronized ArrayPool
// TODO: improve StringArray.MoveItem
// TODO: add CollectionBuilder
// TODO: swap Semaphore for Monitor

[SuppressMessage("Style", "IDE0022:Use expression body for method")]
public static class Program
{
    private static void Main()
    {
        ManualConfig config = new()
        {
            SummaryStyle = new(default, true, SizeUnit.B, TimeUnit.GetBestTimeUnit())
        };
        config.AddLogger(ConsoleLogger.Default);
        config.AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean, BaselineRatioColumn.RatioMean, StatisticColumn.StdDev);
        config.AddColumnProvider(DefaultColumnProviders.Metrics, DefaultColumnProviders.Params);
        BenchmarkRunner.Run<Bench>(config);
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
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
[SuppressMessage("ReSharper", "ClassCanBeSealed.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
#pragma warning disable CA1001
#pragma warning disable CA1707
public class Bench
{
    [Benchmark]
    [Arguments(0)]
    [Arguments(50)]
    public nuint GetRawStringSize(int length) => RawDataMarshal.GetRawStringSize(length);

    [Benchmark]
    []
    public ref RawStringData GetRawStringData(string str) => ref RawDataMarshal.GetRawStringData(str);
}
