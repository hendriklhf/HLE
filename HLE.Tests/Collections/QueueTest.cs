using System;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Collections;

[TestClass]
public class QueueTest
{
    [TestMethod]
    public void EnqueueTest()
    {
        ValueQueue<int> queue = stackalloc int[5];
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.AreEqual(3, queue.Count);
    }

    [TestMethod]
    public void DequeueTest()
    {
        ValueQueue<int> queue = stackalloc int[5];
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Enqueue(5);

        Assert.AreEqual(1, queue.Dequeue());
        Assert.AreEqual(2, queue.Dequeue());
        Assert.AreEqual(3, queue.Dequeue());
        Assert.AreEqual(4, queue.Dequeue());
        Assert.AreEqual(5, queue.Dequeue());
        Assert.AreEqual(0, queue.Count);
    }

    [TestMethod]
    public void PeekTest()
    {
        ValueQueue<int> queue = stackalloc int[1];
        queue.Enqueue(1);

        Assert.AreEqual(1, queue.Peek());
        Assert.AreEqual(1, queue.Count);
    }

    [TestMethod]
    public void ClearTest()
    {
        ValueQueue<int> queue = stackalloc int[5];
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Enqueue(5);
        queue.Clear();
        Assert.AreEqual(0, queue.Count);
    }

    [TestMethod]
    public void EnqueueAndDequeueTest()
    {
        ValueQueue<int> queue = stackalloc int[50];
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
        Assert.AreEqual(12345, queue.Dequeue());
        Assert.AreEqual(0, queue.Count);
    }
}
