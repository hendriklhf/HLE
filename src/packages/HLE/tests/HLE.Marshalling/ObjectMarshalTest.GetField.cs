using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling.UnitTests;

public sealed partial class ObjectMarshalTest
{
    [Fact]
    public void GetFieldTest()
    {
        TestClass obj = new();

        ref List<int> list = ref ObjectMarshal.GetField<List<int>>(obj, 0);
        Assert.Same(list, obj.List);
        Assert.True(Unsafe.AreSame(ref list, in obj.List));

        list = [];
        Assert.Same(list, obj.List);

        ref int i = ref ObjectMarshal.GetField<int>(obj, 8);
        Assert.True(Unsafe.AreSame(ref i, in obj.Int));
        Assert.Equal(obj.Int, i);

        i = 1024;
        Assert.Equal(1024, obj.Int);

        ref string str = ref ObjectMarshal.GetField<string>(obj, 16);
        Assert.True(Unsafe.AreSame(ref str, in obj.Str));
        Assert.Same("hello", str);

        str = "world";
        Assert.Same("world", obj.Str);
    }

    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    private sealed class TestClass
    {
        [FieldOffset(0)]
        public readonly List<int> List = [1, 2, 3];

        [FieldOffset(8)]
        public readonly int Int = 512;

        [FieldOffset(16)]
        public readonly string Str = "hello";
    }
}
