using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Marshalling;
using Xunit;

namespace HLE.Tests.Marshalling;

public sealed class RawDataMarshalTest
{
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
    public static object[][] MethodTableReferenceTestObjects { get; } =
    [
        [string.Empty],
        [new int[1]],
        // ReSharper disable once NotDisposedResource
        [new PooledList<int>()],
        [typeof(Assembly).Assembly]
    ];

    [Fact]
    public unsafe void GetRawDataSizeTest()
    {
        nuint size = RawDataMarshal.GetRawObjectSize("hello");
        int expectedSize = sizeof(nuint) + sizeof(nuint) + sizeof(int) + sizeof(char) * 6;
        Assert.Equal((nuint)expectedSize, size);

        size = RawDataMarshal.GetRawObjectSize(new int[5]);
        expectedSize = sizeof(nuint) + sizeof(nuint) + sizeof(nuint) + sizeof(int) * 5;
        Assert.Equal((nuint)expectedSize, size);
    }

    [Theory]
    [MemberData(nameof(MethodTableReferenceTestObjects))]
    public unsafe void GetMethodTableTest(object obj)
        => Assert.True((MethodTable*)obj.GetType().TypeHandle.Value == RawDataMarshal.GetMethodTable(obj));

    [Theory]
    [MemberData(nameof(MethodTableReferenceTestObjects))]
    public unsafe void GetMethodTablePointerTest(object obj)
        => Assert.True((nuint)obj.GetType().TypeHandle.Value == *RawDataMarshal.GetMethodTablePointer(obj));

    [Fact]
    public void ReadObjectTest()
    {
        ref nuint methodTablePointer = ref RawDataMarshal.GetMethodTableReference("hello");
        string hello = RawDataMarshal.ReadObject<string, nuint>(ref methodTablePointer);
        Assert.Equal("hello", hello);
        Assert.Same("hello", hello);
    }

    [Fact]
    public unsafe void GetRawStringDataTest()
    {
        const string Hello = "hello";
        ref RawStringData rawData = ref RawDataMarshal.GetRawStringData(Hello);

        Assert.Equal(typeof(string).TypeHandle.Value, (nint)rawData.MethodTable);
        Assert.Equal(Hello.Length, rawData.Length);
        Assert.Equal(Hello[0], rawData.FirstChar);
        Assert.True(Hello.AsSpan().SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref rawData.FirstChar, Hello.Length)));
    }
}
