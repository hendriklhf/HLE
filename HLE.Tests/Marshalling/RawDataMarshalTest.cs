using System;
using System.Runtime.InteropServices;
using HLE.Collections;
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
    public void GetRawDataTest()
    {
        int[] arr = new int[5];
        arr.AsSpan().FillAscending(1);
        Span<int> data = MemoryMarshal.Cast<byte, int>(RawDataMarshal.GetRawData(arr));
        if (Environment.Is64BitProcess)
        {
            Assert.IsTrue(data is [5, 0, 1, 2, 3, 4, 5]);
        }
        else
        {
            Assert.IsTrue(data is [5, 1, 2, 3, 4, 5]);
        }

        ref byte dataRef = ref RawDataMarshal.GetRawDataReference(arr);
        Span<byte> dataByRef = MemoryMarshal.CreateSpan(ref dataRef, (int)RawDataMarshal.GetRawDataSize(arr));
        Span<int> dataByRefAsInt = MemoryMarshal.Cast<byte, int>(dataByRef);
        Assert.IsTrue(data.SequenceEqual(dataByRefAsInt));
    }

    [TestMethod]
    public void GetObjectFromRawDataTest()
    {
        int[] arr = new int[5];
        Span<byte> data = RawDataMarshal.GetRawData(arr);
        int[] obj = RawDataMarshal.GetObjectFromRawData<int[]>(data);
        Assert.IsTrue(ReferenceEquals(arr, obj));
    }
}
