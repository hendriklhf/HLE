using System;
using HLE.Marshalling.Windows;

namespace HLE.Marshalling;

internal static unsafe class VirtualMemory
{
    public static void* Alloc(nuint size)
    {
        if (OperatingSystem.IsWindows())
        {
            return Interop.VirtualAlloc(null, size, AllocationTypes.Commit, ProtectionTypes.ExecuteReadWrite);
        }

        ThrowHelper.ThrowOperatingSystemNotSupported();
        return null;
    }

    public static void Free(void* address)
    {
        if (OperatingSystem.IsWindows())
        {
            Interop.VirtualFree(address, 0, FreeTypes.Release);
        }

        ThrowHelper.ThrowOperatingSystemNotSupported();
    }
}
