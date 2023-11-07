using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using HLE.Collections;
using Xunit;

namespace HLE.Tests.Collections;

public sealed partial class SpanHelpersTest
{
    private const int _elementCount = 4096;

#pragma warning disable IDE0300
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
    public static object[][] SumUncheckedParameters { get; } =
    {
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextUInt8()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextInt8()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextUInt16()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextInt16()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextUInt32()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextInt32()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextUInt64()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextInt64()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextUInt128()).ToArray() },
        new object[] { Enumerable.Range(0, _elementCount).Select(static _ => Random.Shared.NextInt128()).ToArray() }
    };
#pragma warning restore IDE0300

    [Theory]
    [MemberData(nameof(SumUncheckedParameters))]
    public void SumUncheckedTest(object values)
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
        => Assert.Equal(Sum(values), SpanHelpers.SumUnchecked<T>(values));

    private static T Sum<T>(T[] values) where T : INumber<T>
    {
        T sum = T.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }
}
