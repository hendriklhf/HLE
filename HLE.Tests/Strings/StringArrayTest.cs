using System;
using System.Collections.Generic;
using HLE.Collections;
using HLE.Strings;
using Xunit;

namespace HLE.Tests.Strings;

public sealed class StringArrayTest
{
    [Fact]
    public void Indexer_Int_Test()
    {
        StringArray array = new(4)
        {
            [0] = "hello",
            [1] = "abc",
            [2] = "xd",
            [3] = "555"
        };

        Assert.True(array is ["hello", "abc", "xd", "555"]);

        array[1] = "////";
        array[2] = string.Empty;

        Assert.True(array is ["hello", "////", "", "555"]);
    }

    [Fact]
    public void Indexer_Int_Throws_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = new StringArray(5)
            {
                [-1] = ""
            };
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = new StringArray(5)
            {
                [5] = ""
            };
        });
    }

    [Fact]
    public void Indexer_Index_Test()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        Assert.Equal("xd", array[^1]);
    }

    [Fact]
    public void Indexer_Index_Throws_Test() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = new StringArray(5)
            {
                [^6] = ""
            };
        });

    [Fact]
    public void Indexer_Range_Test()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        ReadOnlySpan<string> span = array[..2];
        Assert.True(span is ["hello", "abc"]);
    }

    [Fact]
    public void Indexer_Range_Throws_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array[..4];
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array[4..];
        });
    }

    [Fact]
    public void Length_Test()
    {
        StringArray array = new(5);
        Assert.Equal(5, array.Length);

        array = new(256);
        Assert.Equal(256, array.Length);

        array = new(ushort.MaxValue);
        Assert.Equal(ushort.MaxValue, array.Length);
    }

    [Fact]
    public void ICountable_Count_Test()
    {
        ICountable countable = new StringArray(16);
        Assert.Equal(16, countable.Count);
    }

    [Fact]
    public void ICollection_Count_Test()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        ICollection<string> countable = new StringArray(16);
        Assert.Equal(16, countable.Count);
    }

    [Fact]
    public void IReadOnlyCollection_Count_Test()
    {
        IReadOnlyCollection<string> countable = new StringArray(16);
        Assert.Equal(16, countable.Count);
    }

    [Fact]
    public void Initialization_List_Test()
    {
        List<string> list = ["hello", "abc", "xd"];
        StringArray array = new(list);
        Assert.True(array is ["hello", "abc", "xd"]);
    }

    [Fact]
    public void Initialization_Array_Test()
    {
        string[] array = ["hello", "abc", "xd"];
        StringArray stringArray = new(array);
        Assert.True(stringArray is ["hello", "abc", "xd"]);
    }

    [Fact]
    public void Initialization_Span_Test()
    {
        Span<string> span = ["hello", "abc", "xd"];
        StringArray array = new(span);
        Assert.True(array is ["hello", "abc", "xd"]);
    }

    [Fact]
    public void Initialization_ReadOnlySpan_Test()
    {
        ReadOnlySpan<string> span = ["hello", "abc", "xd"];
        StringArray array = new(span);
        Assert.True(array is ["hello", "abc", "xd"]);
    }

    [Fact]
    public void Initialization_IEnumerable_Test()
    {
        IEnumerable<string> enumerable = ["hello", "abc", "xd"];
        StringArray array = new(enumerable);
        Assert.True(array is ["hello", "abc", "xd"]);
    }

    [Fact]
    public void GetStringLengthTest()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        Assert.Equal("hello".Length, array.GetStringLength(0));
        Assert.Equal("abc".Length, array.GetStringLength(1));
        Assert.Equal("xd".Length, array.GetStringLength(2));

        array[1] = "////";
        Assert.Equal("////".Length, array.GetStringLength(1));
    }

    [Fact]
    public void GetStringLength_Throws_Test()
    {
        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array.GetStringLength(-1);
        });

        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            StringArray array = new(["hello", "abc", "xd"]);
            _ = array.GetStringLength(3);
        });
    }

    [Fact]
    public void GetStringLength_Loop_Test()
    {
        StringArray stringArray = new(16);
        Span<string?> array = new string[16];

        for (int i = 0; i < 1024; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32));
            int index = Random.Shared.Next(16);
            array[index] = str;
            stringArray[index] = str;

            for (int j = 0; j < 16; j++)
            {
                Assert.Equal(array[j]?.Length ?? 0, stringArray.GetStringLength(j));
            }
        }
    }

    [Fact]
    public void GetCharsTest()
    {
        StringArray array = new(["hello", "abc", "xd"]);
        Assert.True(array.GetChars(0) is "hello");
        Assert.True(array.GetChars(1) is "abc");
        Assert.True(array.GetChars(2) is "xd");

        array[1] = "////";
        Assert.True(array.GetChars(1) is "////");
    }

    [Fact]
    public void GetChars_Loop_Test()
    {
        StringArray stringArray = new(16);
        Span<string?> array = new string[16];

        for (int i = 0; i < 1024; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32), StringConstants.AlphaNumerics);
            int index = Random.Shared.Next(16);
            array[index] = str;
            stringArray[index] = str;

            Console.WriteLine($"----- Actual vs Expected ----- Write at: {index}");
            for (int j = 0; j < 16; j++)
            {
                ReadOnlySpan<char> actualChars = stringArray.GetChars(j);
                string? expectedChars = array[j];
                Console.WriteLine($"({j}) {actualChars} == {expectedChars}");
                Assert.True(actualChars.SequenceEqual(expectedChars));
            }

        }
    }

    [Fact]
    public void AsSpanTest()
    {
        StringArray stringArray = new(16);
        Assert.Equal(16, stringArray.AsSpan().Length);
        Span<string> array = new string[16];

        for (int i = 0; i < 1024; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32));
            int index = Random.Shared.Next(16);
            array[index] = str;
            stringArray[index] = str;

            Assert.True(array.SequenceEqual(stringArray.AsSpan()));
        }
    }

    [Fact]
    public void MoveStringTest_LeftToRight()
    {
        StringArray array = new(5)
        {
            [0] = "aaa",
            [1] = "bb",
            [2] = "cccc",
            [3] = "d",
            [4] = "eeeee"
        };
        array.MoveString(1, 3);

        Assert.True(array._strings is ["aaa", "cccc", "d", "bb", "eeeee"]);
        Assert.True(array._stringLengths is [3, 4, 1, 2, 5]);
        Assert.True(array._stringStarts is [0, 3, 7, 8, 10]);
        Assert.True(array._stringChars.AsSpan().StartsWith("aaaccccdbbeeeee\0"));

        Assert.True(array.GetChars(0) is "aaa");
        Assert.True(array.GetChars(1) is "cccc");
        Assert.True(array.GetChars(2) is "d");
        Assert.True(array.GetChars(3) is "bb");
        Assert.True(array.GetChars(4) is "eeeee");
    }

    [Fact]
    public void MoveStringTest_RightToLeft()
    {
        StringArray array = new(5)
        {
            [0] = "aaa",
            [1] = "bb",
            [2] = "cccc",
            [3] = "d",
            [4] = "eeeee"
        };
        array.MoveString(3, 1);

        Assert.True(array._strings is ["aaa", "d", "bb", "cccc", "eeeee"]);
        Assert.True(array._stringLengths is [3, 1, 2, 4, 5]);
        Assert.True(array._stringStarts is [0, 3, 4, 6, 10]);
        Assert.True(array._stringChars.AsSpan().StartsWith("aaadbbcccceeeee\0"));

        Assert.True(array.GetChars(0) is "aaa");
        Assert.True(array.GetChars(1) is "d");
        Assert.True(array.GetChars(2) is "bb");
        Assert.True(array.GetChars(3) is "cccc");
        Assert.True(array.GetChars(4) is "eeeee");
    }
}
