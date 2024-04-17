using System.Runtime.InteropServices;
using HLE.Marshalling.Windows;

namespace HLE.Marshalling.Asm;

internal static unsafe partial class MemoryApi
{
    public static void* VirtualAlloc(nuint size, AllocationType allocationType, ProtectionType protectionType)
        => _VirtualAlloc((void*)0, size, allocationType, protectionType);

    public static bool VirtualFree(void* address, nuint size) => _VirtualFree(address, size, FreeType.Release);

    public static bool VirtualProtect(void* address, nuint size, ProtectionType protectionType)
    {
        ProtectionType oldProtectionType = default;
        return _VirtualProtect(address, size, protectionType, &oldProtectionType);
    }

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial void* _VirtualAlloc(void* address, nuint size, AllocationType allocationType, ProtectionType protectionType);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualFree")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool _VirtualFree(void* address, nuint size, FreeType freeType);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualProtect")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool _VirtualProtect(void* address, nuint size, ProtectionType protectionType, ProtectionType* oldProtectionType);
}
