using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HLE.Marshalling.Unix;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
public static partial class Interop
{
    [Pure]
    public static nint GetEnvironment() => _GetEnviron();

    public static void FreeEnvironment(nint environment) => _FreeEnviron(environment);

    [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_GetEnviron")]
    private static partial nint _GetEnviron();

    [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_FreeEnviron")]
    private static partial void _FreeEnviron(nint environ);
}
