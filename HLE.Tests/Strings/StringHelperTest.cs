using System;
using System.Linq;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Strings;

[TestClass]
public partial class StringHelperTest
{
    [TestMethod]
    public void FastAllocateStringTest()
    {
        string str = StringHelper.FastAllocateString(5, out Span<char> chars);
        Assert.AreEqual(5, str.Length);
        "hello".CopyTo(chars);
        Assert.AreEqual("hello", str);
        Assert.IsFalse(ReferenceEquals(str, "hello"));
    }

    [TestMethod]
    public void Chunk_string_int_Test()
    {
        string str = Random.Shared.NextString(1000);
        ReadOnlyMemory<char>[] chunks = str.Chunk(50);
        Assert.IsTrue(chunks.All(c => c.Length == 50));

        str = Random.Shared.NextString(1025);
        chunks = str.Chunk(50);
        ReadOnlyMemory<char>[] allExceptLastChunk = chunks[..^1];
        Assert.AreEqual(20, allExceptLastChunk.Length);
        Assert.IsTrue(allExceptLastChunk.All(c => c.Length == 50));
        Assert.IsTrue(chunks[^1].Length == 25);

        chunks = string.Empty.Chunk(10);
        Assert.AreEqual(0, chunks.Length);

        chunks = "hello".Chunk(10);
        Assert.IsTrue(chunks is [{ Span: "hello" }]);
    }

    [TestMethod]
    public void Chunk_string_int_char_Test()
    {
        string str = $"{new('*', 10)} {new('*', 20)} {new('*', 2)} {new('*', 2)} {new('*', 10)}";
        ReadOnlyMemory<char>[] chunks = str.Chunk(10, ' ');
        int[] chunkLengths = chunks.Select(c => c.Length).ToArray();
        Assert.IsTrue(chunkLengths is [10, 20, 5, 10]);

        chunks = string.Empty.Chunk(10, ' ');
        Assert.AreEqual(0, chunks.Length);

        chunks = "hello".Chunk(10, ' ');
        Assert.IsTrue(chunks is [{ Span: "hello" }]);
    }

    [TestMethod]
    public void TrimAllTest()
    {
        string str = "     aaa        aaa aaa                   aaa     ";
        Assert.AreEqual(TrimAllWithRegex(str), str.TrimAll());

        str = "      a";
        Assert.AreEqual(TrimAllWithRegex(str), str.TrimAll());

        str = "a       ";
        Assert.AreEqual(TrimAllWithRegex(str), str.TrimAll());

        str = "hello";
        Assert.AreEqual(TrimAllWithRegex(str), str.TrimAll());

        str = "         ";
        Assert.AreEqual(TrimAllWithRegex(str), str.TrimAll());

        str = string.Empty;
        Assert.AreEqual(TrimAllWithRegex(str), str.TrimAll());
    }

    private static string TrimAllWithRegex(string str)
    {
        return GetMultipleSpacesRegex().Replace(str.Trim(), " ");
    }

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex GetMultipleSpacesRegex();

    [TestMethod]
    public void IndicesOf_string_char_Test()
    {
        string str = Random.Shared.NextString(1000, "abc ");
        using PoolBufferList<int> correctIndices = new();
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == ' ')
            {
                correctIndices.Add(i);
            }
        }

        int[] indices = str.IndicesOf(' ');
        Assert.IsTrue(correctIndices.AsSpan().SequenceEqual(indices));

        indices = string.Empty.IndicesOf(' ');
        Assert.IsTrue(indices is [] && ReferenceEquals(indices, Array.Empty<int>()));
    }

    [TestMethod]
    public void IndicesOf_string_ReadOnlySpanChar_Test()
    {
        string str = Random.Shared.NextString(1000, "abc ");
        using PoolBufferList<int> correctIndices = new();
        for (int i = 0; i < str.Length; i++)
        {
            if (i < str.Length - 1 && str[i] == ' ' && str[i + 1] == ' ')
            {
                correctIndices.Add(i++);
            }
        }

        int[] indices = str.IndicesOf("  ");
        Assert.IsTrue(correctIndices.AsSpan().SequenceEqual(indices));

        indices = string.Empty.IndicesOf("  ");
        Assert.IsTrue(indices is [] && ReferenceEquals(indices, Array.Empty<int>()));
    }

    [TestMethod]
    public void GetRangesOfSplit_string_char()
    {
        string str = Random.Shared.NextString(1000, "abc ");
        using PoolBufferList<int> correctIndices = new();
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == ' ')
            {
                correctIndices.Add(i);
            }
        }
    }
}
