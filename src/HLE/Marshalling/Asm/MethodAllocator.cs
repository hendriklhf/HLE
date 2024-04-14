using System.Runtime.CompilerServices;
using HLE.Memory;
using HLE.Numerics;
using HLE.Resources;

namespace HLE.Marshalling.Asm;

internal static unsafe class MethodAllocator
{
    private static byte* s_buffer = (byte*)MemoryApi.VirtualAlloc(DefaultBufferSize, AllocationType.Commit, ProtectionType.ExecuteReadWrite);
    private static nuint s_bufferLength = 1024;
    private static nuint s_bufferPosition;

    private const uint DefaultBufferSize = 1024;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void* Allocate(Resource code)
    {
        nuint freeBufferSize = GetFreeBufferSize();
        if (freeBufferSize < (uint)code.Length)
        {
            GrowBuffer((uint)code.Length);
        }

        nuint bufferPosition = s_bufferPosition;
        byte* destination = s_buffer + bufferPosition;
        SpanHelpers<byte>.Copy(code.AsSpan(), destination);
        s_bufferPosition = NumberHelpers.Align<nuint>(bufferPosition + (uint)code.Length, 8, AlignmentMethod.Add);
        if (bufferPosition >= s_bufferLength)
        {
            GrowBuffer(128);
        }

        return destination;
    }

    private static void GrowBuffer(uint sizeHint)
    {
        nuint newLength = BufferHelpers.GrowNativeBuffer(s_bufferLength, sizeHint);
        byte* newBuffer = (byte*)MemoryApi.VirtualAlloc(newLength, AllocationType.Commit, ProtectionType.ExecuteReadWrite);
        SpanHelpers.Memmove(newBuffer, s_buffer, s_bufferPosition);
        s_buffer = newBuffer;
        s_bufferLength = newLength;
    }

    private static nuint GetFreeBufferSize() => s_bufferLength - s_bufferPosition;
}
