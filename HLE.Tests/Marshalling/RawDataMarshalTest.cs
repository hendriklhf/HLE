using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
        [new PooledList<int>()],
        [typeof(Assembly).Assembly]
    ];

    [Fact]
    public unsafe void GetRawDataSizeTest()
    {
        nuint size = RawDataMarshal.GetRawDataSize("hello");
        Assert.Equal((nuint)(sizeof(int) + ("hello".Length + 1) * sizeof(char)), size);

        size = RawDataMarshal.GetRawDataSize(new int[5]);
        Assert.Equal((nuint)(sizeof(nuint) + sizeof(int) * 5), size);
    }

    [Theory]
    [MemberData(nameof(MethodTableReferenceTestObjects))]
    public void GetMethodTablePointerTest(object obj)
        => Assert.Equal((nuint)obj.GetType().TypeHandle.Value, RawDataMarshal.GetMethodTableReference(obj));

    [Fact]
    public void ReadObjectTest()
    {
        ref nuint methodTablePointer = ref RawDataMarshal.GetMethodTableReference("hello");
        string? hello = RawDataMarshal.ReadObject<string, nuint>(ref methodTablePointer);
        Assert.Equal("hello", hello);
        Assert.Same("hello", hello);
    }
}
