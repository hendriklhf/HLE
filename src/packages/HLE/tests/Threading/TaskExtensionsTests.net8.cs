using System;
using System.Threading;
using System.Threading.Tasks;
using HLE.Threading;

namespace HLE.UnitTests.Threading;

public sealed class TaskExtensionsTests
{
    private int _counter;

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task WhenAll(int taskCount)
    {
        Task[] tasks = new Task[taskCount];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = MyTask((i + 1) * 100);
        }

        await Task.WhenAll(tasks.AsSpan());

        Assert.Equal(tasks.Length, _counter);

        for (int i = 0; i < tasks.Length; i++)
        {
            Assert.True(tasks[i].IsCompletedSuccessfully);
        }
    }

    private async Task MyTask(int delay)
    {
        await Task.Delay(delay);
        Interlocked.Increment(ref _counter);
    }
}
