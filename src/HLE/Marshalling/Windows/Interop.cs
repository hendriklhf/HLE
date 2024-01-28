using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HLE.Marshalling.Windows;

[SupportedOSPlatform("windows")]
public static unsafe partial class Interop
{
    [Pure]
    public static char* GetEnvironmentStrings() => __GetEnvironmentStrings();

    public static bool FreeEnvironmentStrings(char* environmentStrings) => __FreeEnvironmentStrings(environmentStrings);

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
        => __SendInput(count, inputs, sizeof(T));

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("kernel32.dll", EntryPoint = "GetEnvironmentStringsW")]
    private static partial char* __GetEnvironmentStrings();

    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("kernel32.dll", EntryPoint = "FreeEnvironmentStringsW")]
    private static partial bool __FreeEnvironmentStrings(char* environmentStrings);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("user32.dll", EntryPoint = "SendInput")]
    private static partial uint __SendInput(uint inputCount, void* inputs, int sizeOfInput);
}
