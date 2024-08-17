using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HLE.Marshalling;
using Xunit;

namespace HLE.UnitTests.Marshalling;

public sealed unsafe partial class MethodTableTest
{
    public static TheoryData<(ushort, Type)> ComponentSizeParameters { get; } = CreateComponentSizeParameters();

    public static TheoryData<Type> MethodTableTypeParameters { get; } =
    [
        typeof(int),
        typeof(Guid),
        typeof(int[]),
        typeof(string[]),
        typeof(Array),
        typeof(string),
        typeof(object),
        typeof(Enum),
        typeof(ValueType),
        typeof(IDisposable),
        typeof(ISpanParsable<int>),
        typeof(ISpanParsable<string>),
        typeof(RuntimeHelpers),
        typeof(ObjectMarshal),
        typeof(StringComparison),
        typeof(RegexOptions),
        typeof(GenericStruct<int>),
        typeof(GenericStruct<string>),
        typeof(GenericRefStruct<int>),
        typeof(GenericRefStruct<string>),
        typeof(GenericRefStructWithRefField<int>),
        typeof(GenericRefStructWithRefField<string>)
    ];

    [Theory]
    [MemberData(nameof(ComponentSizeParameters))]
    public void ComponentSize_Test((ushort Expected, Type Type) parameter)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(parameter.Type);
        Assert.Equal(parameter.Expected, mt->ComponentSize);
    }

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void IsValueType_Test(Type type)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(type);
        Assert.Equal(type.IsValueType, mt->IsValueType);
    }

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void IsReferenceOrContainsReferences_Test(Type type)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(type);

        bool expected = (bool)typeof(RuntimeHelpers).GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))!.MakeGenericMethod(type).Invoke(null, null)!;

        Assert.Equal(expected, mt->IsReferenceOrContainsReferences);
    }

    [Theory]
    [MemberData(nameof(MethodTableTypeParameters))]
    public void IsInterface_Test(Type type)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(type);
        Assert.Equal(type.IsInterface, mt->IsInterface);
    }

    private static TheoryData<(ushort, Type)> CreateComponentSizeParameters()
    {
        // ReSharper disable once UseCollectionExpression
        TheoryData<(ushort, Type)> data = new()
        {
            (sizeof(byte), typeof(byte[])),
            (sizeof(ushort), typeof(ushort[])),
            (sizeof(uint), typeof(uint[])),
            (sizeof(ulong), typeof(ulong[])),
            ((ushort)sizeof(Guid), typeof(Guid[])),
            (sizeof(char), typeof(string))
        };
        return data;
    }
}
