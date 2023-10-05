using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static partial class Interop
{
    public static partial class Unix
    {
        [Pure]
        public static nint GetEnvironment() => _GetEnviron();

        public static void FreeEnvironment(nint environment) => _FreeEnviron(environment);

        [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_GetEnviron")]
        private static partial nint _GetEnviron();

        [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_FreeEnviron")]
        private static partial void _FreeEnviron(nint environ);
    }
}
