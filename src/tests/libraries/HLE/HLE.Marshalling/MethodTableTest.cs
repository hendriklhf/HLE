using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xunit;

namespace HLE.Marshalling.UnitTests;

public sealed unsafe partial class MethodTableTest
{
    public static TheoryData<MethodTableTypeParameter> MethodTableTypeParameters { get; } =
    [
        new(typeof(int), 0, false),
        new(typeof(Guid), 0, false),
        new(typeof(byte[]), sizeof(byte), false),
        new(typeof(ushort[]), sizeof(ushort), false),
        new(typeof(short[]), sizeof(short), false),
        new(typeof(int[]), sizeof(int), false),
        new(typeof(uint[]), sizeof(uint), false),
        new(typeof(ulong[]), sizeof(ulong), false),
        new(typeof(long[]), sizeof(long), false),
        new(typeof(object[]), (ushort)sizeof(object), true),
        new(typeof(string[]), (ushort)sizeof(string), true),
        new(typeof(Guid[]), (ushort)sizeof(Guid), false),
        new(typeof(Array), 0, false),
        new(typeof(string), sizeof(char), false),
        new(typeof(object), 0, false),
        new(typeof(Enum), 0, false),
        new(typeof(ValueType), 0, false),
        new(typeof(IDisposable), 0, false),
        new(typeof(ISpanParsable<int>), 0, false),
        new(typeof(ISpanParsable<string>), 0, false),
        new(typeof(RuntimeHelpers), 0, false),
        new(typeof(ObjectMarshal), 0, false),
        new(typeof(StringComparison), 0, false),
        new(typeof(RegexOptions), 0, false),
        new(typeof(GenericStruct<int>), 0, false),
        new(typeof(GenericStruct<string>), 0, true),
        new(typeof(GenericRefStruct<int>), 0, false),
        new(typeof(GenericRefStruct<string>), 0, true),
        new(typeof(GenericRefStructWithRefField<int>), 0, false),
        new(typeof(GenericRefStructWithRefField<string>), 0, false)
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
