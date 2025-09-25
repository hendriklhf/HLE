using System;
using System.Collections.Generic;
using System.Linq;
using HLE.Collections;

namespace HLE.UnitTests.Collections;

public sealed class ArrayEnumeratorTest
{
    private static readonly int[] s_array = Enumerable.Range(0, 16).ToArray();
    private static readonly List<int> s_list = Enumerable.Range(0, 16).ToList();

    [Fact]
    public void Property_Empty_IsEmpty()
    {
        ArrayEnumerator<int> enumerator = ArrayEnumerator<int>.Empty;
        while (enumerator.MoveNext())
        {
            Assert.Fail("MoveNext() should always return false for the empty enumerator.");
        }
    }

    [Fact]
    public void Enumerates_EmptyArray_Correctly()
    {
        int[] array = [];
        ArrayEnumerator<int> enumerator = new(array);
        while (enumerator.MoveNext())
        {
            Assert.Fail("MoveNext() should always return false when constructed with an empty array.");
        }
    }

    [Fact]
    public void Enumerates_NonEmptyArray_Correctly()
    {
        ArrayEnumerator<int> enumerator = new(s_array);
        int index = 0;
        while (enumerator.MoveNext())
        {
            Assert.Equal(s_array[index++], enumerator.Current);
        }

        Assert.Equal(s_array.Length, index);
    }

    [Fact]
    public void Enumerates_NonEmptyArray_WithStartAndEnd_Correctly()
    {
        const int Start = 5;
        const int AdditionalEndOffset = 3;

        int length = s_array.Length - Start - AdditionalEndOffset;
        ArrayEnumerator<int> enumerator = new(s_array, Start, length);
        int index = Start;
        while (enumerator.MoveNext())
        {
            Assert.Equal(s_array[index++], enumerator.Current);
        }

        Assert.Equal(length, index - Start);
        Assert.Equal(s_array.Length - AdditionalEndOffset, index);
    }

    [Fact]
    public void Enumerates_NonEmptyArray_WithStartAndLengthZero_Correctly()
    {
        ArrayEnumerator<int> enumerator = new(s_array, 0, 0);
        while (enumerator.MoveNext())
        {
            Assert.Fail("MoveNext() should always return false when constructed with an empty array.");
        }
    }

    [Fact]
    public void Enumerates_EmptyList_Correctly()
    {
        List<int> list = [];
        ArrayEnumerator<int> enumerator = new(list);
        while (enumerator.MoveNext())
        {
            Assert.Fail("MoveNext() should always return false when constructed with an empty list.");
        }
    }

    [Fact]
    public void Enumerates_NonEmptyList_Correctly()
    {
        ArrayEnumerator<int> enumerator = new(s_list);
        int index = 0;
        while (enumerator.MoveNext())
        {
            Assert.Equal(s_list[index++], enumerator.Current);
        }

        Assert.Equal(s_list.Count, index);
    }

    [Fact]
    public void AddingToTheList_DoesNotChangeEnumeratorBehavior_AfterEnumeratorConstruction()
    {
        List<int> list = [0, 1, 2];
        ArrayEnumerator<int> enumerator = new(list);
        list.AddRange([3, 4, 5]);

        int index = 0;
        while (enumerator.MoveNext())
        {
            Assert.Equal(s_list[index++], enumerator.Current);
        }

        Assert.Equal(list.Count - 3, index);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenStartIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>(s_array, -1, 0);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenArrayIsEmpty_And_StartIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>([], 0, 0);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenLengthIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>(s_array, 0, -1);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenStartAndLengthAreNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>(s_array, -1, -1);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenStartIsGreaterThanLastArrayIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>(s_array, s_array.Length, 0);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenLengthIsGreaterThanArrayLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>(s_array, 0, s_array.Length + 1);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenLengthMinusStartIsTooLong()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>(s_array, 5, s_array.Length - 4);
    }

    [Fact]
    public void Ctor_ThrowsArgumentOutOfRangeException_WhenStartPlusLengthIsTooLong()
    {
        Assert.Throws<ArgumentOutOfRangeException>(Act);

        return;

        static void Act() => _ = new ArrayEnumerator<int>(s_array, 6, s_array.Length - 5);
    }
}
