using System;
using System.Collections.Generic;
using System.Linq;
using HLE.Collections;
using Xunit;

namespace HLE.Tests.Collections;

public sealed partial class CollectionHelperTest
{
    [Fact]
    public void RandomTest()
    {
        string[] array = Enumerable.Range(0, 1000).Select(static _ => Random.Shared.NextString(25)).ToArray();
        for (int i = 0; i < 1000; i++)
        {
            Assert.Contains(Random.Shared.GetItem(array), array);
        }
    }

    [Fact]
    public void ReplaceTest()
    {
        int[] arr = [1, 2, 3, 2, 5, 2, 2, 2, 3, 3];
        arr.Replace(WhereIsTwo, 4);
        Assert.Equal(5, arr.Count(static i => i == 4));

        static bool WhereIsTwo(int i) => i == 2;
    }

    [Fact]
    public void IndicesOfTest()
    {
        const string str = "test string";
        int[] indices = str.IndicesOf(static c => c is 's');
        Assert.True(indices is [2, 5]);
    }

    [Fact]
    public void RangeEnumeratorTest()
    {
        List<int> items = new(101);
        Range r = ..100;
        foreach (int i in r)
        {
            items.Add(i);
        }

        Assert.Equal(101, items.Count);
        Assert.Equal(0, items[0]);
        Assert.Equal(100, items[^1]);

        items.Clear();
        r = 50..100;
        foreach (int i in r)
        {
            items.Add(i);
        }

        Assert.Equal(51, items.Count);
        Assert.Equal(50, items[0]);
        Assert.Equal(100, items[^1]);

        Assert.Throws<InvalidOperationException>(static () =>
        {
            foreach (int _ in ..^100)
            {
            }
        });

        Assert.Throws<InvalidOperationException>(static () =>
        {
            foreach (int _ in 50..)
            {
            }
        });
    }

    [Fact]
    public void TryGetReadOnlySpanTest()
    {
        IEnumerable<int> array = new[] { 0, 1, 2, 3, 4 };
        IEnumerable<int> list = new List<int> { 0, 1, 2, 3, 4 };
        IEnumerable<char> str = "hello";
        IEnumerable<int> enumerable = Enumerable.Range(0, 5).Select(static _ => Random.Shared.Next()).Where(static i => i > 0);

        bool succeeded = array.TryGetReadOnlySpan<int>(out ReadOnlySpan<int> arraySpan);
        Assert.True(succeeded && arraySpan is [0, 1, 2, 3, 4]);

        succeeded = list.TryGetReadOnlySpan<int>(out ReadOnlySpan<int> listSpan);
        Assert.True(succeeded && listSpan is [0, 1, 2, 3, 4]);

        succeeded = str.TryGetReadOnlySpan<char>(out ReadOnlySpan<char> stringSpan);
        Assert.True(succeeded && stringSpan is "hello");

        succeeded = enumerable.TryGetReadOnlySpan<int>(out ReadOnlySpan<int> enumerableSpan);
        Assert.False(succeeded && enumerableSpan.Length == 0);
    }

    [Fact]
    public void TryGetReadOnlyMemoryTest()
    {
        IEnumerable<int> array = new[] { 0, 1, 2, 3, 4 };
        IEnumerable<int> list = new List<int> { 0, 1, 2, 3, 4 };
        IEnumerable<char> str = "hello";
        IEnumerable<int> enumerable = Enumerable.Range(0, 5).Select(static _ => Random.Shared.Next()).Where(static i => i > 0);

        bool succeeded = array.TryGetReadOnlyMemory<int>(out ReadOnlyMemory<int> arrayMemory);
        Assert.True(succeeded && arrayMemory.Span is [0, 1, 2, 3, 4]);

        succeeded = list.TryGetReadOnlyMemory<int>(out ReadOnlyMemory<int> listMemory);
        Assert.True(succeeded && listMemory.Span is [0, 1, 2, 3, 4]);

        succeeded = str.TryGetReadOnlyMemory<char>(out ReadOnlyMemory<char> stringMemory);
        Assert.True(succeeded && stringMemory.Span is "hello");

        succeeded = enumerable.TryGetReadOnlyMemory<int>(out ReadOnlyMemory<int> enumerableMemory);
        Assert.False(succeeded && enumerableMemory.Length > 0);
    }

    [Fact]
    public void MoveTest()
    {
        Span<char> chars = "hello".ToCharArray();
        chars.MoveItem(1, 3);
        Assert.True(chars is ['h', 'l', 'l', 'e', 'o']);
        chars.MoveItem(3, 1);
        Assert.True(chars is ['h', 'e', 'l', 'l', 'o']);
    }
}
