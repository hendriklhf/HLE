using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Marshalling;

namespace HLE.UnitTests.Marshalling;

public sealed unsafe partial class MethodTableTest
{
    public static TheoryData<MethodTableTypeParameter> MethodTableTypeParameters { get; } =
    [
        new MethodTableTypeParameter(typeof(int), 0, false),
        new MethodTableTypeParameter(typeof(Guid), 0, false),
        new MethodTableTypeParameter(typeof(byte[]), sizeof(byte), false),
        new MethodTableTypeParameter(typeof(ushort[]), sizeof(ushort), false),
        new MethodTableTypeParameter(typeof(short[]), sizeof(short), false),
        new MethodTableTypeParameter(typeof(int[]), sizeof(int), false),
        new MethodTableTypeParameter(typeof(uint[]), sizeof(uint), false),
        new MethodTableTypeParameter(typeof(ulong[]), sizeof(ulong), false),
        new MethodTableTypeParameter(typeof(long[]), sizeof(long), false),
        new MethodTableTypeParameter(typeof(object[]), (ushort)sizeof(object), true),
        new MethodTableTypeParameter(typeof(string[]), (ushort)sizeof(string), true),
        new MethodTableTypeParameter(typeof(Guid[]), (ushort)sizeof(Guid), false),
        new MethodTableTypeParameter(typeof(Array), 0, false),
        new MethodTableTypeParameter(typeof(string), sizeof(char), false),
        new MethodTableTypeParameter(typeof(object), 0, false),
        new MethodTableTypeParameter(typeof(Enum), 0, false),
        new MethodTableTypeParameter(typeof(ValueType), 0, false),
        new MethodTableTypeParameter(typeof(IDisposable), 0, false),
        new MethodTableTypeParameter(typeof(ISpanParsable<int>), 0, false),
        new MethodTableTypeParameter(typeof(ISpanParsable<string>), 0, false),
        new MethodTableTypeParameter(typeof(RuntimeHelpers), 0, false),
        new MethodTableTypeParameter(typeof(ObjectMarshal), 0, false),
        new MethodTableTypeParameter(typeof(StringComparison), 0, false),
        new MethodTableTypeParameter(typeof(RegexOptions), 0, false),
        new MethodTableTypeParameter(typeof(GenericStruct<int>), 0, false),
        new MethodTableTypeParameter(typeof(GenericStruct<string>), 0, true),
        new MethodTableTypeParameter(typeof(GenericRefStruct<int>), 0, false),
        new MethodTableTypeParameter(typeof(GenericRefStruct<string>), 0, true),
        new MethodTableTypeParameter(typeof(GenericRefStructWithRefField<int>), 0, false),
        new MethodTableTypeParameter(typeof(GenericRefStructWithRefField<string>), 0, false)
    ];

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void ComponentSize(MethodTableTypeParameter parameter)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(parameter.Type);
        if (parameter.Type.IsArray || parameter.Type == typeof(string))
        {
            Assert.True(mt->HasComponentSize);
            Assert.Equal(parameter.ComponentSize, mt->ComponentSize);
        }
        else
        {
            Assert.False(mt->HasComponentSize);
        }
    }

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void IsValueType(MethodTableTypeParameter parameter)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(parameter.Type);
        Assert.Equal(parameter.Type.IsValueType, mt->IsValueType);
    }

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void IsReferenceOrContainsReferences(MethodTableTypeParameter parameter)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(parameter.Type);

        MethodInfo runtimeHelpersIsReferenceOrContainsReference = typeof(RuntimeHelpers)
            .GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!
            .MakeGenericMethod(parameter.Type);

        bool expected = (bool)runtimeHelpersIsReferenceOrContainsReference.Invoke(null, null)!;
        Assert.Equal(expected, ObjectMarshal.IsReferenceOrContainsReferences(mt));
    }

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void IsInterface(MethodTableTypeParameter parameter)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(parameter.Type);
        Assert.Equal(parameter.Type.IsInterface, mt->IsInterface);
    }

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void ContainsManagedPointers(MethodTableTypeParameter parameter)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(parameter.Type);
        Assert.Equal(parameter.ContainsGCPointers, mt->ContainsGCPointers);
    }
}
