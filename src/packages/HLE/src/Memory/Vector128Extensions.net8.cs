using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

internal static class Vector128Extensions
{
#pragma warning disable IDE0051
    private static ReadOnlySpan<byte> Int8Indices => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
    private static ReadOnlySpan<ushort> Int16Indices => [0, 1, 2, 3, 4, 5, 6, 7];
    private static ReadOnlySpan<uint> Int32Indices => [0, 1, 2, 3];
    private static ReadOnlySpan<ulong> Int64Indices => [0, 1];
#pragma warning restore IDE0051

    extension<T>(Vector128<T>) where T : unmanaged
    {
        public static unsafe Vector128<T> Indices => sizeof(T) switch
        {
            sizeof(byte) => Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(Int8Indices)).As<byte, T>(),
            sizeof(ushort) => Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(Int16Indices)).As<ushort, T>(),
            sizeof(uint) => Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(Int32Indices)).As<uint, T>(),
            sizeof(ulong) => Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(Int64Indices)).As<ulong, T>(),
            _ => throw new UnreachableException()
        };
    }
}
