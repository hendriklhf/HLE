using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

internal static class ArrayPoolSettings
{
    public static int TrailingZeroCountBucketIndexOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitOperations.TrailingZeroCount(MinimumArrayLength);
    }

    public static TimeSpan TrimmingInterval { get; } = TimeSpan.FromMinutes(1);

    public static TimeSpan MaximumLastAccessTime { get; } = TimeSpan.FromMinutes(2);

    public const int MinimumArrayLength = 0x10; // has to be pow of 2
    public const int MaximumArrayLength = 0x800000; // has to be pow of 2
    public const int MaximumPow2Length = 1 << 30;

    public const double TrimThreshold = 0.725;
    public const double CommonlyPooledTypeTrimThreshold = 0.875;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCommonlyPooledType<T>()
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        => typeof(T) == typeof(byte) ||
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
           typeof(T) == typeof(float) ||
           typeof(T) == typeof(double) ||
           typeof(T) == typeof(decimal) ||
           typeof(T).IsEnum;
}
