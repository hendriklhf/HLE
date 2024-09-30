using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace HLE.Marshalling;

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

        [Pure]
        public static void* Mmap(void* address, nuint length, uint protection, uint flags, uint fd, uint offset)
            => _Mmap(address, length, protection, flags, fd, offset);

        [LibraryImport("libc", EntryPoint = "mmap")]
        private static partial void* _Mmap(void* address, nuint length, uint protection, uint flags, uint fd, uint offset);

        [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_GetEnviron")]
        private static partial void* _GetEnviron();

        [LibraryImport("libSystem.Native", EntryPoint = "SystemNative_FreeEnviron")]
        private static partial void _FreeEnviron(void* environ);
    }
}
