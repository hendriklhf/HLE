using System;
using System.Linq;
using System.Text.RegularExpressions;
using HLE.Collections;
using HLE.Memory;
using HLE.TestUtilities;
using HLE.Text;
using Xunit;

namespace HLE.UnitTests.Text;

public sealed partial class StringHelpersTest
{
    public static TheoryData<string, char> IndicesOfParameters { get; } = CreateIndicesOfParameters();

    public static TheoryData<string> TrimAllParameters { get; } = new
    (
        "",
        "                   ",
        "a",
        "aaaaaaaaaaaaa",
        " a ",
        "    aa          a       ",
        "a         aaa",
        "               aaaaa          aaaaaaaa  aa        ",
        "a a a a a     aaaaaa         aaaaa a      a   ",
        "          aaaaaaa  a aaaaaa a a     a a aaaaaaaaa"
    );

    public static TheoryData<string> RegexEscapeWithoutMetaCharsParameters { get; } = CreateRegexEscapeWithoutMetaCharsParameters();

    public static TheoryData<string> RegexEscapeWithMetaCharsParameters { get; } = CreateRegexEscapeWithMetaCharsParameters();

    public static TheoryData<string[]> JoinAndConcatStringsParameters { get; } = CreateJoinStringsParameters();

    public static TheoryData<char[]> JoinCharsParameters { get; } = CreateJoinCharsParameters();

    public static TheoryData<string> RandomStringParameters { get; } = TheoryDataHelpers.CreateRandomStrings(256, 8, 64);

    [Theory]
    [MemberData(nameof(TrimAllParameters))]
    public void TrimAll_Test(string str)
        => Assert.Equal(TrimAllRegex.Replace(str, " ").Trim(), StringHelpers.TrimAll(str));

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled, 1_000)]
    private static partial Regex TrimAllRegex { get; }

    [Theory]
    [MemberData(nameof(IndicesOfParameters))]
    public void IndicesOf_Char_Test(string str, char c)
    {
        // ReSharper disable once NotDisposedResource (it is disposed)
        ValueList<int> loopedIndices = new(stackalloc int[256]);
        try
        {
            GetLoopedIndices(ref loopedIndices, str, c);
            ReadOnlySpan<int> indices = str.IndicesOf(c);
            Assert.True(indices.SequenceEqual(loopedIndices.AsSpan()));
        }
        finally
        {
            loopedIndices.Dispose();
        }
    }

    [Fact]
    public void IndicesOf_Char_EmptyString_Test()
    {
        int[] indices = "".IndicesOf('A');
        Assert.Empty(indices);
        Assert.Same(Array.Empty<int>(), indices);
    }

    [Fact]
    public void IndicesOf_Char_CharIsNotInString_Test()
    {
        int[] indices = "BCDEFGH".IndicesOf('A');
        Assert.Empty(indices);
        Assert.Same(Array.Empty<int>(), indices);
    }

    // TODO: IndicesOf(string, ReadOnlySpan<char>)

    [Theory]
    [MemberData(nameof(RegexEscapeWithoutMetaCharsParameters))]
    public void RegexEscape_ContainsNoMetaChars_Test(string str)
    {
        string actual = StringHelpers.RegexEscape(str);
        Assert.Same(str, actual);

        string expected = Regex.Escape(str);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(RegexEscapeWithMetaCharsParameters))]
    public void RegexEscape_ContainsMetaChars_Test(string str)
    {
        string actual = StringHelpers.RegexEscape(str);
        string expected = Regex.Escape(str);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Join_CharSeparator_ReadOnlySpan_String_EmptyStrings_Test()
    {
        int writtenChars = StringHelpers.Join(',', ReadOnlySpan<string>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinAndConcatStringsParameters))]
    public void Join_CharSeparator_ReadOnlySpan_String_Test(string[] strings)
    {
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(strings.Length * 64);
        int writtenChars = StringHelpers.Join(',', strings, buffer.AsSpan());
        ReadOnlySpan<char> chars = buffer[..writtenChars];
        Assert.True(chars.SequenceEqual(string.Join(',', strings)));
    }

    [Fact]
    public void Join_CharSeparator_ReadOnlySpan_ReadOnlyMemory_Char_EmptyStrings_Test()
    {
        int writtenChars = StringHelpers.Join(',', ReadOnlySpan<ReadOnlyMemory<char>>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinAndConcatStringsParameters))]
    public void Join_CharSeparator_ReadOnlySpan_ReadOnlyMemory_Char_Test(string[] strings)
    {
        ReadOnlySpan<ReadOnlyMemory<char>> stringsAsMemory = strings.Select(static s => s.AsMemory()).ToArray();
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(strings.Length * 64);
        int writtenChars = StringHelpers.Join(',', stringsAsMemory, buffer.AsSpan());
        ReadOnlySpan<char> chars = buffer[..writtenChars];
        Assert.True(chars.SequenceEqual(string.Join(',', strings)));
    }

    [Fact]
    public void Join_StringSeparator_ReadOnlySpan_String_EmptyStrings_Test()
    {
        int writtenChars = StringHelpers.Join(", ", ReadOnlySpan<string>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinAndConcatStringsParameters))]
    public void Join_StringSeparator_ReadOnlySpan_String_Test(string[] strings)
    {
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(strings.Length * 64);
        int writtenChars = StringHelpers.Join(", ", strings, buffer.AsSpan());
        ReadOnlySpan<char> chars = buffer[..writtenChars];
        Assert.True(chars.SequenceEqual(string.Join(", ", strings)));
    }

    [Fact]
    public void Join_StringSeparator_ReadOnlySpan_ReadOnlyMemory_Char_EmptyStrings_Test()
    {
        int writtenChars = StringHelpers.Join(", ", ReadOnlySpan<ReadOnlyMemory<char>>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinAndConcatStringsParameters))]
    public void Join_StringSeparator_ReadOnlySpan_ReadOnlyMemory_Char_Test(string[] strings)
    {
        ReadOnlySpan<ReadOnlyMemory<char>> stringsAsMemory = strings.Select(static s => s.AsMemory()).ToArray();
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(strings.Length * 64);
        int writtenChars = StringHelpers.Join(", ", stringsAsMemory, buffer.AsSpan());
        ReadOnlySpan<char> chars = buffer[..writtenChars];
        Assert.True(chars.SequenceEqual(string.Join(", ", strings)));
    }

    [Fact]
    public void Join_CharSeparator_ReadOnlySpan_Char_EmptyChars_Test()
    {
        int writtenChars = StringHelpers.Join(',', ReadOnlySpan<char>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinCharsParameters))]
    public void Join_CharSeparator_ReadOnlySpan_Char_Test(char[] chars)
    {
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(chars.Length * 2);
        int writtenChars = StringHelpers.Join(',', chars, buffer.AsSpan());
        ReadOnlySpan<char> str = buffer[..writtenChars];
        Assert.True(str.SequenceEqual(string.Join(',', chars)));
    }

    [Fact]
    public void Join_StringSeparator_ReadOnlySpan_Char_EmptyChars_Test()
    {
        int writtenChars = StringHelpers.Join(", ", ReadOnlySpan<char>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinCharsParameters))]
    public void Join_StringSeparator_ReadOnlySpan_Char_Test(char[] chars)
    {
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(chars.Length * 3);
        int writtenChars = StringHelpers.Join(", ", chars, buffer.AsSpan());
        ReadOnlySpan<char> str = buffer[..writtenChars];
        Assert.True(str.SequenceEqual(string.Join(", ", chars)));
    }

    [Fact]
    public void Concat_ReadOnlySpan_String_EmptyStrings_Test()
    {
        int writtenChars = StringHelpers.Concat(ReadOnlySpan<string>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinAndConcatStringsParameters))]
    public void Concat_ReadOnlySpan_String_Test(string[] strings)
    {
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(strings.Length * 64);
        int writtenChars = StringHelpers.Concat(strings, buffer.AsSpan());
        ReadOnlySpan<char> chars = buffer[..writtenChars];
        Assert.True(chars.SequenceEqual(string.Concat(strings)));
    }

    [Fact]
    public void Concat_ReadOnlySpan_ReadOnlyMemory_Char_EmptyStrings_Test()
    {
        int writtenChars = StringHelpers.Concat(ReadOnlySpan<ReadOnlyMemory<char>>.Empty, stackalloc char[16]);
        Assert.Equal(0, writtenChars);
    }

    [Theory]
    [MemberData(nameof(JoinAndConcatStringsParameters))]
    public void Concat_ReadOnlySpan_ReadOnlyMemory_Char_Test(string[] strings)
    {
        ReadOnlySpan<ReadOnlyMemory<char>> stringsAsMemory = strings.Select(static s => s.AsMemory()).ToArray();
        using RentedArray<char> buffer = ArrayPool<char>.Shared.RentAsRentedArray(strings.Length * 64);
        int writtenChars = StringHelpers.Concat(stringsAsMemory, buffer.AsSpan());
        ReadOnlySpan<char> chars = buffer[..writtenChars];
        Assert.True(chars.SequenceEqual(string.Concat(strings)));
    }

    private static void GetLoopedIndices(ref ValueList<int> indices, string str, char c)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == c)
            {
                indices.Add(i);
            }
        }
    }

    private static TheoryData<string, char> CreateIndicesOfParameters()
    {
        TheoryData<string, char> data = new();
        for (int i = 1; i < 1024; i++)
        {
            string str = Random.Shared.NextString(i, StringConstants.AlphaNumerics);
            char c = Random.Shared.GetItem(StringConstants.AlphaNumerics);
            data.Add(str, c);
        }

        for (int i = 0; i < 1024; i++)
        {
            string str = Random.Shared.NextString(4096, StringConstants.AlphaNumerics);
            char c = Random.Shared.GetItem(StringConstants.AlphaNumerics);
            data.Add(str, c);
        }

        return data;
    }

    private static TheoryData<string> CreateRegexEscapeWithoutMetaCharsParameters()
    {
        TheoryData<string> data = [string.Empty];
        for (int i = 0; i < 64; i++)
        {
            string str = Random.Shared.NextString(i, StringConstants.AlphaNumerics);
            data.Add(str);
        }

        return data;
    }

    private static TheoryData<string> CreateRegexEscapeWithMetaCharsParameters()
    {
        TheoryData<string> data = new();

        for (int i = 0; i < 2048; i++)
        {
            string str = Random.Shared.NextString(i, StringConstants.AlphaNumerics + StringHelpers.RegexMetaChars);
            data.Add(str);
        }

        return data;
    }

    private static TheoryData<string[]> CreateJoinStringsParameters()
    {
        TheoryData<string[]> data = new();
        for (int i = 0; i < 64; i++)
        {
            string[] strings = Enumerable.Range(0, Random.Shared.Next(4, 32))
                .Select(static _ => Random.Shared.NextString(Random.Shared.Next(4, 32), StringConstants.AlphaNumerics))
                .ToArray();

            data.Add(strings);
        }

        return data;
    }

    private static TheoryData<char[]> CreateJoinCharsParameters()
    {
        TheoryData<char[]> data = new();
        for (int i = 0; i < 64; i++)
        {
            char[] chars = new char[Random.Shared.Next(4, 32)];
            Random.Shared.Fill(chars, StringConstants.AlphaNumerics);
            data.Add(chars);
        }

        return data;
    }
}
