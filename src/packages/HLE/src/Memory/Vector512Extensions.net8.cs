using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

internal static class Vector512Extensions
{
#pragma warning disable IDE0051
    private static ReadOnlySpan<byte> Int8Indices => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63];
    private static ReadOnlySpan<ushort> Int16Indices => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31];
    private static ReadOnlySpan<uint> Int32Indices => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
    private static ReadOnlySpan<ulong> Int64Indices => [0, 1, 2, 3, 4, 5, 6, 7];
#pragma warning restore IDE0051

    extension<T>(Vector512<T>) where T : unmanaged
    {
        public static unsafe Vector512<T> Indices => sizeof(T) switch
        {
            sizeof(byte) => Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(Int8Indices)).As<byte, T>(),
            sizeof(ushort) => Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(Int16Indices)).As<ushort, T>(),
            sizeof(uint) => Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(Int32Indices)).As<uint, T>(),
            sizeof(ulong) => Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(Int64Indices)).As<ulong, T>(),
            _ => throw new UnreachableException()
        };
    }
}
