using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

namespace HLE.Native;

public static unsafe class NativeMethods
{
    private static readonly delegate*<byte*, byte*, nuint, void> s_repmovsq = CreateRepMovsq();

    public static void RepMovsq(byte* destination, byte* source, nuint byteCount)
    {
        ValidatePlatform();

        if (byteCount == 0)
        {
            return;
        }

        if ((byteCount & ((uint)sizeof(nuint) - 1)) != 0)
        {
            ThrowByteCountNeedsToBeDivisibleByPointerSize(nameof(byteCount));
        }

        s_repmovsq(destination, source, byteCount >>> (Environment.Is64BitProcess ? 3 : 2));
    }

    private static delegate*<byte*, byte*, nuint, void> CreateRepMovsq()
    {
        Assembler assembler = CreateAssembler();

        assembler.push(rsi);
        assembler.push(rdi);
        assembler.mov(rsi, rdx);
        assembler.mov(rdi, rcx);
        assembler.mov(rcx, r8);
        assembler.rep.movsq();
        assembler.pop(rdi);
        assembler.pop(rsi);
        assembler.ret();

        return (delegate*<byte*, byte*, nuint, void>)CreateDelegate(assembler);
    }

    private static Assembler CreateAssembler() => new(Environment.Is64BitProcess ? 64 : 32);

    private static void* CreateDelegate(Assembler assembler)
    {
        MemoryStream stream = new();
        StreamCodeWriter codeWriter = new(stream);
        assembler.Assemble(codeWriter, 0);
        nuint codeLength = (nuint)stream.Position;
        byte* buffer = MemoryApi.VirtualAlloc(codeLength, AllocationType.Commit, ProtectionType.ExecuteReadWrite);
        stream.GetBuffer().AsSpan(..(int)codeLength).CopyTo(new(buffer, (int)codeLength));
        return buffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidatePlatform()
    {
        if (!Environment.Is64BitProcess || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            ThrowPlatformNotSupported();
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowPlatformNotSupported()
        => throw new PlatformNotSupportedException($"The platform needs to be x64 in order to use methods of {typeof(NativeMethods)}.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowByteCountNeedsToBeDivisibleByPointerSize(string paramName)
        => throw new ArgumentException($"The amount bytes that will be copied needs to be divisible by {sizeof(nuint)}.", paramName);
}
