using System;
using HLE.Collections;
using Xunit;

namespace HLE.Tests.Collections;

public sealed class ValueQueueTest
{
    [Fact]
    public void EnqueueTest()
    {
        ValueQueue<int> queue = new(stackalloc int[5]);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.Equal(3, queue.Count);
    }

    [Fact]
    public void DequeueTest()
    {
        ValueQueue<int> queue = new(stackalloc int[5]);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Enqueue(5);

        Assert.Equal(1, queue.Dequeue());
        Assert.Equal(2, queue.Dequeue());
        Assert.Equal(3, queue.Dequeue());
        Assert.Equal(4, queue.Dequeue());
        Assert.Equal(5, queue.Dequeue());
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void PeekTest()
    {
        ValueQueue<int> queue = new(stackalloc int[1]);
        queue.Enqueue(1);

        Assert.Equal(1, queue.Peek());
        Assert.Equal(1, queue.Count);
    }

    [Fact]
    public void ClearTest()
    {
        ValueQueue<int> queue = new(stackalloc int[5]);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Enqueue(5);
        queue.Clear();
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void EnqueueAndDequeueTest()
    {
        ValueQueue<int> queue = new(stackalloc int[50]);
        for (int i = 0; i < 10_000; i++)
        {
            int count = Random.Shared.Next(0, 50);
            for (int j = 0; j < count; j++)
            {
                queue.Enqueue(Random.Shared.Next());
            }

            for (int j = 0; j < count; j++)
            {
                queue.Dequeue();
            }
        }

        queue.Enqueue(12345);
        Assert.Equal(12345, queue.Dequeue());
        Assert.Equal(0, queue.Count);
    }
}
