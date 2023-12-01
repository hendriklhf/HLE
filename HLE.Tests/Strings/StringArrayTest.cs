using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Strings;
using Xunit;
using Xunit.Abstractions;

namespace HLE.Tests.Strings;

public sealed class StringArrayTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public void EmptyArrayCreationTest()
    {
        StringArray array = new(0);
        Assert.Same(Array.Empty<string>(), array._strings);
        Assert.Same(Array.Empty<int>(), array._starts);
        Assert.Same(Array.Empty<int>(), array._lengths);
        Assert.Null(array._chars);
    }

    [Fact]
    public void Indexer_Int_Test()
    {
        const int ArrayLength = 64;
        const int Iterations = 4096;

        StringArray stringArray = new(ArrayLength);
        Span<string?> array = new string[ArrayLength];

        for (int i = 0; i < Iterations; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32), StringConstants.AlphaNumerics);
            int index = Random.Shared.Next(ArrayLength);
            array[index] = str;
            stringArray[index] = str;

            _testOutputHelper.WriteLine($"----- Actual vs Expected ----- Write at: {index}");
            for (int j = 0; j < ArrayLength; j++)
            {
                string actualString = stringArray[j];
                string? expectedString = array[j];
                _testOutputHelper.WriteLine($"({j}) {actualString} == {expectedString}");
                Assert.Equal(expectedString, actualString);
            }
        }
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
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        Assert.Equal("hello", array[^3]);
        Assert.Equal("abc", array[^2]);
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
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray array = new(strings);
        ReadOnlySpan<string> span = array[..2];
        Assert.True(span is ["hello", "abc"]);
    }

    [Fact]
    public void Indexer_Range_Throws_Test()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
            StringArray array = new(strings);
            _ = array[..4];
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
            StringArray array = new(strings);
            _ = array[4..];
        });
    }

    [Fact]
    public void LengthTest()
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
        List<string> strings = Enumerable.Range(0, 1024)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToList();
        StringArray stringArray = new(strings);
        Assert.Equal(strings.Count, stringArray.Length);

        for (int i = 0; i < strings.Count; i++)
        {
            Assert.Same(strings[i], stringArray[i]);
            Assert.Equal(strings[i].Length, stringArray.GetStringLength(i));
            Assert.True(strings[i].AsSpan().SequenceEqual(stringArray.GetChars(i)));
        }
    }

    [Fact]
    public void Initialization_Array_Test()
    {
        string[] strings = Enumerable.Range(0, 1024)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();
        StringArray stringArray = new(strings);
        Assert.Equal(strings.Length, stringArray.Length);

        for (int i = 0; i < strings.Length; i++)
        {
            Assert.Same(strings[i], stringArray[i]);
            Assert.Equal(strings[i].Length, stringArray.GetStringLength(i));
            Assert.True(strings[i].AsSpan().SequenceEqual(stringArray.GetChars(i)));
        }
    }

    [Fact]
    public void Initialization_Span_Test()
    {
        Span<string> strings = Enumerable.Range(0, 1024)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();
        StringArray stringArray = new(strings);
        Assert.Equal(strings.Length, stringArray.Length);

        for (int i = 0; i < strings.Length; i++)
        {
            Assert.Same(strings[i], stringArray[i]);
            Assert.Equal(strings[i].Length, stringArray.GetStringLength(i));
            Assert.True(strings[i].AsSpan().SequenceEqual(stringArray.GetChars(i)));
        }
    }

    [Fact]
    public void Initialization_ReadOnlySpan_Test()
    {
        ReadOnlySpan<string> strings = Enumerable.Range(0, 1024)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();
        StringArray stringArray = new(strings);
        Assert.Equal(strings.Length, stringArray.Length);

        for (int i = 0; i < strings.Length; i++)
        {
            Assert.Same(strings[i], stringArray[i]);
            Assert.Equal(strings[i].Length, stringArray.GetStringLength(i));
            Assert.True(strings[i].AsSpan().SequenceEqual(stringArray.GetChars(i)));
        }
    }

    [Fact]
    public void Initialization_IEnumerable_Test()
    {
        string[] strings =
            Enumerable.Range(0, 1024)
                .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();

        StringArray stringArray = new(strings.AsEnumerable());
        Assert.Equal(strings.Length, stringArray.Length);

        for (int i = 0; i < strings.Length; i++)
        {
            Assert.Same(strings[i], stringArray[i]);
            Assert.Equal(strings[i].Length, stringArray.GetStringLength(i));
            Assert.True(strings[i].AsSpan().SequenceEqual(stringArray.GetChars(i)));
        }
    }

    [Fact]
    public void GetStringLength_Throws_Test()
    {
        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
            StringArray array = new(strings);
            _ = array.GetStringLength(-1);
        });

        Assert.Throws<IndexOutOfRangeException>(() =>
        {
            ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
            StringArray array = new(strings);
            _ = array.GetStringLength(3);
        });
    }

    [Fact]
    public void GetStringLengthTest()
    {
        const int ArrayLength = 64;
        const int Iterations = 4096;

        StringArray stringArray = new(ArrayLength);
        Span<string?> array = new string[ArrayLength];

        for (int i = 0; i < Iterations; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32), StringConstants.AlphaNumerics);
            int index = Random.Shared.Next(ArrayLength);
            array[index] = str;
            stringArray[index] = str;

            for (int j = 0; j < ArrayLength; j++)
            {
                Assert.Equal(array[j]?.Length ?? 0, stringArray.GetStringLength(j));
            }
        }
    }

    [Fact]
    public void GetCharsTest()
    {
        const int ArrayLength = 64;
        const int Iterations = 4096;

        StringArray stringArray = new(ArrayLength);
        Span<string?> array = new string[ArrayLength];

        for (int i = 0; i < Iterations; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32), StringConstants.AlphaNumerics);
            int index = Random.Shared.Next(ArrayLength);
            array[index] = str;
            stringArray[index] = str;

            _testOutputHelper.WriteLine($"----- Actual vs Expected ----- Write at: {index}");
            for (int j = 0; j < ArrayLength; j++)
            {
                ReadOnlySpan<char> actualChars = stringArray.GetChars(j);
                string? expectedChars = array[j];
                _testOutputHelper.WriteLine($"({j}) {actualChars} == {expectedChars}");
                Assert.True(actualChars.SequenceEqual(expectedChars));
            }
        }
    }

    [Fact]
    public void AsSpanTest()
    {
        const int Iterations = 4096;
        const int ArrayLength = 1024;

        Span<string> array = Enumerable.Range(0, ArrayLength)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();
        StringArray stringArray = new(array);

        Assert.Equal(ArrayLength, stringArray.AsSpan().Length);
        Assert.True(stringArray.AsSpan().SequenceEqual(array));

        for (int i = 0; i < Iterations; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(0, 32), StringConstants.AlphaNumerics);
            int index = Random.Shared.Next(ArrayLength);
            array[index] = str;
            stringArray[index] = str;

            Assert.True(stringArray.AsSpan().SequenceEqual(array));
        }
    }

    [Fact]
    public void ToArrayTest()
    {
        Span<string> span = Enumerable.Range(0, 1024)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();
        StringArray stringArray = new(span);
        string[] array = stringArray.ToArray();
        Assert.True(stringArray.ToArray().AsSpan().SequenceEqual(array));
    }

    [Fact]
    public void ToListTest()
    {
        Span<string> span = Enumerable.Range(0, 1024)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();
        StringArray stringArray = new(span);
        List<string> list = stringArray.ToList();
        Assert.True(stringArray.ToArray().AsSpan().SequenceEqual(CollectionsMarshal.AsSpan(list)));
    }

    [Fact]
    public void IReadOnlySpanProvider_GetReadOnlySpan_Test()
    {
        ReadOnlySpan<string> strings = ["hello", "abc", "xd"];
        StringArray stringArray = new(strings);
        IReadOnlySpanProvider<string> readOnlySpanProvider = stringArray;
        Assert.True(stringArray.AsSpan().SequenceEqual(readOnlySpanProvider.GetReadOnlySpan()));
    }

    [Fact]
    public void ClearTest()
    {
        StringArray stringArray = new(Enumerable.Range(0, 1024)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)));
        stringArray.Clear();
        for (int i = 0; i < stringArray.Length; i++)
        {
            Assert.Null(stringArray[i]);
            Assert.Equal(0, stringArray.GetStringLength(i));
            Assert.Equal(0, stringArray.GetChars(i).Length);
        }
    }

    [Fact]
    public void IndexOfTest()
    {
        const int Iterations = 1024;
        const int ArrayLength = 512;

        Span<string> array = Enumerable.Range(0, ArrayLength)
            .Select(static _ => Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics)).ToArray();
        StringArray stringArray = new(array);

        for (int i = 0; i < Iterations; i++)
        {
            string str = Random.Shared.NextString(Random.Shared.Next(8, 64), StringConstants.AlphaNumerics);
            int index = Random.Shared.Next(ArrayLength);
            array[index] = str;
            stringArray[index] = str;

            for (int j = 0; j < ArrayLength; j++)
            {
                Assert.Equal(array.IndexOf(array[j]), stringArray.IndexOf(array[j]));
            }
        }
    }
}
