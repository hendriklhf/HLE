using System;
using System.IO;
using System.Runtime.InteropServices;
using Iced.Intel;
using static Iced.Intel.AssemblerRegisters;

namespace HLE.Native;

public static unsafe partial class NativeMethods
{
    private static readonly delegate*<byte*, byte*, nuint, void> s_memmove = CreateMemmove();

    private const int VirtualAllocationType = 0x1000;
    private const int VirtualAllocProtectionType = 0x40;

    public static void Memmove(byte* destination, byte* source, nuint byteCount) => s_memmove(destination, source, byteCount >>> 3);

    private static delegate*<byte*, byte*, nuint, void> CreateMemmove()
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
        byte* buffer = (byte*)VirtualAlloc(0, (nuint)stream.Position, VirtualAllocationType, VirtualAllocProtectionType);
        stream.GetBuffer().AsSpan(..(int)stream.Position).CopyTo(new(buffer, (int)stream.Position));
        return buffer;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial nuint VirtualAlloc(nuint address, nuint size, int allocationType, int protectionType);
}
