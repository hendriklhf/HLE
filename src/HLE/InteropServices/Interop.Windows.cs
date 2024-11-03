using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HLE.InteropServices;

internal static partial class Interop
{
    [SupportedOSPlatform("windows")]
    [SuppressMessage("Minor Code Smell", "S100:Methods and properties should be named in PascalCase")]
    public static unsafe partial class Windows
    {
        [Pure]
        public static char* GetEnvironmentStrings() => _GetEnvironmentStrings();

        public static bool FreeEnvironmentStrings(char* environmentStrings) => _FreeEnvironmentStrings(environmentStrings);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport("kernel32.dll", EntryPoint = "GetEnvironmentStringsW")]
        private static partial char* _GetEnvironmentStrings();

        [return: MarshalAs(UnmanagedType.Bool)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport("kernel32.dll", EntryPoint = "FreeEnvironmentStringsW")]
        private static partial bool _FreeEnvironmentStrings(char* environmentStrings);
    }
}
