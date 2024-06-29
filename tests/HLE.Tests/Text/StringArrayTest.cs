using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Text;
using Xunit;

namespace HLE.Tests.Text;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
public sealed class StringArrayTest
{
    public static TheoryData<string[]> StringArrayParameters { get; } = CreateStringArrayParameters();

    [Fact]
    public void Indexer_int_get_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        for (int i = 0; i < strings.Length; i++)
        {
            Assert.Same(strings[i], array[i]);
        }
    }

    [Fact]
    public void Indexer_int_get_Throws_IndexOutOfRangeException_GreaterIndex_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        Assert.Throws<IndexOutOfRangeException>(() => array[array.Length]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-345)]
    [InlineData(-90347)]
    public void Indexer_int_get_Throws_IndexOutOfRangeException_Negative_Index_Test(int index)
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        Assert.Throws<IndexOutOfRangeException>(() => array[index]);
    }

    [Fact]
    public void Indexer_int_set_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(3);
        for (int i = 0; i < strings.Length; i++)
        {
            array[i] = strings[i];
            Assert.Same(strings[i], array[i]);
        }
    }

    [Fact]
    public void Indexer_int_set_Throws_IndexOutOfRangeException_GreaterIndex_Test()
    {
        StringArray array = new(3);
        Assert.Throws<ArgumentOutOfRangeException>(() => array[array.Length] = "hello");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-345)]
    [InlineData(-90347)]
    public void Indexer_int_set_Throws_IndexOutOfRangeException_Negative_Index_Test(int index)
    {
        StringArray array = new(3);
        Assert.Throws<ArgumentOutOfRangeException>(() => array[index] = "hello");
    }

    [Fact]
    public void Indexer_Index_get_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        for (int i = 0; i < strings.Length; i++)
        {
            Assert.Same(strings[i], array[new Index(i)]);
        }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(395)]
    [InlineData(34957)]
    public void Indexer_Index_get_Throws_IndexOutOfRangeException_GreaterIndex_Test(int index)
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        Assert.Throws<IndexOutOfRangeException>(() => array[new Index(index)]);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(234, true)]
    [InlineData(54754, true)]
    public void Indexer_Index_get_Throws_IndexOutOfRangeException_Negative_Index_Test(int index, bool fromEnd)
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        Assert.Throws<IndexOutOfRangeException>(() => array[new Index(index, fromEnd)]);
    }

    [Fact]
    public void Indexer_Index_set_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        for (int i = 0; i < strings.Length; i++)
        {
            array[new Index(i)] = strings[i];
            Assert.Same(strings[i], array[i]);
        }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(395)]
    [InlineData(34957)]
    public void Indexer_Index_set_Throws_IndexOutOfRangeException_GreaterIndex_Test(int index)
    {
        StringArray array = new(3);
        Assert.Throws<ArgumentOutOfRangeException>(() => array[new Index(index)] = "hello");
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(234, true)]
    [InlineData(54754, true)]
    public void Indexer_Index_set_Throws_IndexOutOfRangeException_Negative_Index_Test(int index, bool fromEnd)
    {
        StringArray array = new(3);
        Assert.Throws<ArgumentOutOfRangeException>(() => array[new Index(index, fromEnd)] = "hello");
    }

    [Fact]
    public void Indexer_Range_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        ReadOnlySpan<string> s1 = array[1..];
        Assert.True(s1 is ["abc", "xd"]);

        ReadOnlySpan<string> s2 = array[..2];
        Assert.True(s2 is ["hello", "abc"]);
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(3, 1)]
    [InlineData(1, 4)]
    [InlineData(2, 4)]
    public void Indexer_Range_Throws_ArgumentOutOfRangeException_Test(int start, int end)
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Range range = new(start, end);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = array[range]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(389)]
    [InlineData(512)]
    public void Length_Test(int length)
    {
        StringArray array = new(length);
        Assert.Equal(length, array.Length);
    }

    [Fact]
    public void ICountable_Count_Test()
    {
        StringArray array = new(8);
        ICountable countable = CastDownReliably<StringArray, ICountable>(array);
        Assert.Equal(array.Length, countable.Count);
    }

    [Fact]
    public void ICollection_Count_Test()
    {
        StringArray array = new(8);
        ICollection<string> collection = CastDownReliably<StringArray, ICollection<string>>(array);
        Assert.Equal(array.Length, collection.Count);
    }

    [Fact]
    public void ICollection_IsReadOnly_Test()
    {
        StringArray array = new(8);
        ICollection<string> collection = CastDownReliably<StringArray, ICollection<string>>(array);
        Assert.False(collection.IsReadOnly);
    }

    [Fact]
    public void IReadOnlyCollection_Count_Test()
    {
        StringArray array = new(8);
        IReadOnlyCollection<string> readOnlyCollection = CastDownReliably<StringArray, IReadOnlyCollection<string>>(array);
        Assert.Equal(array.Length, readOnlyCollection.Count);
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void Ctor_List_Test(string[] strings)
    {
        List<string> list = strings.ToList();
        StringArray array = new(list);

        Assert.Equal(list.Count, array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Assert.Same(list[i], array[i]);
        }
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void Ctor_Array_Test(string[] strings)
    {
        StringArray array = new(strings);

        Assert.Equal(strings.Length, array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Assert.Same(strings[i], array[i]);
        }
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void Ctor_Span_Test(string[] strings)
    {
        StringArray array = new(strings.AsSpan());

        Assert.Equal(strings.Length, array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Assert.Same(strings[i], array[i]);
        }
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void Ctor_ReadOnlySpan_Test(string[] strings)
    {
        ReadOnlySpan<string> span = strings;
        StringArray array = new(span);

        Assert.Equal(strings.Length, array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Assert.Same(strings[i], array[i]);
        }
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void Ctor_Enumerable_Test(string[] strings)
    {
        IEnumerable<string> enumerable = CastDownReliably<string[], IEnumerable<string>>(strings);
        StringArray array = new(enumerable);

        Assert.Equal(strings.Length, array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Assert.Same(strings[i], array[i]);
        }
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void Ctor_Yielding_Enumerable_Test(string[] strings)
    {
        IEnumerable<string> enumerable = AsYieldingEnumerable(strings);
        StringArray array = new(enumerable);

        Assert.Equal(strings.Length, array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            Assert.Same(strings[i], array[i]);
        }
    }

    [Fact]
    public void GetStringLength_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.Equal("hello".Length, array.GetStringLength(0));
        Assert.Equal("abc".Length, array.GetStringLength(1));
        Assert.Equal("xd".Length, array.GetStringLength(2));
    }

    [Fact]
    public void GetStringLength_Throws_IndexOutOfRangeException_GreaterIndex_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.Throws<IndexOutOfRangeException>(() => array.GetStringLength(3));
    }

    [Fact]
    public void GetStringLength_Throws_IndexOutOfRangeException_NegativeIndex_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.Throws<IndexOutOfRangeException>(() => array.GetStringLength(-1));
    }

    [Fact]
    public void GetChars_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(array.GetChars(0) is "hello");
        Assert.True(array.GetChars(1) is "abc");
        Assert.True(array.GetChars(2) is "xd");
    }

    [Fact]
    public void GetChars_Throws_IndexOutOfRangeException_GreaterIndex_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.Throws<IndexOutOfRangeException>(() => _ = array.GetChars(3));
    }

    [Fact]
    public void GetChars_Throws_IndexOutOfRangeException_NegativeIndex_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.Throws<IndexOutOfRangeException>(() => _ = array.GetChars(-1));
    }

    [Fact]
    public void AsSpan_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(array.AsSpan().SequenceEqual(strings));
    }

    [Fact]
    public void AsMemory_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(array.AsMemory().Span.SequenceEqual(strings));
    }

    [Fact]
    public void ToArray_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(array.ToArray().AsSpan().SequenceEqual(strings));
    }

    [Fact]
    public void ToArray_Empty_Test()
    {
        StringArray array = StringArray.Empty;
        Assert.Same(Array.Empty<string>(), array.ToArray());
    }

    [Fact]
    public void ToArray_Start_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

#pragma warning disable IDE0057
        Assert.True(array.ToArray(1).AsSpan().SequenceEqual(strings[1..]));
#pragma warning restore IDE0057
    }

    [Fact]
    public void ToArray_Start_Length_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(array.ToArray(1, 1).AsSpan().SequenceEqual(strings[1..2]));
    }

    [Fact]
    public void ToArray_Range_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(array.ToArray(1..2).AsSpan().SequenceEqual(strings[1..2]));
    }

    [Fact]
    public void ToList_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(CollectionsMarshal.AsSpan(array.ToList()).SequenceEqual(strings));
    }

    [Fact]
    public void ToList_Start_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

#pragma warning disable IDE0057
        Assert.True(CollectionsMarshal.AsSpan(array.ToList(1)).SequenceEqual(strings[1..]));
#pragma warning restore IDE0057
    }

    [Fact]
    public void ToList_Start_Length_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(CollectionsMarshal.AsSpan(array.ToList(1, 1)).SequenceEqual(strings[1..2]));
    }

    [Fact]
    public void ToList_Range_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);

        Assert.True(CollectionsMarshal.AsSpan(array.ToList(1..2)).SequenceEqual(strings[1..2]));
    }

    [Fact]
    public void IReadOnlySpanProvider_GetReadOnlySpan_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        IReadOnlySpanProvider<string> spanProvider = CastDownReliably<StringArray, IReadOnlySpanProvider<string>>(array);

        Assert.True(spanProvider.GetReadOnlySpan().SequenceEqual(array.AsSpan()));
    }

    [Fact]
    public void IReadOnlyMemoryProvider_GetReadOnlyMemory_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        IReadOnlyMemoryProvider<string> spanProvider = CastDownReliably<StringArray, IReadOnlyMemoryProvider<string>>(array);

        Assert.True(spanProvider.GetReadOnlyMemory().Span.SequenceEqual(array.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void IndexOf_String_Test(string[] strings)
    {
        StringArray array = new(strings);
        if (array.Length == 0)
        {
            Assert.Equal(-1, array.IndexOf("hello"));
            return;
        }

        string needle = Random.Shared.GetItem(strings);

        int actual = strings.AsSpan().IndexOf(needle);
        int expected = array.IndexOf(needle);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void IndexOf_ReadOnlySpan_Test(string[] strings)
    {
        StringArray array = new(strings);
        if (array.Length == 0)
        {
            Assert.Equal(-1, array.IndexOf("hello".AsSpan()));
            return;
        }

        string needle = Random.Shared.GetItem(strings);

        int actual = strings.AsSpan().IndexOf(needle);
        int expected = array.IndexOf(needle.AsSpan());

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IndexOf_String_StringComparision_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("abc", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_String_StringComparision_StartIndex_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("abc", StringComparison.OrdinalIgnoreCase, 1);
        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_ReadOnlySpan_StringComparision_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("abc".AsSpan(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_ReadOnlySpan_StringComparision_StartIndex_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("abc".AsSpan(), StringComparison.OrdinalIgnoreCase, 1);
        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_String_StringComparision_DoesntContain_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("xd", StringComparison.OrdinalIgnoreCase);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_String_StringComparision_StartIndex_DoesntContain_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("xd", StringComparison.OrdinalIgnoreCase, 1);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_ReadOnlySpan_StringComparision_DoesntContain_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("xd".AsSpan(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_ReadOnlySpan_StringComparision_StartIndex_DoesntContain_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        int index = array.IndexOf("xd".AsSpan(), StringComparison.OrdinalIgnoreCase, 1);
        Assert.Equal(-1, index);
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    [SuppressMessage("Minor Code Smell", "S4058:Overloads with a \"StringComparison\" parameter should be used")]
    public void IndexOf_ReadOnlySpan_StartingIndex_Test(string[] strings)
    {
        StringArray array = new(strings);
        if (array.Length == 0)
        {
            Assert.Equal(-1, array.IndexOf("hello"));
            return;
        }

        string needle = Random.Shared.GetItem(strings);

        int start = strings.Length >>> 2;
        int actual = Array.IndexOf(strings, needle, start);
        int expected = array.IndexOf(needle, start);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    [SuppressMessage("Minor Code Smell", "S4058:Overloads with a \"StringComparison\" parameter should be used")]
    [SuppressMessage("Assertions", "xUnit2017:Do not use Contains() to check if a value exists in a collection")]
    public void Contains_String_Test(string[] strings)
    {
        StringArray array = new(strings);
        if (array.Length == 0)
        {
            Assert.False(array.Contains("hello"));
            return;
        }

        string needle = Random.Shared.GetItem(strings);
        bool contains = array.Contains(needle);
        Assert.True(contains);
    }

    [Theory]
    [MemberData(nameof(StringArrayParameters))]
    public void Contains_ReadOnlySpan_Test(string[] strings)
    {
        StringArray array = new(strings);
        if (array.Length == 0)
        {
            Assert.False(array.Contains("hello".AsSpan()));
            return;
        }

        string needle = Random.Shared.GetItem(strings);
        bool contains = array.Contains(needle.AsSpan());
        Assert.True(contains);
    }

    [Fact]
    public void Contains_String_StringComparision_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        bool contains = array.Contains("abc", StringComparison.OrdinalIgnoreCase);
        Assert.True(contains);
    }

    [Fact]
    public void Contains_ReadOnlySpan_StringComparision_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        bool contains = array.Contains("abc".AsSpan(), StringComparison.OrdinalIgnoreCase);
        Assert.True(contains);
    }

    [Fact]
    public void Contains_String_StringComparision_DoesntContain_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        bool contains = array.Contains("xd", StringComparison.OrdinalIgnoreCase);
        Assert.False(contains);
    }

    [Fact]
    public void Contains_ReadOnlySpan_StringComparision_DoesntContain_Test()
    {
        StringArray array = new("hello", "ABc", "abc");
        bool contains = array.Contains("xd".AsSpan(), StringComparison.OrdinalIgnoreCase);
        Assert.False(contains);
    }

    [Fact]
    public void Add_Throws_NotSupportedException_Test()
    {
        ICollection<string> collection = CastDownReliably<StringArray, ICollection<string>>(new(4));
        Assert.Throws<NotSupportedException>(() => collection.Add("abc"));
    }

    [Fact]
    public void Remove_Throws_NotSupportedException_Test()
    {
        ICollection<string> collection = CastDownReliably<StringArray, ICollection<string>>(new(4));
        Assert.Throws<NotSupportedException>(() => collection.Remove("abc"));
    }

    // TODO: add missing tests

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static TTo CastDownReliably<TFrom, TTo>(TFrom from) where TFrom : TTo
        => from;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<string> AsYieldingEnumerable(string[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            yield return array[i];
        }
    }

    private static TheoryData<string[]> CreateStringArrayParameters()
    {
        ReadOnlySpan<string> randomStrings = Enumerable.Range(0, 4096)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(4, 32), StringConstants.AlphaNumerics))
            .ToArray();

        TheoryData<string[]> data = new();
        for (int i = 0; i <= 1024; i++)
        {
            string[] array = new string[i];
            Random.Shared.Fill(array, randomStrings);
            data.Add(array);
        }

        return data;
    }
}
