using System;
using System.Threading;
using System.Threading.Tasks;
using HLE.RemoteExecution;
using HLE.TestUtilities;
using HLE.Threading;

namespace HLE.UnitTests.Threading;

public sealed class EventInvokerTest(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    private static readonly TheoryData<int> s_targetCountParameters = TheoryDataHelpers.CreateRange(0, Environment.ProcessorCount * 2);

    public static TheoryData<(RemoteExecutorOptions, int)> ProcessorCountAndTargetCountMatrix { get; } =
        TheoryDataHelpers.CreateMatrix(TheoryDataHelpers.ProcessorCountOptions, s_targetCountParameters);

    private int _counter;

    [Theory(Timeout = 10_000)]
    [MemberData(nameof(ProcessorCountAndTargetCountMatrix))]
    public async Task InvokeAsync((RemoteExecutorOptions Options, int TargetCount) parameters)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_InvokeAsync, parameters.Options, parameters.TargetCount);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    [Theory(Timeout = 10_000)]
    [MemberData(nameof(ProcessorCountAndTargetCountMatrix))]
    public async Task QueueOnThreadPool((RemoteExecutorOptions Options, int TargetCount) parameters)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_QueueOnThreadPool, parameters.Options, parameters.TargetCount);
        Assert.RemoteExecutionSuccess(result, _output);
    }

    private async Task Remote_InvokeAsync(int targetCount)
    {
        AsyncEventHandler<EventInvokerTest, string>? eventHandler = null;
        for (int i = 0; i < targetCount; i++)
        {
            eventHandler += OnSomethingAsync;
        }

        await EventInvoker.InvokeAsync(eventHandler, this, "hello");

        int invocationListLength = eventHandler?.GetInvocationList().Length ?? 0;
        Assert.Equal(targetCount, invocationListLength);
        Assert.Equal(invocationListLength, _counter);
    }

    private void Remote_QueueOnThreadPool(int targetCount)
    {
        EventHandler<string>? eventHandler = null;
        for (int i = 0; i < targetCount; i++)
        {
            eventHandler += OnSomething;
        }

        EventInvoker.QueueOnThreadPool(eventHandler, this, "hello");

        SpinWait spinWait = new();
        while (Volatile.Read(ref _counter) < targetCount)
        {
            spinWait.SpinOnce();
        }

        int invocationListLength = eventHandler?.GetInvocationList().Length ?? 0;
        Assert.Equal(targetCount, invocationListLength);
        Assert.Equal(invocationListLength, _counter);
    }

    private Task OnSomethingAsync(EventInvokerTest sender, string args, CancellationToken cancellationToken)
    {
        Assert.Same(this, sender);
        Assert.Same("hello", args);
        Interlocked.Increment(ref _counter);
        return Task.CompletedTask;
    }

    private void OnSomething(object? sender, string args)
    {
        Assert.Same(this, sender);
        Assert.Same("hello", args);
        Interlocked.Increment(ref _counter);
    }
}
