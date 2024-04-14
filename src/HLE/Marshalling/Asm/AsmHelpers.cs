using System;
using System.Runtime.InteropServices;
using HLE.Resources;

namespace HLE.Marshalling.Asm;

// volatile: rax, rcx, rdx, r8-r11
// non-volatile: rbx, rbp, rdi, rsi, r12-r15

public static unsafe class AsmHelpers
{
    private static readonly ResourceReader s_resourceReader = new(typeof(AsmHelpers).Assembly);
    private static readonly delegate*<void*, void*, nuint, void> s_repmovsq = (delegate*<void*, void*, nuint, void>)CreateMethod("Marshalling.Asm.asmhelpers.x64.repmovsq.bin");
    private static readonly delegate*<void*, void*, nuint, void> s_repmovsd = (delegate*<void*, void*, nuint, void>)CreateMethod("Marshalling.Asm.asmhelpers.x86.repmovsd.bin");

    public static void Repmovsq(void* destination, void* source, nuint byteCount)
    {
        if (!Environment.Is64BitProcess || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException();
        }

        if (byteCount == 0)
        {
            return;
        }

        if (byteCount % (uint)sizeof(nuint) != 0)
        {
            throw new InvalidOperationException();
        }

        s_repmovsq(destination, source, byteCount >>> 3);
    }

    public static void Repmovsd(void* destination, void* source, nuint byteCount)
    {
        if (Environment.Is64BitProcess || RuntimeInformation.ProcessArchitecture != Architecture.X86)
        {
            throw new PlatformNotSupportedException();
        }

        if (byteCount == 0)
        {
            return;
        }

        if (byteCount % (uint)sizeof(nuint) != 0)
        {
            throw new InvalidOperationException();
        }

        s_repmovsd(destination, source, byteCount >>> 2);
    }

    private static void* CreateMethod(string name) => MethodAllocator.Allocate(s_resourceReader.Read(name).AsSpan());
}
