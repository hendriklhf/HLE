using System;
using HLE.Marshalling;
using Xunit;

namespace HLE.UnitTests.Marshalling;

public sealed unsafe class MethodTableTest
{
    public static TheoryData<(ushort, Type)> ComponentSizeParameters { get; } = CreateComponentSizeParameters();

    [Theory]
    [MemberData(nameof(ComponentSizeParameters))]
    public void ComponentSize_Test((ushort Expected, Type Type) parameter)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(parameter.Type);
        Assert.Equal(parameter.Expected, mt->ComponentSize);
    }

    [Theory]
    [InlineData(typeof(int))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(string))]
    [InlineData(typeof(object))]
    public void IsValueType_Test(Type type)
    {
        MethodTable* mt = ObjectMarshal.GetMethodTableFromType(type);
        Assert.Equal(type.IsValueType, mt->IsValueType);
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
