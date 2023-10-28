using System;
using HLE.Collections.Concurrent;
using Xunit;

namespace HLE.Tests.Collections.Concurrent;

public sealed class ConcurrentPoolBufferListTest
{
    [Fact]
    public void IndexerTest()
    {
        using ConcurrentPooledList<char> list = [.. "helXo".AsSpan()];
        Assert.Equal('h', list[0]);
        Assert.Equal('o', list[4]);
        Assert.Equal('X', list[^2]);
        list[0] = 'H';
        Assert.Equal('H', list[0]);
    }

    [Fact]
    public void CountTest()
    {
        using ConcurrentPooledList<char> list = [.. "hello".AsSpan()];
        Assert.Equal(5, list.Count);
    }

    [Fact]
    public void AddTest()
    {
        using ConcurrentPooledList<char> list = [];
        for (int i = 0; i < 1000; i++)
        {
            list.Add('x');
        }

        Assert.Equal(1000, list.Count);
        Assert.True(list is ['x', .., 'x']);
    }

    [Fact]
    public void ClearTest()
    {
        using ConcurrentPooledList<char> list = [];
        for (int i = 0; i < 1000; i++)
        {
            list.Add('x');
        }

        list.Clear();
        Assert.Empty(list);
    }

    [Fact]
    public void RemoveTest()
    {
        using ConcurrentPooledList<char> list = [.. "hello".AsSpan()];
        list.Remove('l');
        Assert.True(list._list is ['h', 'e', 'l', 'o']);
    }

    [Fact]
    public void RemoveAtTest()
    {
        using ConcurrentPooledList<char> list = [.. "hello".AsSpan()];
        list.RemoveAt(2);
        Assert.Equal("helo", new(list._list.AsSpan()));
    }

    [Fact]
    public void InsertTest()
    {
        using ConcurrentPooledList<char> list = [.. "helo".AsSpan()];
        list.Insert(2, 'l');
        Assert.Equal("hello", new(list._list.AsSpan()));
    }

    [Fact]
    public void EnumeratorTest()
    {
        using ConcurrentPooledList<char> list = [.. "hello".AsSpan()];
        Span<char> result = stackalloc char[5];
        int resultLength = 0;
        foreach (char c in list)
        {
            result[resultLength++] = c;
        }

        Assert.Equal("hello", new(result));
    }

    [Fact]
    public void AddRemoveLoopTest()
    {
        using ConcurrentPooledList<char> list = [];
        for (int i = 0; i < 100_000; i++)
        {
            list.Add('x');
            list.Remove('x');
        }

        Assert.Empty(list);
    }

    [Fact]
    public void AddLoopTest()
    {
        using ConcurrentPooledList<char> list = [];
        for (int i = 0; i < 100_000; i++)
        {
            list.Add('x');
        }

        Assert.Equal(100_000, list.Count);
        Assert.True(list is ['x', .., 'x']);
    }
}
