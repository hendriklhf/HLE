using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using HLE.Numerics;

namespace HLE.Memory;

public static unsafe partial class SpanHelpers
{
    private const nuint MemcpyAlignment = 64;
    private const nuint MemcpyAlignmentThreshold = 256;

    internal static void Memcpy(void* destination, void* source, nuint byteCount)
        => Memcpy(ref Unsafe.AsRef<byte>(destination), ref Unsafe.AsRef<byte>(source), byteCount);

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity")]
    internal static void Memcpy(ref byte destination, ref byte source, nuint byteCount)
    {
        Debug.Assert(byteCount != 0);

        if (byteCount >= MemcpyAlignmentThreshold && !MemoryHelpers.IsAligned(ref destination, MemcpyAlignment))
        {
            MemcpyAlignmentResult alignmentResult = Align(ref source, ref destination, byteCount);
            destination = ref alignmentResult.Destination;
            source = ref alignmentResult.Source;
            byteCount = alignmentResult.ByteCount;
        }

    CheckByteCount:
        // casting to ulong to have a consistent size across 32 and 64 bit processes,
        // as cases expect a 64-bit integer
        switch (BitOperations.LeadingZeroCount((ulong)byteCount))
        {
            case <= 47: // >= 65536
                nuint byteCountBefore = byteCount;
                Copy65536ByteBlocks(ref destination, ref source, ref byteCount);
                if (byteCount == 0)
                {
                    return;
                }

                nuint copiedBytes = byteCountBefore - byteCount;
                destination = ref Unsafe.Add(ref destination, copiedBytes);
                source = ref Unsafe.Add(ref source, copiedBytes);
                goto CheckByteCount;
            case 48: // >= 32768
                Copy32768Bytes__NoInline(ref destination, ref source);
                if (byteCount == 32768)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 32768);
                source = ref Unsafe.Add(ref source, 32768);
                byteCount -= 32768;
                goto CheckByteCount;
            case 49: // >= 16384
                Copy16384Bytes__NoInline(ref destination, ref source);
                if (byteCount == 16384)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 16384);
                source = ref Unsafe.Add(ref source, 16384);
                byteCount -= 16384;
                goto CheckByteCount;
            case 50: // >= 8192
                Copy8192Bytes__NoInline(ref destination, ref source);
                if (byteCount == 8192)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 8192);
                source = ref Unsafe.Add(ref source, 8192);
                byteCount -= 8192;
                goto CheckByteCount;
            case 51: // >= 4096
                Copy4096Bytes__NoInline(ref destination, ref source);
                if (byteCount == 4096)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 4096);
                source = ref Unsafe.Add(ref source, 4096);
                byteCount -= 4096;
                goto CheckByteCount;
            case 52: // >= 2048
                Copy2048Bytes__NoInline(ref destination, ref source);
                if (byteCount == 2048)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 2048);
                source = ref Unsafe.Add(ref source, 2048);
                byteCount -= 2048;
                goto CheckByteCount;
            case 53: // >= 1024
                Copy1024Bytes__NoInline(ref destination, ref source);
                if (byteCount == 1024)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 1024);
                source = ref Unsafe.Add(ref source, 1024);
                byteCount -= 1024;
                goto CheckByteCount;
            case 54: // >= 512
                Copy512Bytes__NoInline(ref destination, ref source);
                if (byteCount == 512)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 512);
                source = ref Unsafe.Add(ref source, 512);
                byteCount -= 512;
                goto CheckByteCount;
            case 55: // >= 256
                Copy256Bytes__NoInline(ref destination, ref source);
                if (byteCount == 256)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 256);
                source = ref Unsafe.Add(ref source, 256);
                byteCount -= 256;
                goto CheckByteCount;
            case 56: // >= 128
                Copy128Bytes__NoInline(ref destination, ref source);
                if (byteCount == 128)
                {
                    return;
                }

                destination = ref Unsafe.Add(ref destination, 128);
                source = ref Unsafe.Add(ref source, 128);
                byteCount -= 128;
                goto CheckByteCount;
            case 57: // >= 64
            {
                Copy64Bytes(ref source, ref destination);
                byteCount -= 64;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 64 - byteCount;
                destination = ref Unsafe.Add(ref destination, 64 - remainder);
                source = ref Unsafe.Add(ref source, 64 - remainder);
                Copy64Bytes(ref source, ref destination);
                return;
            }
            case 58: // >= 32
            {
                Copy32Bytes(ref source, ref destination);
                byteCount -= 32;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 32 - byteCount;
                destination = ref Unsafe.Add(ref destination, 32 - remainder);
                source = ref Unsafe.Add(ref source, 32 - remainder);
                Copy32Bytes(ref source, ref destination);
                return;
            }
            case 59: // >= 16
            {
                Copy16Bytes(ref source, ref destination);
                byteCount -= 16;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 16 - byteCount;
                destination = ref Unsafe.Add(ref destination, 16 - remainder);
                source = ref Unsafe.Add(ref source, 16 - remainder);
                Copy16Bytes(ref source, ref destination);
                return;
            }
            case 60: // >= 8
            {
                Copy8Bytes(ref source, ref destination);
                byteCount -= 8;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 8 - byteCount;
                destination = ref Unsafe.Add(ref destination, 8 - remainder);
                source = ref Unsafe.Add(ref source, 8 - remainder);
                Copy8Bytes(ref source, ref destination);
                return;
            }
        }

        switch (byteCount)
        {
            case 7:
                Copy4Bytes(ref source, ref destination);
                Copy4Bytes(ref Unsafe.Add(ref source, 3), ref Unsafe.Add(ref destination, 3));
                return;
            case 6:
                Copy4Bytes(ref source, ref destination);
                Copy2Bytes(ref Unsafe.Add(ref source, 4), ref Unsafe.Add(ref destination, 4));
                return;
            case 5:
                Copy4Bytes(ref source, ref destination);
                Copy1Byte(ref Unsafe.Add(ref source, 4), ref Unsafe.Add(ref destination, 4));
                return;
            case 4:
                Copy4Bytes(ref source, ref destination);
                return;
            case 3:
                Copy2Bytes(ref source, ref destination);
                Copy1Byte(ref Unsafe.Add(ref source, 2), ref Unsafe.Add(ref destination, 2));
                return;
            case 2:
                Copy2Bytes(ref source, ref destination);
                return;
            case 1:
                Copy1Byte(ref source, ref destination);
                return;
            default:
                ThrowHelper.ThrowUnreachableException();
                return;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy65536ByteBlocks(ref byte destination, ref byte source, ref nuint byteCount)
    {
        Debug.Assert(byteCount >= 65536);

        do
        {
            Copy65536Bytes__NoInline(ref destination, ref source);
            byteCount -= 65536;
            if (byteCount == 0)
            {
                return;
            }

            destination = ref Unsafe.Add(ref destination, 65536);
            source = ref Unsafe.Add(ref source, 65536);
        }
        while (byteCount >= 65536);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy65536Bytes(ref byte source, ref byte destination)
    {
        Copy32768Bytes(ref source, ref destination);
        Copy32768Bytes(ref Unsafe.Add(ref source, 32768), ref Unsafe.Add(ref destination, 32768));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy32768Bytes(ref byte source, ref byte destination)
    {
        Copy16384Bytes(ref source, ref destination);
        Copy16384Bytes(ref Unsafe.Add(ref source, 16384), ref Unsafe.Add(ref destination, 16384));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy16384Bytes(ref byte source, ref byte destination)
    {
        Copy8192Bytes(ref source, ref destination);
        Copy8192Bytes(ref Unsafe.Add(ref source, 8192), ref Unsafe.Add(ref destination, 8192));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy8192Bytes(ref byte source, ref byte destination)
    {
        Copy4096Bytes(ref source, ref destination);
        Copy4096Bytes(ref Unsafe.Add(ref source, 4096), ref Unsafe.Add(ref destination, 4096));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy4096Bytes(ref byte source, ref byte destination)
    {
        Copy2048Bytes(ref source, ref destination);
        Copy2048Bytes(ref Unsafe.Add(ref source, 2048), ref Unsafe.Add(ref destination, 2048));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy2048Bytes(ref byte source, ref byte destination)
    {
        Copy1024Bytes(ref source, ref destination);
        Copy1024Bytes(ref Unsafe.Add(ref source, 1024), ref Unsafe.Add(ref destination, 1024));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy1024Bytes(ref byte source, ref byte destination)
    {
        Copy512Bytes(ref source, ref destination);
        Copy512Bytes(ref Unsafe.Add(ref source, 512), ref Unsafe.Add(ref destination, 512));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy512Bytes(ref byte source, ref byte destination)
    {
        Copy256Bytes(ref source, ref destination);
        Copy256Bytes(ref Unsafe.Add(ref source, 256), ref Unsafe.Add(ref destination, 256));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy256Bytes(ref byte source, ref byte destination)
    {
        Copy128Bytes(ref source, ref destination);
        Copy128Bytes(ref Unsafe.Add(ref source, 128), ref Unsafe.Add(ref destination, 128));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy128Bytes(ref byte source, ref byte destination)
    {
        Copy64Bytes(ref source, ref destination);
        Copy64Bytes(ref Unsafe.Add(ref source, 64), ref Unsafe.Add(ref destination, 64));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy64Bytes(ref byte source, ref byte destination)
    {
        if (Vector512.IsHardwareAccelerated)
        {
            Vector512.LoadUnsafe(ref source).StoreUnsafe(ref destination);
            return;
        }

        Copy32Bytes(ref source, ref destination);
        Copy32Bytes(ref Unsafe.Add(ref source, 32), ref Unsafe.Add(ref destination, 32));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy32Bytes(ref byte source, ref byte destination)
    {
        if (Vector256.IsHardwareAccelerated)
        {
            Vector256.LoadUnsafe(ref source).StoreUnsafe(ref destination);
            return;
        }

        Copy16Bytes(ref source, ref destination);
        Copy16Bytes(ref Unsafe.Add(ref source, 16), ref Unsafe.Add(ref destination, 16));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy16Bytes(ref byte source, ref byte destination)
    {
        if (Vector128.IsHardwareAccelerated)
        {
            Vector128.LoadUnsafe(ref source).StoreUnsafe(ref destination);
            return;
        }

        Copy8Bytes(ref source, ref destination);
        Copy8Bytes(ref Unsafe.Add(ref source, 8), ref Unsafe.Add(ref destination, 8));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy8Bytes(ref byte source, ref byte destination) =>
        Unsafe.As<byte, long>(ref destination) = Unsafe.As<byte, long>(ref source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy4Bytes(ref byte source, ref byte destination) => Unsafe.As<byte, int>(ref destination) = Unsafe.As<byte, int>(ref source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy2Bytes(ref byte source, ref byte destination) =>
        Unsafe.As<byte, short>(ref destination) = Unsafe.As<byte, short>(ref source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once RedundantAssignment
    private static void Copy1Byte(ref byte source, ref byte destination) => destination = source;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static MemcpyAlignmentResult Align(ref byte source, ref byte destination, nuint byteCount)
    {
        Debug.Assert(byteCount >= MemcpyAlignment);

        ref byte alignedDestination = ref MemoryHelpers.Align(ref destination, MemcpyAlignment, AlignmentMethod.Add);
        nuint alignmentDifference = (nuint)Unsafe.AsPointer(ref alignedDestination) - (nuint)Unsafe.AsPointer(ref destination);

        Debug.Assert(alignmentDifference != 0);

        Copy64Bytes(ref source, ref destination);
        return new(
            ref Unsafe.Add(ref source, alignmentDifference),
            ref Unsafe.Add(ref destination, alignmentDifference),
            byteCount - alignmentDifference
        );
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy65536Bytes__NoInline(ref byte destination, ref byte source) => Copy65536Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy32768Bytes__NoInline(ref byte destination, ref byte source) => Copy32768Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy16384Bytes__NoInline(ref byte destination, ref byte source) => Copy16384Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy8192Bytes__NoInline(ref byte destination, ref byte source) => Copy8192Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy4096Bytes__NoInline(ref byte destination, ref byte source) => Copy4096Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy2048Bytes__NoInline(ref byte destination, ref byte source) => Copy2048Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy1024Bytes__NoInline(ref byte destination, ref byte source) => Copy1024Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy512Bytes__NoInline(ref byte destination, ref byte source) => Copy512Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy256Bytes__NoInline(ref byte destination, ref byte source) => Copy256Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy128Bytes__NoInline(ref byte destination, ref byte source) => Copy128Bytes(ref source, ref destination);
}
