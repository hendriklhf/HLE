using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xunit;

namespace HLE.Memory.UnitTests;

public sealed partial class SpanHelpersTest
{
    public static TheoryData<Array> SumUncheckedParameters { get; } = CreateSumUncheckedParameters();

    [Theory]
    [MemberData(nameof(SumUncheckedParameters))]
    public void SumUncheckedTest(Array values)
    {
        Type? arrayElementType = values.GetType().GetElementType();
        Assert.NotNull(arrayElementType);

        MethodInfo? sumUncheckedCoreMethod = typeof(SpanHelpersTest)
            .GetMethod(nameof(SumUncheckedCore), BindingFlags.NonPublic | BindingFlags.Static)
            ?.MakeGenericMethod(arrayElementType);

        Assert.NotNull(sumUncheckedCoreMethod);

        sumUncheckedCoreMethod.Invoke(null, [values]);
    }

    private static void SumUncheckedCore<T>(T[] values) where T : IBinaryInteger<T>
        => Assert.Equal(Sum(values), SpanHelpers.SumUnchecked(values));

    private static T Sum<T>(T[] values) where T : INumber<T>
    {
        T sum = T.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    private static TheoryData<Array> CreateSumUncheckedParameters()
    {
        ReadOnlySpan<int> elementCounts = [0, 1, 2, 3, 4, 5, 6, 7, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096];
        ReadOnlySpan<Type> elementTypes =
        [
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(Int128), typeof(UInt128), typeof(char)
        ];

        TheoryData<Array> data = new();

        foreach (int elementCount in elementCounts)
        {
            foreach (Type elementType in elementTypes)
            {
                Array array = Array.CreateInstance(elementType, elementCount);
                Random.Shared.Fill(array);
                data.Add(array);
            }
        }

        ReadOnlySpan<int> randomElementCounts = Enumerable.Range(0, 16).Select(static _ => Random.Shared.Next(32, 1024)).ToArray();
        foreach (int elementCount in randomElementCounts)
        {
            foreach (Type elementType in elementTypes)
            {
                Array array = Array.CreateInstance(elementType, elementCount);
                Random.Shared.Fill(array);
                data.Add(array);
            }
        }

        return data;
    }
}
