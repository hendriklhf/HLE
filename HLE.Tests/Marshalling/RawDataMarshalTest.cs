using HLE.Marshalling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HLE.Tests.Marshalling;

[TestClass]
public class RawDataMarshalTest
{
    [TestMethod]
    public void GetRawDataSizeTest()
    {
        nuint size = RawDataMarshal.GetRawDataSize("hello");
        Assert.AreEqual((nuint)(sizeof(int) + ("hello".Length + 1) * sizeof(char)), size);

        size = RawDataMarshal.GetRawDataSize(new int[5]);
        Assert.AreEqual((nuint)(sizeof(int) + sizeof(int) + sizeof(int) * 5), size);
    }

    [TestMethod]
    public void GetMethodTablePointerTest()
        => Assert.AreEqual((nuint)typeof(string).TypeHandle.Value, RawDataMarshal.GetMethodTableReference(string.Empty));

    [TestMethod]
    public void ReadObjectTest()
    {
        ref nuint methodTablePointer = ref RawDataMarshal.GetMethodTableReference("hello");
        string hello = RawDataMarshal.ReadObject<string, nuint>(ref methodTablePointer);
        Assert.AreEqual("hello", hello);
        Assert.IsTrue(ReferenceEquals("hello", hello));
    }
}
