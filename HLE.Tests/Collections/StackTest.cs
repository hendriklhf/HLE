using System;
using HLE.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Collections;

[TestClass]
public class StackTest
{
    [TestMethod]
    public void PushTest()
    {
        ValueStack<int> stack = stackalloc int[5];
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        Assert.AreEqual(3, stack.Count);
    }

    [TestMethod]
    public void PopTest()
    {
        ValueStack<int> stack = stackalloc int[5];
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        Assert.AreEqual(3, stack.Pop());
        Assert.AreEqual(2, stack.Pop());
        Assert.AreEqual(1, stack.Pop());
        Assert.AreEqual(0, stack.Count);
    }

    [TestMethod]
    public void PeekTest()
    {
        ValueStack<int> stack = stackalloc int[5];
        stack.Push(5);
        Assert.AreEqual(5, stack.Peek());
        Assert.AreEqual(1, stack.Count);
    }

    [TestMethod]
    public void ClearTest()
    {
        ValueStack<int> stack = stackalloc int[5];
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Clear();
        Assert.AreEqual(0, stack.Count);
    }

    [TestMethod]
    public void PushAndPopTest()
    {
        ValueStack<int> stack = stackalloc int[50];
        for (int i = 0; i < 10_000; i++)
        {
            int count = Random.Shared.Next(0, 50);
            for (int j = 0; j < count; j++)
            {
                stack.Push(Random.Shared.Next());
            }

            for (int j = 0; j < count; j++)
            {
                stack.Pop();
            }
        }

        stack.Push(12345);
        Assert.AreEqual(12345, stack.Pop());
        Assert.AreEqual(0, stack.Count);
    }
}
