using System;
using System.Runtime.CompilerServices;
using HLE.Marshalling.Windows;
using HLE.Memory;
using HLE.Numerics;

namespace HLE.Marshalling.Asm;

internal static unsafe class MethodAllocator
{
    private static byte* s_buffer = (byte*)MemoryApi.VirtualAlloc(DefaultBufferSize, AllocationType.Commit, ProtectionType.Execute);
    private static nuint s_bufferLength = DefaultBufferSize;
    private static nuint s_bufferPosition;

    private const uint DefaultBufferSize = 1024;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void* Allocate(ReadOnlySpan<byte> code)
    {
        MemoryApi.VirtualProtect(s_buffer, s_bufferLength, ProtectionType.ReadWrite);
        try
        {
            nuint freeBufferSize = GetFreeBufferSize();
            if (freeBufferSize < (uint)code.Length)
            {
                GrowBuffer((uint)code.Length);
            }

            nuint bufferPosition = s_bufferPosition;
            byte* destination = s_buffer + bufferPosition;
            SpanHelpers<byte>.Copy(code, destination);
            s_bufferPosition = NumberHelpers.Align(bufferPosition + (uint)code.Length, (nuint)sizeof(nuint), AlignmentMethod.Add);
            if (bufferPosition >= s_bufferLength)
            {
                GrowBuffer(DefaultBufferSize);
            }

            return destination;
        }
        finally
        {
            MemoryApi.VirtualProtect(s_buffer, s_bufferLength, ProtectionType.Execute);
        }
    }

    private static void GrowBuffer(uint sizeHint)
    {
        nuint bufferLength = s_bufferLength;
        nuint newLength = BufferHelpers.GrowNativeBuffer(bufferLength, sizeHint);
        byte* newBuffer = (byte*)MemoryApi.VirtualAlloc(newLength, AllocationType.Commit, ProtectionType.ReadWrite);
        byte* oldBuffer = s_buffer;
        SpanHelpers.Memmove(newBuffer, oldBuffer, s_bufferPosition);
        MemoryApi.VirtualFree(oldBuffer, bufferLength);
        s_buffer = newBuffer;
        s_bufferLength = newLength;
    }

    private static nuint GetFreeBufferSize() => s_bufferLength - s_bufferPosition;
}
