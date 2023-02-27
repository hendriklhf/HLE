using System;
using BenchmarkDotNet.Attributes;
using HLE.Twitch;
using HLE.Twitch.Chatterino;

namespace HLE.Debug;

public static class Program
{
    private static void Main()
    {
        using TwitchClient client = new();
        client.JoinChannels(ChatterinoHelper.GetChannels());
        client.OnDataReceived += (_, d) =>
        {
            Console.WriteLine(new string(d.Span));
            d.Dispose();
        };
        client.Connect();
        Console.ReadKey();
    }
}

/*
ManualConfig config = new()
{
    SummaryStyle = new(default, true, SizeUnit.B, TimeUnit.GetBestTimeUnit())
};
config.AddLogger(ConsoleLogger.Default);
config.AddColumn(TargetMethodColumn.Method, StatisticColumn.Mean);
config.AddColumnProvider(DefaultColumnProviders.Metrics, DefaultColumnProviders.Params);
BenchmarkRunner.Run<Bench>(config);
*/

[MemoryDiagnoser]
public class Bench
{
}
