using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using Xunit;

namespace HLE.UnitTests.Marshalling;

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

    [Fact]
    public void ListsDefaultArrayIsEmptyAndAlwaysSameInstance()
    {
        // this is testing external code, but current implementations assert that
        // no new array is allocated for an empty list and the same empty array is used every time

        List<int> l1 = [];
        List<int> l2 = [];

        int[] a1 = ListMarshal.GetArray(l1);
        int[] a2 = ListMarshal.GetArray(l2);

        Assert.Same(a1, a2);
        Assert.Empty(a1);
        Assert.Empty(a2); // actually just one array has to be tested to be empty, if Assert.Same already succeeded
    }

    [Fact]
    public void SetCount_Test()
    {
        List<int> list = [];
        ListMarshal.SetArray(list, new int[8]);
        ListMarshal.SetCount(list, 8);

        Assert.Equal(8, list.Count);
        Assert.Equal(8, list.Capacity);
    }

    [Fact]
    public void SetCount_ThrowsArgumentOutOfRangeException_Test()
    {
        List<int> list = [];
        Assert.Throws<ArgumentOutOfRangeException>(() => ListMarshal.SetCount(list, 1));
    }

    [Fact]
    public void ConstructList_ReadOnlySpan_Test()
    {
        ReadOnlySpan<int> items = [0, 1, 2, 3, 4];
        List<int> list = ListMarshal.ConstructList(items);

        Assert.True(items.SequenceEqual(CollectionsMarshal.AsSpan(list)));
        Assert.True(list.Capacity >= items.Length);
    }

    [Fact]
    public void ConstructList_Span_Test()
    {
        Span<int> items = [0, 1, 2, 3, 4];
        List<int> list = ListMarshal.ConstructList(items);

        Assert.True(items.SequenceEqual(CollectionsMarshal.AsSpan(list)));
        Assert.True(list.Capacity >= items.Length);
    }

    [Fact]
    public void ConstructList_Array_Test()
    {
        int[] items = [0, 1, 2, 3, 4];
        List<int> list = ListMarshal.ConstructList(items);

        Assert.True(items.AsSpan().SequenceEqual(CollectionsMarshal.AsSpan(list)));
        Assert.True(list.Capacity >= items.Length);
    }

    [Fact]
    public void ConstructList_List_Test()
    {
        List<int> items = [0, 1, 2, 3, 4];
        List<int> list = ListMarshal.ConstructList(items);

        Assert.True(CollectionsMarshal.AsSpan(items).SequenceEqual(CollectionsMarshal.AsSpan(list)));
        Assert.True(list.Capacity >= items.Count);
    }

    [Fact]
    public void ConstructList_Ref_Test()
    {
        ReadOnlySpan<int> items = [0, 1, 2, 3, 4];
        List<int> list = ListMarshal.ConstructList(ref MemoryMarshal.GetReference(items), items.Length);

        Assert.True(items.SequenceEqual(CollectionsMarshal.AsSpan(list)));
        Assert.True(list.Capacity >= items.Length);
    }
}
