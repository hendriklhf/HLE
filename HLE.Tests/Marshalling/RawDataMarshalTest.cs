using HLE.Marshalling;
using Xunit;

namespace HLE.Tests.Marshalling;

public sealed class RawDataMarshalTest
{
    [Fact]
    public void GetRawDataSizeTest()
    {
        nuint size = RawDataMarshal.GetRawDataSize("hello");
        Assert.Equal((nuint)(sizeof(int) + ("hello".Length + 1) * sizeof(char)), size);

        size = RawDataMarshal.GetRawDataSize(new int[5]);
        Assert.Equal((nuint)(sizeof(int) + sizeof(int) + sizeof(int) * 5), size);
    }

    [Fact]
    public void GetMethodTablePointerTest()
        => Assert.Equal((nuint)typeof(string).TypeHandle.Value, RawDataMarshal.GetMethodTableReference(string.Empty));

    [Fact]
    public void ReadObjectTest()
    {
        ref nuint methodTablePointer = ref RawDataMarshal.GetMethodTableReference("hello");
        string hello = RawDataMarshal.ReadObject<string, nuint>(ref methodTablePointer);
        Assert.Equal("hello", hello);
        Assert.True(ReferenceEquals("hello", hello));
    }
}
