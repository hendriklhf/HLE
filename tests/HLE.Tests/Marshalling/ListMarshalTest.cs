using System;
using System.Collections.Generic;
using HLE.Marshalling;
using Xunit;

namespace HLE.Tests.Marshalling;

public sealed class ListMarshalTest
{
    [Fact]
    public void AsMemory_Int_Test()
    {
        List<int> intList = [0, 1, 2, 3, 4, 5];
        Memory<int> intMemory = ListMarshal.AsMemory(intList);

        Assert.Equal(intList.Count, intMemory.Length);
        Assert.True(intMemory.Span is [0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void AsMemory_String_Test()
    {
        List<string> stringList = ["a", "b", "c"];
        Memory<string> stringMemory = ListMarshal.AsMemory(stringList);

        Assert.Equal(stringList.Count, stringMemory.Length);
        Assert.True(stringMemory.Span is ["a", "b", "c"]);
    }

    [Fact]
    public void AsArray_Int_Test()
    {
        List<int> intList = [0, 1, 2, 3, 4, 5];
        int[] intArray = ListMarshal.AsArray(intList);

        Assert.Equal(intList.Capacity, intArray.Length);
        Assert.True(intArray.AsSpan(..6) is [0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void AsArray_String_Test()
    {
        List<string> stringList = ["a", "b", "c"];
        string[] stringArray = ListMarshal.AsArray(stringList);

        Assert.Equal(stringList.Capacity, stringArray.Length);
        Assert.True(stringArray.AsSpan(..3) is ["a", "b", "c"]);
    }
}
