using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HLE.Marshalling.Windows;

[SupportedOSPlatform("windows")]
[SuppressMessage("Minor Code Smell", "S100:Methods and properties should be named in PascalCase")]
public static unsafe partial class Interop
{
    [Pure]
    public static char* GetEnvironmentStrings() => _GetEnvironmentStrings();

    public static bool FreeEnvironmentStrings(char* environmentStrings) => _FreeEnvironmentStrings(environmentStrings);

    public static void* VirtualAlloc(void* address, nuint size, AllocationTypes allocationTypes, ProtectionTypes protectionTypes)
        => _VirtualAlloc(address, size, allocationTypes, protectionTypes);

    public static bool VirtualProtect(void* address, nuint size, ProtectionTypes protectionTypes, ProtectionTypes* oldProtectionTypes)
        => _VirtualProtect(address, size, protectionTypes, oldProtectionTypes);

    public static bool VirtualFree(void* address, nuint size, FreeTypes freeTypes) => _VirtualFree(address, size, freeTypes);

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

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("kernel32.dll", EntryPoint = "GetEnvironmentStringsW")]
    private static partial char* _GetEnvironmentStrings();

    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("kernel32.dll", EntryPoint = "FreeEnvironmentStringsW")]
    private static partial bool _FreeEnvironmentStrings(char* environmentStrings);
}
