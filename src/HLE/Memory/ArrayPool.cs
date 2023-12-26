using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

internal static class ArrayPool
{
    /// <summary>
    /// Stores the maximum amount of arrays per length that the ArrayPool can hold.
    /// </summary>
    public static ReadOnlySpan<int> BucketCapacities => new[]
    {
        // 16,32,64,128,256,512
        512, 512, 512, 512, 256, 256,
        // 1024,2048,4096,8192
        256, 128, 128, 128,
        // 16384,32768,65536,
        64, 64, 32,
        // 131072,262144,524288,1048576
        32, 16, 16, 16,
        // 2097152,4194304,8388608
        8, 8, 8
    };

    public const int MinimumArrayLength = 0x10; // has to be pow of 2
    public const int MaximumArrayLength = 0x800000; // has to be pow of 2
    // ReSharper disable once StaticMemberInGenericType
    public static int IndexOffset { get; } = BitOperations.TrailingZeroCount(MinimumArrayLength);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCommonlyPooledTypeCore<T>() =>
        typeof(T) == typeof(byte) ||
        typeof(T) == typeof(sbyte) ||
        typeof(T) == typeof(short) ||
        typeof(T) == typeof(ushort) ||
        typeof(T) == typeof(int) ||
        typeof(T) == typeof(uint) ||
        typeof(T) == typeof(long) ||
        typeof(T) == typeof(ulong) ||
        typeof(T) == typeof(nint) ||
        typeof(T) == typeof(nuint) ||
        typeof(T) == typeof(bool) ||
        typeof(T) == typeof(string) ||
        typeof(T) == typeof(char) ||
        typeof(T) == typeof(Int128) ||
        typeof(T) == typeof(UInt128) ||
        typeof(T).IsEnum;
}
