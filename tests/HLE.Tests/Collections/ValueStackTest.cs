using System;
using HLE.Collections;
using Xunit;

namespace HLE.Tests.Collections;

public sealed class ValueStackTest
{
    [Fact]
    public void PushTest()
    {
        ValueStack<int> stack = new(stackalloc int[5]);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        Assert.Equal(3, stack.Count);
    }

    [Fact]
    public void PopTest()
    {
        ValueStack<int> stack = new(stackalloc int[5]);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        Assert.Equal(3, stack.Pop());
        Assert.Equal(2, stack.Pop());
        Assert.Equal(1, stack.Pop());
        Assert.Equal(0, stack.Count);
    }

    [Fact]
    public void PeekTest()
    {
        ValueStack<int> stack = new(stackalloc int[5]);
        stack.Push(5);
        Assert.Equal(5, stack.Peek());
        Assert.Equal(1, stack.Count);
    }

    [Fact]
    public void ClearTest()
    {
        ValueStack<int> stack = new(stackalloc int[5]);
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        stack.Clear();
        Assert.Equal(0, stack.Count);
    }

    [Fact]
    public void PushAndPopTest()
    {
        ValueStack<int> stack = new(stackalloc int[50]);
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
        Assert.Equal(12345, stack.Pop());
        Assert.Equal(0, stack.Count);
    }
}
