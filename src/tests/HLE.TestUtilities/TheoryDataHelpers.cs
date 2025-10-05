using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using HLE.RemoteExecution;
using Xunit;

namespace HLE.TestUtilities;

[SuppressMessage("Maintainability", "CA1515:Consider making public types internal")] // remove
public static class TheoryDataHelpers
{
    public static TheoryData<RemoteExecutorOptions> VectorExecutionOptions { get; } = CreateVectorExecutionOptions();

    public static TheoryData<RemoteExecutorOptions> ProcessorCountOptions { get; } = CreateProcessorCountOptions();

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
                data.Add(new string(chars));
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }

        return data;
    }

    public static TheoryData<(TFirst, TSecond)> CreateMatrix<TFirst, TSecond>(TheoryData<TFirst> first, TheoryData<TSecond> second)
    {
        TheoryData<(TFirst, TSecond)> data = new();
        foreach (TFirst item1 in first)
        {
            foreach (TSecond item2 in second)
            {
                data.Add((item1, item2));
            }
        }

        return data;
    }

    private static TheoryData<RemoteExecutorOptions> CreateVectorExecutionOptions()
    {
        TheoryData<RemoteExecutorOptions> data =
        [
            new RemoteExecutorOptions(),
            new RemoteExecutorOptions
            {
                EnvironmentVariables =
                {
                    { "DOTNET_EnableHWIntrinsic", "0" }
                }
            }
        ];

        if (RuntimeInformation.ProcessArchitecture is Architecture.X64 or Architecture.X86)
        {
            AddXArchOptions(data);
        }

        if (RuntimeInformation.ProcessArchitecture is Architecture.Arm64 or Architecture.Arm)
        {
            AddArmOptions(data);
        }

        return data;
    }

    private static void AddXArchOptions(TheoryData<RemoteExecutorOptions> data)
    {
        if (Avx512F.IsSupported)
        {
            data.Add(new RemoteExecutorOptions
            {
                EnvironmentVariables =
                {
                    { "DOTNET_EnableAVX512", "0" }
                }
            });
        }

        if (Avx2.IsSupported)
        {
            data.Add(new RemoteExecutorOptions
            {
                EnvironmentVariables =
                {
                    { "DOTNET_EnableAVX512", "0" },
                    { "DOTNET_EnableAVX2", "0" }
                }
            });
        }

        if (Avx.IsSupported)
        {
            data.Add(new RemoteExecutorOptions
            {
                EnvironmentVariables =
                {
                    { "DOTNET_EnableAVX512", "0" },
                    { "DOTNET_EnableAVX2", "0" },
                    { "DOTNET_EnableAVX", "0" }
                }
            });
        }
    }

    private static void AddArmOptions(TheoryData<RemoteExecutorOptions> data)
    {
        // TODO: implement
        _ = data;
        _ = 1;
    }

    private static TheoryData<RemoteExecutorOptions> CreateProcessorCountOptions()
    {
        TheoryData<RemoteExecutorOptions> data =
        [
            new RemoteExecutorOptions
            {
                EnvironmentVariables =
                {
                    { "DOTNET_PROCESSOR_COUNT", "1" }
                }
            }
        ];

        if (Environment.ProcessorCount != 1)
        {
            data.Add(new RemoteExecutorOptions
            {
                EnvironmentVariables =
                {
                    { "DOTNET_PROCESSOR_COUNT", (Environment.ProcessorCount / 2).ToString() }
                }
            });

            data.Add(new RemoteExecutorOptions
            {
                EnvironmentVariables =
                {
                    { "DOTNET_PROCESSOR_COUNT", Environment.ProcessorCount.ToString() }
                }
            });
        }

        return data;
    }
}
