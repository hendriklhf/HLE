using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Resources;

namespace HLE.Marshalling.Asm;

// volatile: rax, rcx, rdx, r8-r11
// non-volatile: rbx, rbp, rdi, rsi, r12-r15

internal static unsafe class AsmHelpers
{
    private static readonly ResourceReader s_resourceReader = new(typeof(AsmHelpers).Assembly);
    private static readonly delegate*<void*, void*, nuint, void> s_repmovsq = (delegate*<void*, void*, nuint, void>)CreateMethod("Marshalling.Asm.asmhelpers.x64.repmovsq.bin");
    private static readonly delegate*<void*, void*, nuint, void> s_repmovsd = (delegate*<void*, void*, nuint, void>)CreateMethod("Marshalling.Asm.asmhelpers.x86.repmovsd.bin");
    private static readonly delegate*<ulong> s_rdrand64 = (delegate*<ulong>)CreateMethod("Marshalling.Asm.asmhelpers.x64.rdrand64.bin");
    private static readonly delegate*<uint> s_rdrand32 = (delegate*<uint>)CreateMethod("Marshalling.Asm.asmhelpers.x86.rdrand32.bin");
    private static readonly delegate*<ushort> s_rdrand16 = (delegate*<ushort>)CreateMethod("Marshalling.Asm.asmhelpers.x86.rdrand16.bin");
    private static readonly delegate*<byte*, void*, void> s_parseHexColor = (delegate*<byte*, void*, void>)CreateMethod("Marshalling.Asm.asmhelpers.x64.ParseHexColor.bin");

    private static bool IsX64 => RuntimeInformation.ProcessArchitecture is Architecture.X64;

    private static bool IsX86 => RuntimeInformation.ProcessArchitecture is Architecture.X86;

    private static bool IsX64OrX86 => IsX64 || IsX86;

    public static void Repmovsq(void* destination, void* source, nuint byteCount)
    {
        ValidatePlatform(IsX64, Architecture.X64);

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
        ValidatePlatform(IsX86, Architecture.X86);

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

    [Pure]
    public static ulong Rdrand64()
    {
        ValidatePlatform(IsX64, Architecture.X64);
        return s_rdrand64();
    }

    [Pure]
    public static uint Rdrand32()
    {
        ValidatePlatform(IsX64OrX86, Architecture.X86);
        return s_rdrand32();
    }

    [Pure]
    public static ushort Rdrand16()
    {
        ValidatePlatform(IsX64OrX86, Architecture.X86);
        return s_rdrand16();
    }

    [Pure]
    public static byte Rdrand8() => (byte)Rdrand16();

    public static void ParseHexColor(byte* hex, void* colors)
    {
        ValidatePlatform(IsX64, Architecture.X64);
        s_parseHexColor(hex, colors);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidatePlatform(bool isValidArchitecture, [ConstantExpected] Architecture supportedArchitecture)
    {
        if (!isValidArchitecture)
        {
            ThrowPlatformNotSupportedException(supportedArchitecture);
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowPlatformNotSupportedException(Architecture supportedArchitecture) => throw new PlatformNotSupportedException($"The method can only be called on {supportedArchitecture} architectures.");

    private static void* CreateMethod(string name) => MethodAllocator.Allocate(s_resourceReader.Read(name).AsSpan());
}
