using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using HLE.Collections;
using HLE.Memory;
using Xunit;

namespace HLE.UnitTests.Memory;

public sealed partial class SpanHelpersTest
{
    public static TheoryData<Array> IndicesOfParameters { get; } = CreateIndicesOfParameters();

    private static ReadOnlySpan<byte> Int8FillValues => [0, 1];

    private static ReadOnlySpan<ushort> Int16FillValues => [0, 1];

    private static ReadOnlySpan<uint> Int32FillValues => [0, 1];

    private static ReadOnlySpan<ulong> Int64FillValues => [0, 1];

    private static readonly ConcurrentDictionary<Type, MethodInfo> s_indicesOfCoreMethodCache = new();

    [Theory]
    [MemberData(nameof(IndicesOfParameters))]
    public unsafe void IndicesOfTest(Array items)
    {
        Type? elementType = items.GetType().GetElementType();
        Assert.NotNull(elementType);

        if (!s_indicesOfCoreMethodCache.TryGetValue(elementType, out MethodInfo? method))
        {
            method = typeof(SpanHelpersTest).GetMethod(nameof(IndicesOfCore), BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);
            method = method.MakeGenericMethod(elementType);
            s_indicesOfCoreMethodCache.TryAdd(elementType, method);
        }

        delegate*<Array, void> indicesOfCore = (delegate*<Array, void>)method.MethodHandle.GetFunctionPointer();
        indicesOfCore(items);
    }

    private static unsafe void IndicesOfCore<T>(T[] items) where T : struct, IEquatable<T>
    {
        // ReSharper disable once NotDisposedResource (it is disposed. can't use a "using" statement, as "loopedIndices" is passed by mutable ref)
        ValueList<int> loopedIndices = new(items.Length);
        try
        {
            ulong longOne = 1;
            T one = *(T*)&longOne;
            GetLoopedIndices<T>(items, ref loopedIndices);
            int[] indicesBuffer = ArrayPool<int>.Shared.Rent(items.Length);
            try
            {
                int indicesCount = SpanHelpers.IndicesOf(items, one, indicesBuffer.AsSpan());

                ReadOnlySpan<int> indices = indicesBuffer.AsSpanUnsafe(..indicesCount);
                Assert.True(indices.SequenceEqual(loopedIndices.AsSpan()));
            }
            finally
            {
                ArrayPool<int>.Shared.Return(indicesBuffer);
            }
        }
        finally
        {
            loopedIndices.Dispose();
        }
    }

    private static unsafe void GetLoopedIndices<T>(ReadOnlySpan<T> items, ref ValueList<int> indices) where T : struct, IEquatable<T>
    {
        ulong longOne = 1;
        T one = *(T*)&longOne;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Equals(one))
            {
                indices.Add(i);
            }
        }
    }

    private static TheoryData<Array> CreateIndicesOfParameters()
    {
        ReadOnlySpan<Type> elementTypes =
        [
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(char)
        ];

        TheoryData<Array> data = new();
        Dictionary<Type, MethodInfo> fillMethodCache = new(elementTypes.Length);

        for (int elementCount = 0; elementCount <= 1024; elementCount++)
        {
            foreach (Type elementType in elementTypes)
            {
                Array array = Array.CreateInstance(elementType, elementCount);
                Fill(array, elementType, fillMethodCache);
                data.Add(array);
            }
        }

        return data;
    }

    private static unsafe void Fill(Array array, Type elementType, Dictionary<Type, MethodInfo> cache)
    {
        if (!cache.TryGetValue(elementType, out MethodInfo? method))
        {
            method = typeof(SpanHelpersTest).GetMethod(nameof(FillCore), BindingFlags.NonPublic | BindingFlags.Static)!;
            method = method.MakeGenericMethod(elementType);
            cache.Add(elementType, method);
        }

        delegate*<Array, void> fillCore = (delegate*<Array, void>)method.MethodHandle.GetFunctionPointer();
        fillCore(array);
    }

    private static void FillCore<T>(T[] array) where T : struct
        => Random.Shared.Fill(array, GetFillValues<T>());

    private static unsafe ReadOnlySpan<T> GetFillValues<T>() where T : struct
        => sizeof(T) switch
        {
            sizeof(byte) => MemoryMarshal.Cast<byte, T>(Int8FillValues),
            sizeof(ushort) => MemoryMarshal.Cast<ushort, T>(Int16FillValues),
            sizeof(uint) => MemoryMarshal.Cast<uint, T>(Int32FillValues),
            sizeof(ulong) => MemoryMarshal.Cast<ulong, T>(Int64FillValues),
            _ => throw new NotSupportedException()
        };
}
