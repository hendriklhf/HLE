using System;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureValidIntegerType<T>()
    {
        if (typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) &&
            typeof(T) != typeof(short) && typeof(T) != typeof(ushort) &&
            typeof(T) != typeof(int) && typeof(T) != typeof(uint) &&
            typeof(T) != typeof(long) && typeof(T) != typeof(ulong) &&
            typeof(T) != typeof(nint) && typeof(T) != typeof(nuint) &&
            typeof(T) != typeof(char))
        {
            throw new NotSupportedException($"{typeof(T)} is not a valid integer type.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateElementCountType<T>()
    {
        if (typeof(T) != typeof(sbyte) &&
            typeof(T) != typeof(byte) &&
            typeof(T) != typeof(short) &&
            typeof(T) != typeof(ushort) &&
            typeof(T) != typeof(int) &&
            typeof(T) != typeof(uint) &&
            typeof(T) != typeof(long) &&
            typeof(T) != typeof(ulong) &&
            typeof(T) != typeof(nint) &&
            typeof(T) != typeof(nuint))
        {
            throw new NotSupportedException("The element count type must be a signed or unsigned integer type.");
        }
    }
}
