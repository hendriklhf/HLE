using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HLE.InteropServices;

internal static partial class Interop
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    [SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
    public static unsafe partial class Unix
    {
        [Pure]
        public static void* GetEnvironment() => _GetEnviron();

        public static void FreeEnvironment(void* environment) => _FreeEnviron(environment);

        [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_GetEnviron")]
        private static partial void* _GetEnviron();

        [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_FreeEnviron")]
        private static partial void _FreeEnviron(void* environ);
    }
}
