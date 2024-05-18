using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Test.TestUtilities;
using Xunit;

namespace HLE.Tests.Collections;

public sealed class ArrayEnumeratorTest
{
    public static TheoryData<int> EnumerateParameters { get; } = TheoryDataHelpers.CreateExclusiveRange(1, 256);

    public static TheoryData<int> EnumerateRangeParameters { get; } = TheoryDataHelpers.CreateExclusiveRange(8, 256);

    [Theory]
    [MemberData(nameof(EnumerateParameters))]
    public void Enumerate_Array_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        int counter = 0;
        ArrayEnumerator<int> enumerator = new(array);
        while (enumerator.MoveNext())
        {
            Assert.Equal(array[counter++], enumerator.Current);
        }

        Assert.Equal(counter, array.Length);
    }

    [Fact]
    public void Enumerate_Array_Empty_Test()
    {
        ArrayEnumerator<int> enumerator = new(Array.Empty<int>());
        while (enumerator.MoveNext())
        {
            Assert.Fail();
        }
    }

    [Theory]
    [MemberData(nameof(EnumerateRangeParameters))]
    public void Enumerate_Array_Range_Test(int length)
    {
        int[] array = new int[length];
        Random.Shared.Fill(array);

        int counter = 0;
        ArrayEnumerator<int> enumerator = new(array, 4, array.Length - 4);
        while (enumerator.MoveNext())
        {
            Assert.Equal(array[counter++ + 4], enumerator.Current);
        }

        Assert.Equal(counter, array.Length - 4);
    }

    [Theory]
    [MemberData(nameof(EnumerateRangeParameters))]
    public void Constructor_Throws_ArgumentOutOfRangeException(int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            int[] array = new int[length];
            ArrayEnumerator<int> enumerator = new(array, length + 8, length);
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            int[] array = new int[length];
            ArrayEnumerator<int> enumerator = new(array, length - 8, length + 8);
            while (enumerator.MoveNext())
            {
                Assert.Fail();
            }
        });
    }

    [Fact]
    public void Enumerate_Array_Range_Empty_Test()
    {
        ArrayEnumerator<int> enumerator = new([], 0, 0);
        while (enumerator.MoveNext())
        {
            Assert.Fail();
        }
    }

    [Theory]
    [MemberData(nameof(EnumerateParameters))]
    public void Enumerate_List_Test(int length)
    {
        List<int> list = [];
        CollectionsMarshal.SetCount(list, length);
        Random.Shared.Fill(list);

        int counter = 0;
        ArrayEnumerator<int> enumerator = new(list);
        while (enumerator.MoveNext())
        {
            Assert.Equal(list[counter++], enumerator.Current);
        }

        Assert.Equal(counter, list.Count);
    }

    [Fact]
    public void Enumerate_List_Empty_Test()
    {
        ArrayEnumerator<int> enumerator = new(new List<int>());
        while (enumerator.MoveNext())
        {
            Assert.Fail();
        }
    }
}
