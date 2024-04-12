using System.Runtime.InteropServices;

namespace HLE.Native;

internal static unsafe partial class MemoryApi
{
    public static byte* VirtualAlloc(nuint size, AllocationType allocationType, ProtectionType protectionType)
        => _VirtualAlloc((byte*)0, size, allocationType, protectionType);

    public static bool VirtualFree(byte* address, nuint size) => _VirtualFree(address, size, FreeType.Release);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial byte* _VirtualAlloc(byte* address, nuint size, AllocationType allocationType, ProtectionType protectionType);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualFree")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool _VirtualFree(byte* address, nuint size, FreeType freeType);
}
