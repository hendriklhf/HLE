using System;
using System.Collections.Concurrent;
using System.IO.Hashing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using Xunit;

namespace HLE.Test.TestUtilities;

public static class TheoryDataHelpers
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<ulong, TheoryData>> s_theoryDataCache = new();

    public static TheoryData<T> CreateRange<T>(T min, T max) where T : unmanaged, INumber<T>
    {
        if (!s_theoryDataCache.TryGetValue(typeof(T), out ConcurrentDictionary<ulong, TheoryData>? cache))
        {
            cache = new();
            s_theoryDataCache.TryAdd(typeof(T), cache);
        }

        ulong hash = Hash(min, max);
        if (cache.TryGetValue(hash, out TheoryData? theoryData))
        {
            return Unsafe.As<TheoryData<T>>(theoryData);
        }

        TheoryData<T> data = new();
        for (T i = min; i <= max; i++)
        {
            data.Add(i);
        }

        cache.TryAdd(hash, data);
        return data;
    }

    private static unsafe ulong Hash<T>(params ReadOnlySpan<T> items) where T : unmanaged
    {
        ReadOnlySpan<byte> bytes = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(items)), sizeof(T) * items.Length);
        return XxHash64.HashToUInt64(bytes);
    }

    public static TheoryData<string> CreateRandomStrings(int stringCount, int minLength, int maxLength)
    {
        TheoryData<string> data = new();
        for (int i = 0; i < stringCount; i++)
        {
            int length = Random.Shared.Next(minLength, maxLength);
            string str = StringMarshal.FastAllocateString(length, out Span<char> chars);
            Random.Shared.Fill(chars);
            data.Add(str);
        }

        return data;
    }
}
