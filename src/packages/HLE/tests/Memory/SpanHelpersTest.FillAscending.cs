using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using HLE.Collections;
using HLE.Memory;

namespace HLE.UnitTests.Memory;

public sealed partial class SpanHelpersTest
{
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
    public static TheoryData<object> FillAscendingParameters { get; } =
        new(
            (object)(byte)0,
            (byte)128,
            (byte)255,
            (sbyte)-127,
            (sbyte)0,
            (sbyte)120,
            (short)-30_000,
            (short)0,
            (short)30_000,
            (ushort)0,
            (ushort)30_000,
            (ushort)60_000,
            -1_000_000,
            -200,
            0,
            1000,
            1_000_000,
            0U,
            3000U,
            1_000_000U,
            -1_000_000L,
            -5000L,
            0L,
            10_000L,
            1_000_000L,
            0UL,
            10_000UL,
            1_000_000U
        );

    [Theory]
    [MemberData(nameof(FillAscendingParameters))]
    public void FillAscendingTest(object start)
    {
        MethodInfo? fillAscendingCoreMethod = typeof(SpanHelpersTest)
            .GetMethod(nameof(FillAscendingTestCore), BindingFlags.NonPublic | BindingFlags.Static)
            ?.MakeGenericMethod(start.GetType());

        Assert.NotNull(fillAscendingCoreMethod);

        fillAscendingCoreMethod.Invoke(null, [start]);
    }

    private static void FillAscendingTestCore<T>(T start) where T : unmanaged, INumber<T>
    {
        T[] buffer = ArrayPool<T>.Shared.Rent(500_000);
        try
        {
            Span<T> numbers = buffer.AsSpanUnsafe(..500_000);
            SpanHelpers.FillAscending(numbers, start);

            for (int i = 0; i < numbers.Length; i++)
            {
                Assert.Equal(T.CreateTruncating(i) + start, numbers[i]);
            }
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer);
        }
    }
}
