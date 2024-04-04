using System;
using System.Diagnostics.CodeAnalysis;
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
            typeof(T) != typeof(nint) && typeof(T) != typeof(nuint))
        {
            ThrowInvalidIntegerType<T>();
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidIntegerType<T>() => throw new NotSupportedException($"{typeof(T)} is not a valid integer type.");
}
