using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HLE.RemoteExecution;
using HLE.TestUtilities;
using HLE.Text;

namespace HLE.UnitTests.Text;

public sealed class LazyStringMultiThreadingTest
{
    private readonly LazyString[] _lazyStrings = new LazyString[s_lazyStringCount];
    private volatile bool _running = true;

    private static readonly int s_lazyStringCount = Environment.ProcessorCount * 16;
    private static readonly int s_threadCount = int.Max(1, Environment.ProcessorCount / 2);
    private static readonly TimeSpan s_testRunDuration = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_joinTimeout = TimeSpan.FromSeconds(1);

    [Theory(Timeout = 10_000)]
    [MemberData(nameof(TheoryDataHelpers.ProcessorCountOptions), MemberType = typeof(TheoryDataHelpers))]
    public async Task IsThreadSafe(RemoteExecutorOptions options)
    {
        RemoteExecutorResult result = await RemoteExecutor.InvokeAsync(Remote_IsThreadSafe, options);
        Assert.RemoteExecutionSuccess(result);
    }

    private async Task Remote_IsThreadSafe()
    {
        Thread init = new(static state =>
        {
            LazyStringMultiThreadingTest test = Unsafe.As<LazyStringMultiThreadingTest>(state!);
            for (int i = 0; test._running; i++)
            {
                ref LazyString lzstr = ref test._lazyStrings[i % test._lazyStrings.Length];
                char[] chars = new char[16];
                Random.Shared.Fill(chars);
                lzstr = new(chars, chars.Length);
            }
        })
        {
            IsBackground = true
        };

        Thread[] threads = new Thread[s_threadCount];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new(static state =>
            {
                LazyStringMultiThreadingTest test = Unsafe.As<LazyStringMultiThreadingTest>(state!);
                for (int i = 0; test._running; i++)
                {
                    string s = test._lazyStrings[i % test._lazyStrings.Length].ToString();
                    Assert.NotNull(s);
                }
            })
            {
                IsBackground = true
            };
        }

        init.Start(this);
        await Task.Delay(100);

        foreach (Thread thread in threads)
        {
            thread.Start(this);
        }

        await Task.Delay(s_testRunDuration);
        _running = false;

        init.Join(s_joinTimeout);
        foreach (Thread thread in threads)
        {
            thread.Join(s_joinTimeout);
        }
    }
}
