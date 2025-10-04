using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.TestUtilities;

namespace HLE.UnitTests.Marshalling;

[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
public sealed unsafe partial class ObjectMarshalTest
{
    public static object[][] GetObjectSizeArrayParameters { get; } =
    [
        [new int[64], (nuint)(ObjectMarshal.BaseObjectSize + sizeof(nuint) + sizeof(int) * 64)],
        [new byte[512], (nuint)(ObjectMarshal.BaseObjectSize + sizeof(nuint) + sizeof(byte) * 512)],
        [new string[16], (nuint)(ObjectMarshal.BaseObjectSize + sizeof(nuint) + sizeof(string) * 16)],
        [new object[32], (nuint)(ObjectMarshal.BaseObjectSize + sizeof(nuint) + sizeof(string) * 32)]
    ];

    public static object[][] GetObjectSizeStringParameters { get; } =
    [
        [""],
        ["iojaowid"],
        ["sjoifsjeoifjsoiejfosf"],
        ["sdjroigjodjrgodrigd"],
        ["wöajfoiajwoidjaoiwjdaowidjaowijd"]
    ];

    public static object[][] GetRawArraySizeParameters { get; } =
    [
        [new byte[16]],
        [new int[32]],
        [new string[64]],
        [new char[128]]
    ];

    [Fact]
    public void BaseObjectSizeTest()
        => Assert.Equal((uint)(sizeof(nuint) + sizeof(nuint)), ObjectMarshal.BaseObjectSize);

    [Theory]
    [MemberData(nameof(GetObjectSizeArrayParameters))]
    public void GetObjectSize_Array_Test(Array array, nuint expectedSize)
        => Assert.Equal(expectedSize, ObjectMarshal.GetObjectSize(array));

    [Theory]
    [MemberData(nameof(GetObjectSizeStringParameters))]
    public void GetObjectSize_String_Test(string str)
    {
        nuint expectedSize = (nuint)(ObjectMarshal.BaseObjectSize + sizeof(int) + sizeof(char) * (str.Length + 1));
        Assert.Equal(expectedSize, ObjectMarshal.GetObjectSize(str));
    }

    [Fact]
    public void GetMethodTableReference_T_Test()
    {
        const string Hello = "hello";
        ref nuint reference = ref ObjectMarshal.GetMethodTableReference(Hello);
        Assert.Equal(typeof(string).TypeHandle.Value, (nint)reference);
    }

    [Fact]
    public void GetMethodTablePointer_T_Test()
    {
        const string Hello = "hello";
        nuint* ptr = ObjectMarshal.GetMethodTablePointer(Hello);
        Assert.Equal(typeof(string).TypeHandle.Value, (nint)(*ptr));
    }

    [Fact]
    public void GetMethodTable_T_Instance_Test()
    {
        const string Hello = "hello";
        MethodTable* mt = ObjectMarshal.GetMethodTable(Hello);
        Assert.Equal(typeof(string).TypeHandle.Value, (nint)mt);
    }

    [Fact]
    public void ReadObject_T_Test()
    {
        const string Hello = "hello";
        string str = ObjectMarshal.ReadObject<string>(ref ObjectMarshal.GetMethodTableReference(Hello));
        Assert.Same(Hello, str);
    }

    [Fact]
    public void ReadObject_TObject_TRef_Test()
    {
        const string Hello = "hello";
        string str = ObjectMarshal.ReadObject<nuint, string>(ref ObjectMarshal.GetMethodTableReference(Hello));
        Assert.Same(Hello, str);
    }

    [Fact]
    public void ReadObject_VoidPtr_Test()
    {
        const string Hello = "hello";
        nuint* ptr = ObjectMarshal.GetMethodTablePointer(Hello);
        string str = ObjectMarshal.ReadObject<string>(ptr);
        Assert.Same(Hello, str);
    }

    [Fact]
    public void ReadObject_MethodTablePointer_Test()
    {
        const string Hello = "hello";
        MethodTable** ptr = (MethodTable**)ObjectMarshal.GetMethodTablePointer(Hello);
        string str = ObjectMarshal.ReadObject<string>(ptr);
        Assert.Same(Hello, str);
    }

    [Fact]
    public void ReadObject_Nuint_Test()
    {
        const string Hello = "hello";
        void* ptr = ObjectMarshal.GetMethodTablePointer(Hello);
        string str = ObjectMarshal.ReadObject<string>((nuint)ptr);
        Assert.Same(Hello, str);
    }

    [Fact]
    public void GetRawStringDataTest()
    {
        const string Hello = "hello";
        ref RawStringData data = ref ObjectMarshal.GetRawStringData(Hello);

        Assert.True(data.MethodTable == ObjectMarshal.GetMethodTable<string>());

        Assert.Equal(Hello.Length, data.Length);
        Assert.Equal(Hello[0], data.FirstChar);

        Assert.True(Unsafe.AreSame(ref StringMarshal.GetReference(Hello), ref data.FirstChar));
        Assert.True(MemoryMarshal.CreateReadOnlySpan(ref data.FirstChar, data.Length).SequenceEqual(Hello));
    }

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void GetRawStringSize_String_Test(string str)
        => Assert.Equal(ObjectMarshal.GetObjectSize(str), ObjectMarshal.GetRawStringSize(str));

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void GetRawStringSize_Int_Test(string str)
        => Assert.Equal(ObjectMarshal.GetObjectSize(str), ObjectMarshal.GetRawStringSize(str.Length));

    [Fact]
    public void GetRawArrayDataTest()
    {
        int[] array = new int[8];
        Random.Shared.Fill(array);

        ref RawArrayData<int> data = ref ObjectMarshal.GetRawArrayData(array);

        Assert.True(data.MethodTable == ObjectMarshal.GetMethodTable<int[]>());

        Assert.Equal((uint)array.Length, data.Length);
        Assert.Equal(array[0], data.FirstElement);

        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(array), ref data.FirstElement));
        Assert.True(MemoryMarshal.CreateReadOnlySpan(ref data.FirstElement, (int)data.Length).SequenceEqual(array));
    }

    [Theory]
    [MemberData(nameof(GetRawArraySizeParameters))]
    [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "testMethod.Invoke asserts")]
    public void GetRawArraySize_Array_Test(Array array)
    {
        MethodInfo method = typeof(ObjectMarshalTest).GetMethod(nameof(GetRawArraySizeTestCore), BindingFlags.NonPublic | BindingFlags.Static)!;
        MethodInfo testMethod = method.MakeGenericMethod(array.GetType().GetElementType()!);
        testMethod.Invoke(null, [array]);
    }

    private static void GetRawArraySizeTestCore<T>(T[] array)
        => Assert.Equal(ObjectMarshal.GetObjectSize(array), ObjectMarshal.GetRawArraySize<T>(array.Length));

    [Fact]
    public void GetMethodTable_Type_Test()
    {
        int[] array = new int[1];
        Assert.True(ObjectMarshal.GetMethodTable<int[]>() == **(MethodTable***)&array);
        Assert.True(ObjectMarshal.GetMethodTable<int[]>() == (MethodTable*)typeof(int[]).TypeHandle.Value);

        string str = "hello";
        Assert.True(ObjectMarshal.GetMethodTable<string>() == **(MethodTable***)&str);
        Assert.True(ObjectMarshal.GetMethodTable<string>() == (MethodTable*)typeof(string).TypeHandle.Value);
    }

    [Fact]
    public void BoxOnStack_Int()
    {
        int value = 89435784;
        object o = ObjectMarshal.BoxOnStack(ref value, out _);
        Assert.True(o is int);
        Assert.Equal(typeof(int), o.GetType());
        Assert.Equal(value, (int)o);
    }

    [Fact]
    public void BoxOnStack_Memory()
    {
        Memory<int> mem = new int[16];
        Random.Shared.Fill(mem.Span);

        object o = ObjectMarshal.BoxOnStack(ref mem, out _);
        Assert.True(o is Memory<int>);
        Assert.Equal(typeof(Memory<int>), o.GetType());
        Assert.Equal(mem, (Memory<int>)o);
        Assert.True(mem.Span.SequenceEqual(((Memory<int>)o).Span));
    }

    [Fact]
    public void Unbox_IntToUInt()
    {
        const int Value = 16;
        object o = TestHelpers.NoInline.Cast<int, object>(Value);
        uint u = ObjectMarshal.Unbox<uint>(o);
        Assert.Equal((uint)Value, u);
    }

    [Fact]
    public void Unbox_MemoryToReadOnlyMemory()
    {
        Memory<int> mem = new int[16];
        Random.Shared.Fill(mem.Span);

        object o = TestHelpers.NoInline.Cast<Memory<int>, object>(mem);
        ReadOnlyMemory<int> rom = ObjectMarshal.Unbox<ReadOnlyMemory<int>>(o);
        Assert.True(mem.Span.SequenceEqual(rom.Span));
    }
}
