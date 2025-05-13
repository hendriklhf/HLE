using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

internal static class Vector256Extensions
{
    private static ReadOnlySpan<byte> Int8Indices => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31];
    private static ReadOnlySpan<ushort> Int16Indices => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
    private static ReadOnlySpan<uint> Int32Indices => [0, 1, 2, 3, 4, 5, 6, 7];
    private static ReadOnlySpan<ulong> Int64Indices => [0, 1, 2, 3];

    extension<T>(Vector256<T>) where T : unmanaged
    {
        public static unsafe Vector256<T> Indices => sizeof(T) switch
        {
            sizeof(byte) => Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(Int8Indices)).As<byte, T>(),
            sizeof(ushort) => Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(Int16Indices)).As<ushort, T>(),
            sizeof(uint) => Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(Int32Indices)).As<uint, T>(),
            sizeof(ulong) => Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(Int64Indices)).As<ulong, T>(),
            _ => throw new UnreachableException()
        };
    }
}
