using System;
using System.Linq;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Marshalling;
using HLE.Strings;
using Xunit;

namespace HLE.Tests.Strings;

public sealed partial class StringHelperTest
{
    [Fact]
    public void FastAllocateStringTest()
    {
        string str = StringMarshal.FastAllocateString(5, out Span<char> chars);
        Assert.Equal(5, str.Length);
        "hello".CopyTo(chars);
        Assert.Equal("hello", str);
        Assert.NotSame(str, "hello");
    }

    [Fact]
    public void Chunk_string_int_Test()
    {
        string str = Random.Shared.NextString(1000);
        ReadOnlyMemory<char>[] chunks = str.Chunk(50);
        Assert.True(chunks.All(static c => c.Length == 50));

        str = Random.Shared.NextString(1025);
        chunks = str.Chunk(50);
        ReadOnlyMemory<char>[] allExceptLastChunk = chunks[..^1];
        Assert.Equal(20, allExceptLastChunk.Length);
        Assert.True(allExceptLastChunk.All(static c => c.Length == 50));
        Assert.True(chunks[^1].Length == 25);

        chunks = string.Empty.Chunk(10);
        Assert.Empty(chunks);

        chunks = "hello".Chunk(10);
        Assert.True(chunks is [{ Span: "hello" }]);
    }

    [Fact]
    public void Chunk_string_int_char_Test()
    {
        string str = $"{new('*', 10)} {new('*', 20)} {new('*', 2)} {new('*', 2)} {new('*', 10)}";
        ReadOnlyMemory<char>[] chunks = str.Chunk(10, ' ');
        int[] chunkLengths = chunks.Select(static c => c.Length).ToArray();
        Assert.True(chunkLengths is [10, 20, 5, 10]);

        chunks = string.Empty.Chunk(10, ' ');
        Assert.Empty(chunks);

        chunks = "hello".Chunk(10, ' ');
        Assert.True(chunks is [{ Span: "hello" }]);
    }

    [Fact]
    public void TrimAllTest()
    {
        string str = "     aaa        aaa aaa                   aaa     ";
        Assert.Equal(TrimAllWithRegex(str), str.TrimAll());

        str = "      a";
        Assert.Equal(TrimAllWithRegex(str), str.TrimAll());

        str = "a       ";
        Assert.Equal(TrimAllWithRegex(str), str.TrimAll());

        str = "hello";
        Assert.Equal(TrimAllWithRegex(str), str.TrimAll());

        str = "         ";
        Assert.Equal(TrimAllWithRegex(str), str.TrimAll());

        str = string.Empty;
        Assert.Equal(TrimAllWithRegex(str), str.TrimAll());
    }

    private static string TrimAllWithRegex(string str) => GetMultipleSpacesRegex().Replace(str.Trim(), " ");

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex GetMultipleSpacesRegex();

    [Fact]
    public void IndicesOf_string_char_Test()
    {
        string str = Random.Shared.NextString(10_000, "abc ");
        using PooledList<int> correctIndices = [];
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == ' ')
            {
                correctIndices.Add(i);
            }
        }

        int[] indices = str.IndicesOf(' ');
        Assert.True(correctIndices.AsSpan().SequenceEqual(indices));

        indices = string.Empty.IndicesOf(' ');
        Assert.Empty(indices);
        Assert.Same(Array.Empty<int>(), indices);
    }

    [Fact]
    public void IndicesOf_string_ReadOnlySpanChar_Test()
    {
        string str = Random.Shared.NextString(1000, "abc ");
        using PooledList<int> correctIndices = [];
        for (int i = 0; i < str.Length; i++)
        {
            if (i < str.Length - 1 && str[i] == ' ' && str[i + 1] == ' ')
            {
                correctIndices.Add(i++);
            }
        }

        int[] indices = str.IndicesOf("  ");
        Assert.True(correctIndices.AsSpan().SequenceEqual(indices));

        indices = string.Empty.IndicesOf("  ");
        Assert.True(indices is [] && ReferenceEquals(indices, Array.Empty<int>()));
    }

    [Fact]
    public void RegexEscapeTest()
    {
        for (int i = 0; i < 100; i++)
        {
            string str = Random.Shared.NextString(1000, $"{StringHelpers.RegexMetaChars}awidjhiaouwhdiuahwdiauzowgdabkiyjhgefd");
            Assert.Equal(Regex.Escape(str), StringHelpers.RegexEscape(str));
        }

        for (int i = 0; i < 100; i++)
        {
            string str = Random.Shared.NextString(1000, "awidjhiaouwhdiuahwdiauzowgdabkiyjhgefd");
            Assert.Equal(Regex.Escape(str), StringHelpers.RegexEscape(str));
        }
    }
}
