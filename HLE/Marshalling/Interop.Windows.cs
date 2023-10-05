using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static partial class Interop
{
    public static unsafe partial class Windows
    {
        [Pure]
        public static char* GetEnvironmentStrings() => _GetEnvironmentStrings();

        public static bool FreeEnvironmentStrings(char* ptr) => _FreeEnvironmentStrings(ptr);

        [LibraryImport("kernel32.dll", EntryPoint = "GetEnvironmentStringsW")]
        private static partial char* _GetEnvironmentStrings();

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("kernel32.dll", EntryPoint = "FreeEnvironmentStringsW")]
        private static partial bool _FreeEnvironmentStrings(char* ptr);
    }
}
