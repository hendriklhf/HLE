using System;
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

    public static uint SendInput<T>(ReadOnlySpan<T> inputs) where T : unmanaged, IInput
    {
        fixed (T* ptr = inputs)
        {
            return SendInput(ptr, (uint)inputs.Length);
        }
    }

    public static uint SendInput<T>(T* input) where T : unmanaged, IInput
        => SendInput(input, 1);

    public static uint SendInput<T>(T* inputs, uint count) where T : unmanaged, IInput
        => _SendInput(count, inputs, sizeof(T));

    public static void* VirtualAlloc(void* address, nuint size, AllocationTypes allocationTypes, ProtectionTypes protectionTypes)
        => _VirtualAlloc(address, size, allocationTypes, protectionTypes);

    public static bool VirtualFree(void* address, nuint size, FreeTypes freeTypes) => _VirtualFree(address, size, freeTypes);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial void* _VirtualAlloc(void* address, nuint size, AllocationTypes allocationTypes, ProtectionTypes protectionTypes);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualFree")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool _VirtualFree(void* address, nuint size, FreeTypes freeTypes);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("kernel32.dll", EntryPoint = "GetEnvironmentStringsW")]
    private static partial char* _GetEnvironmentStrings();

    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("kernel32.dll", EntryPoint = "FreeEnvironmentStringsW")]
    private static partial bool _FreeEnvironmentStrings(char* environmentStrings);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll", EntryPoint = "SendInput")]
    private static partial uint _SendInput(uint inputCount, void* inputs, int sizeOfInput);
}
