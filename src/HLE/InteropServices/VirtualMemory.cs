using System;
using HLE.Marshalling;

namespace HLE.InteropServices;

internal static unsafe class VirtualMemory
{
    public static void* Alloc(nuint size)
    {
        if (OperatingSystem.IsWindows())
        {
            return Interop.Windows.VirtualAlloc(null, size, AllocationTypes.Commit, ProtectionTypes.ReadWrite);
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // TODO
        }

        ThrowHelper.ThrowOperatingSystemNotSupported();
        return null;
    }

    public static void Free(void* address)
    {
        if (OperatingSystem.IsWindows())
        {
            Interop.Windows.VirtualFree(address, 0, FreeTypes.Release);
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // TODO
        }

        ThrowHelper.ThrowOperatingSystemNotSupported();
    }
}
