using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using HLE.Marshalling.Windows;

namespace HLE.Marshalling.Asm;

[SuppressMessage("Minor Code Smell", "S100:Methods and properties should be named in PascalCase")]
internal static unsafe partial class MemoryApi
{
    public static void* VirtualAlloc(nuint size, AllocationTypes allocationTypes, ProtectionTypes protectionTypes)
        => _VirtualAlloc((void*)0, size, allocationTypes, protectionTypes);

    public static bool VirtualFree(void* address, nuint size) => _VirtualFree(address, size, FreeTypes.Release);

    public static bool VirtualProtect(void* address, nuint size, ProtectionTypes protectionTypes)
    {
        ProtectionTypes oldProtectionTypes = default;
        return _VirtualProtect(address, size, protectionTypes, &oldProtectionTypes);
    }

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial void* _VirtualAlloc(void* address, nuint size, AllocationTypes allocationTypes, ProtectionTypes protectionTypes);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualFree")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool _VirtualFree(void* address, nuint size, FreeTypes freeTypes);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualProtect")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool _VirtualProtect(void* address, nuint size, ProtectionTypes protectionTypes, ProtectionTypes* oldProtectionType);
}
