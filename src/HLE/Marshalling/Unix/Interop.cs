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
    public static nint GetEnvironment() => __GetEnviron();

    public static void FreeEnvironment(nint environment) => __FreeEnviron(environment);

    [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_GetEnviron")]
    private static partial nint __GetEnviron();

    [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_FreeEnviron")]
    private static partial void __FreeEnviron(nint environ);
}
