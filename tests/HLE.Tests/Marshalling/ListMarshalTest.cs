using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using Xunit;

namespace HLE.Tests.Marshalling;

public sealed class ListMarshalTest
{
    [Fact]
    public void AsMemory_Test()
    {
        List<int> list = [0, 1, 2, 3, 4, 5];
        Memory<int> memory = ListMarshal.AsMemory(list);

        Assert.Equal(list.Count, memory.Length);
        Assert.True(memory.Span is [0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void AsMemory_Empty_Test()
    {
        List<int> list = [];
        Memory<int> memory = ListMarshal.AsMemory(list);

        Assert.Equal(list.Count, memory.Length);
        Assert.Equal(0, memory.Length);
    }

    [Fact]
    public void AsArray_Test()
    {
        List<int> list = [0, 1, 2, 3, 4, 5];
        int[] array = ListMarshal.GetArray(list);

        Assert.Equal(list.Capacity, array.Length);
        Assert.True(array.AsSpan(..6) is [0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void AsArray_Empty_Test()
    {
        List<int> intList = [];
        int[] intArray = ListMarshal.GetArray(intList);

        Assert.Equal(intList.Capacity, intArray.Length);
        Assert.Empty(intArray);
    }

    [Fact]
    public void GetReference_Test()
    {
        List<int> list = [8, 1, 2, 3, 4, 5];
        ref int reference = ref ListMarshal.GetReference(list);

        Assert.Equal(8, reference);

        reference = 0;
        Assert.Equal(0, list[0]);
    }

    [Fact]
    public void GetReference_ThrowsOnEmptyArray_Test() =>
        Assert.Throws<InvalidOperationException>(static () =>
        {
            List<int> list = [];
            _ = ref ListMarshal.GetReference(list);
        });

    [Fact]
    public void GetReference_DoesntThrowOnEmptyList_Test()
    {
        List<int> list = [1];
        list.Remove(1);
        ref int i = ref ListMarshal.GetReference(list);
        i = 5;
        CollectionsMarshal.SetCount(list, 1);
        Assert.Equal(5, list[0]);
    }

    [Fact]
    public void SetArray_Test()
    {
        List<int> list = [0, 1, 2, 3];
        int[] array = new int[8];
        Random.Shared.Fill(array);
        ListMarshal.SetArray(list, array);

        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(array), ref ListMarshal.GetReference(list)));
        Assert.True(CollectionsMarshal.AsSpan(list).SequenceEqual(array.AsSpan(..list.Count)));
    }

    [Fact]
    public void SetArray_ThrowsArgumentOutOfRangeException_Test() =>
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
        {
            List<int> list = [0, 1, 2, 3, 4, 5, 6, 7];
            int[] array = new int[4];
            ListMarshal.SetArray(list, array);
        });
}
