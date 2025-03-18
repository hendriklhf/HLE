using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace HLE.TestUtilities;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")] // remove
public static class TheoryDataHelpers
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<ulong, object>> s_theoryDataCache = new();

    public static TheoryData<T> CreateRange<T>(T min, T max) where T : unmanaged, INumber<T>
    {
        if (!s_theoryDataCache.TryGetValue(typeof(T), out ConcurrentDictionary<ulong, object>? cache))
        {
            cache = new();
            s_theoryDataCache.TryAdd(typeof(T), cache);
        }

        ulong hash = Hash(min, max);
        if (cache.TryGetValue(hash, out object? theoryData))
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
        char[] buffer = ArrayPool<char>.Shared.Rent(maxLength);
        try
        {
            for (int i = 0; i < stringCount; i++)
            {
                int length = Random.Shared.Next(minLength, maxLength);
                Span<char> chars = buffer.AsSpan(..length);
                Random.Shared.NextBytes(MemoryMarshal.Cast<char, byte>(chars));
                data.Add(new(chars));
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }

        return data;
    }
}
