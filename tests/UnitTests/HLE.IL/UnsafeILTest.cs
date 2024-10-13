using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.TestUtilities;
using Xunit;

namespace HLE.IL.UnitTests;

// ReSharper disable once InconsistentNaming
public sealed unsafe class UnsafeILTest
{
    [Fact]
    public void GetFieldTest()
    {
        TestClass obj = new("hello", 64);

        string str = UnsafeIL.GetField<string>(obj, (nuint)(sizeof(nuint)));
        Assert.Same("hello", str);

        int value = UnsafeIL.GetField<int>(obj, (nuint)(sizeof(nuint) + 8));
        Assert.Equal(64, value);
    }

    [Fact]
    public void GetArrayReferenceTest()
    {
        int[] array = new int[2];
        ref int expected = ref MemoryMarshal.GetArrayDataReference(array);
        ref int actual = ref UnsafeIL.GetArrayReference(array);

        Assert.True(Unsafe.AreSame(ref expected, ref actual));
    }

    [Fact]
    public void GetArrayReference_WithIndex_Test()
    {
        int[] array = new int[8];
        ref int expected = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), 7);
        ref int actual = ref UnsafeIL.GetArrayReference(array, 7);

        Assert.True(Unsafe.AreSame(ref expected, ref actual));
    }

    [Fact]
    public void AsStringTest()
    {
        ref char reference = ref MemoryMarshal.GetReference("hello".AsSpan());
        string str = UnsafeIL.AsString(ref reference);
        Assert.Same("hello", str);
    }

    [Fact]
    public void AsArrayTest()
    {
        int[] expected = new int[2];
        ref int reference = ref MemoryMarshal.GetArrayDataReference(expected);
        int[] actual = UnsafeIL.AsArray(ref reference);
        Assert.Same(expected, actual);
    }

    [Fact]
    public void AsTest()
    {
        object obj = "hello";
        string str = UnsafeIL.As<object, string>(obj);

        Assert.Same(obj, str);
        Assert.Equal("hello", str);
    }

    [Fact]
    public void As_LongToByte()
    {
        const ulong One = 1;
        byte one = UnsafeIL.As<ulong, byte>(One);

        Assert.Equal(1, one);
    }

    [Fact]
    public void AsRefTest()
    {
        const string Hello = "hello";
        ref RawStringData data = ref UnsafeIL.AsRef<string, RawStringData>(Hello);

        Assert.Equal(Hello.Length, data.Length);
        Assert.True(ObjectMarshal.GetMethodTable<string>() == data.MethodTable);
        Assert.Equal(Hello[0], data.FirstChar);
    }

    [Fact]
    public void RefAsTest()
    {
        RawStringData data = new()
        {
            MethodTable = ObjectMarshal.GetMethodTable<string>(),
            Length = 1,
            FirstChar = 'X'
        };

        string str = UnsafeIL.RefAs<RawStringData, string>(ref data);

        Assert.Equal(data.Length, str.Length);
        Assert.True(ObjectMarshal.GetMethodTable<string>() == data.MethodTable);
        Assert.Equal(data.FirstChar, str[0]);
    }

    [Fact]
    public void AsPointerTest()
    {
        const string Hello = "hello";
        RawStringData* data = UnsafeIL.AsPointer<string, RawStringData>(Hello);

        Assert.Equal((nuint)ObjectMarshal.GetMethodTable<string>(), (nuint)data->MethodTable);
        Assert.Equal(Hello.Length, data->Length);
        Assert.Equal(Hello[0], data->FirstChar);
    }

    [Fact]
    public void PointerAsTest()
    {
        RawStringData data = new()
        {
            MethodTable = ObjectMarshal.GetMethodTable<string>(),
            Length = 1,
            FirstChar = 'X'
        };

        string str = UnsafeIL.PointerAs<RawStringData, string>(&data);

        Assert.Equal(data.Length, str.Length);
        Assert.True(data.MethodTable == ObjectMarshal.GetMethodTable(str));
        Assert.Equal(data.FirstChar, str[0]);
    }

    [Fact]
    public void UnboxTest()
    {
        const int Value = int.MaxValue;
        object box = TestHelpers.Box(Value);

        // should not throw as byte != int
        ref byte unbox = ref UnsafeIL.Unbox<byte>(box);

        int unboxedValue = Unsafe.As<byte, int>(ref unbox);
        Assert.Equal(Value, unboxedValue);
    }
}
