using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Numerics;

namespace HLE.Memory;

[SkipLocalsInit]
public static unsafe partial class SpanHelpers<T>
{
    /// <summary>
    /// <c>Memmove(ref T destination, ref T source, nuint elementCount)</c>
    /// </summary>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "exactly what i want")]
    private static readonly delegate*<ref T, ref T, nuint, void> s_memmove = GetMemmoveFunctionPointer();

    private const nuint AlignmentThreshold = 256; // if changed,  label "Copy128" also needs to be fixed

    private static delegate*<ref T, ref T, nuint, void> GetMemmoveFunctionPointer()
    {
        MethodInfo? memmove = Array.Find(
            typeof(Buffer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static),
            static m => m is { Name: "Memmove", IsGenericMethod: true }
        );

        if (memmove is not null)
        {
            return (delegate*<ref T, ref T, nuint, void>)memmove
                .MakeGenericMethod(typeof(T)).MethodHandle.GetFunctionPointer();
        }

        Debug.Fail($"Using {nameof(MemmoveFallback)} method.");
        return &MemmoveFallback;
    }

    private static void MemmoveFallback(ref T destination, ref T source, nuint elementCount)
    {
        if (elementCount == 0)
        {
            return;
        }

        ReadOnlySpan<T> sourceSpan;
        Span<T> destinationSpan;
        while (elementCount >= int.MaxValue)
        {
            sourceSpan = MemoryMarshal.CreateReadOnlySpan(ref source, int.MaxValue);
            destinationSpan = MemoryMarshal.CreateSpan(ref destination, int.MaxValue);
            sourceSpan.CopyTo(destinationSpan);

            elementCount -= int.MaxValue;
            source = ref Unsafe.Add(ref source, int.MaxValue);
            destination = ref Unsafe.Add(ref destination, int.MaxValue);
        }

        sourceSpan = MemoryMarshal.CreateReadOnlySpan(ref source, (int)elementCount);
        destinationSpan = MemoryMarshal.CreateSpan(ref destination, (int)elementCount);
        sourceSpan.CopyTo(destinationSpan);
    }

    public static void Copy(T[] source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    public static void Copy(T[] source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(T[] source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetArrayDataReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(Span<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(Span<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Copy(ReadOnlySpan{T},Span{T})"/>
    public static void Copy(ReadOnlySpan<T> source, T[] destination)
        => Memmove(ref MemoryMarshal.GetArrayDataReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <summary>
    /// Copies all elements from the source into the destination without checking if enough space is available.
    /// </summary>
    /// <param name="source">The source of the elements.</param>
    /// <param name="destination">The destination of the elements.</param>
    public static void Copy(ReadOnlySpan<T> source, Span<T> destination)
        => Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy(ReadOnlySpan<T> source, ref T destination)
        => Memmove(ref destination, ref MemoryMarshal.GetReference(source), (uint)source.Length);

    public static void Copy(ReadOnlySpan<T> source, T* destination)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref MemoryMarshal.GetReference(source), (uint)source.Length);

    /// <inheritdoc cref="Memmove(ref T,ref T,nuint)"/>
    public static void Memmove(T* destination, T* source, nuint elementCount)
        => Memmove(ref Unsafe.AsRef<T>(destination), ref Unsafe.AsRef<T>(source), elementCount);

    /// <summary>
    /// Copies the given amount of elements from the source into the destination.
    /// </summary>
    /// <param name="destination">The destination of the elements.</param>
    /// <param name="source">The source of the elements.</param>
    /// <param name="elementCount">The amount of elements that will be copied from source to destination.</param>
    public static void Memmove(ref T destination, ref T source, nuint elementCount)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() || Overlap(ref source, ref destination, elementCount))
        {
            s_memmove(ref destination, ref source, elementCount);
            return;
        }

        Memmove(ref Unsafe.As<T, byte>(ref destination), ref Unsafe.As<T, byte>(ref source), elementCount * (uint)sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Overlap(ref T source, ref T destination, nuint elementCount) =>
        (nuint)Unsafe.ByteOffset(ref source, ref destination) < elementCount ||
        (nuint)Unsafe.ByteOffset(ref destination, ref source) < elementCount;

    [MethodImpl(MethodImplOptions.NoInlining)]
    [SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity")]
    private static void Memmove(ref byte destination, ref byte source, nuint byteCount)
    {
        switch (byteCount)
        {
            case 0:
                return;
            case >= AlignmentThreshold:
                AlignmentResult alignmentResult = Align(ref source, ref destination, byteCount);
                if (alignmentResult.ByteCount == 0)
                {
                    return;
                }

                destination = ref alignmentResult.Destination;
                source = ref alignmentResult.Source;
                byteCount = alignmentResult.ByteCount;
                break;
            default:
                goto CopyLessThanAlignmentThreshold;
        }

        bool nothingMoreToCopy = Copy32768ByteBlocks(ref destination, ref source, ref byteCount);
        if (nothingMoreToCopy)
        {
            return;
        }

        if (byteCount >= 16384)
        {
            Copy16384Bytes__NoInline(ref destination, ref source);
            if (byteCount == 16384)
            {
                return;
            }

            destination = Unsafe.Add(ref destination, 16384);
            source = Unsafe.Add(ref source, 16384);
            byteCount -= 16384;
        }

        if (byteCount >= 8192)
        {
            Copy8192Bytes__NoInline(ref destination, ref source);
            if (byteCount == 8192)
            {
                return;
            }

            destination = Unsafe.Add(ref destination, 8192);
            source = Unsafe.Add(ref source, 8192);
            byteCount -= 8192;
        }

        if (byteCount >= 4096)
        {
            Copy4096Bytes__NoInline(ref destination, ref source);
            if (byteCount == 4096)
            {
                return;
            }

            destination = Unsafe.Add(ref destination, 4096);
            source = Unsafe.Add(ref source, 4096);
            byteCount -= 4096;
        }

        if (byteCount >= 2048)
        {
            Copy2048Bytes__NoInline(ref destination, ref source);
            if (byteCount == 2048)
            {
                return;
            }

            destination = Unsafe.Add(ref destination, 2048);
            source = Unsafe.Add(ref source, 2048);
            byteCount -= 2048;
        }

        if (byteCount >= 1024)
        {
            Copy1024Bytes__NoInline(ref destination, ref source);
            if (byteCount == 1024)
            {
                return;
            }

            destination = Unsafe.Add(ref destination, 1024);
            source = Unsafe.Add(ref source, 1024);
            byteCount -= 1024;
        }

        if (byteCount >= 512)
        {
            Copy512Bytes_NoInline(ref destination, ref source);
            if (byteCount == 512)
            {
                return;
            }

            destination = Unsafe.Add(ref destination, 512);
            source = Unsafe.Add(ref source, 512);
            byteCount -= 512;
        }

        if (byteCount >= 256)
        {
            Copy256Bytes__NoInline(ref destination, ref source);
            if (byteCount == 256)
            {
                return;
            }

            destination = Unsafe.Add(ref destination, 256);
            source = Unsafe.Add(ref source, 256);
            byteCount -= 256;
        }

        CopyLessThanAlignmentThreshold:
        switch (byteCount)
        {
            case >= 128:
                Copy128Bytes__NoInline(ref destination, ref source);
                if (byteCount == 128)
                {
                    return;
                }

                byteCount -= 128;
                goto CopyLessThanAlignmentThreshold;
            case >= 64:
            {
                Copy64Bytes(ref source, ref destination);
                byteCount -= 64;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 64 - byteCount;
                destination = Unsafe.Add(ref destination, 64 - remainder);
                source = Unsafe.Add(ref source, 64 - remainder);
                Copy64Bytes(ref source, ref destination);
                return;
            }
            case >= 32:
            {
                Copy32Bytes(ref source, ref destination);
                byteCount -= 32;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 32 - byteCount;
                destination = Unsafe.Add(ref destination, 32 - remainder);
                source = Unsafe.Add(ref source, 32 - remainder);
                Copy32Bytes(ref source, ref destination);
                return;
            }
            case >= 16:
            {
                Copy16Bytes(ref source, ref destination);
                byteCount -= 16;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 16 - byteCount;
                destination = Unsafe.Add(ref destination, 16 - remainder);
                source = Unsafe.Add(ref source, 16 - remainder);
                Copy16Bytes(ref source, ref destination);
                return;
            }
            case >= 8:
            {
                Copy8Bytes(ref source, ref destination);
                byteCount -= 8;
                if (byteCount == 0)
                {
                    return;
                }

                nuint remainder = 8 - byteCount;
                destination = Unsafe.Add(ref destination, 8 - remainder);
                source = Unsafe.Add(ref source, 8 - remainder);
                Copy8Bytes(ref source, ref destination);
                return;
            }
            case 7:
                Copy4Bytes(ref source, ref destination);
                Copy4Bytes(ref Unsafe.Add(ref source, 3), ref Unsafe.Add(ref destination, 3));
                return;
            case 6:
                Copy4Bytes(ref source, ref destination);
                Copy4Bytes(ref Unsafe.Add(ref source, 2), ref Unsafe.Add(ref destination, 2));
                return;
            case 5:
                Copy4Bytes(ref source, ref destination);
                Copy4Bytes(ref Unsafe.Add(ref source, 1), ref Unsafe.Add(ref destination, 1));
                return;
            case 4:
                Copy4Bytes(ref source, ref destination);
                return;
            case 3:
                Copy2Bytes(ref source, ref destination);
                Copy2Bytes(ref Unsafe.Add(ref source, 1), ref Unsafe.Add(ref destination, 1));
                return;
            case 2:
                Copy2Bytes(ref source, ref destination);
                return;
            case 1:
                Copy1Bytes(ref source, ref destination);
                return;
            default:
                ThrowHelper.ThrowUnreachableException();
                return;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Copy32768ByteBlocks(ref byte destination, ref byte source, ref nuint byteCount)
    {
        while (byteCount >= 32768)
        {
            Copy32768Bytes(ref source, ref destination);
            byteCount -= 32768;
            if (byteCount == 0)
            {
                return true;
            }

            destination = Unsafe.Add(ref destination, 32768);
            source = Unsafe.Add(ref source, 32768);
        }

        return false;
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
        Copy4096Bytes(ref Unsafe.Add(ref source, 2048), ref Unsafe.Add(ref destination, 4096));
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

        Unsafe.As<byte, long>(ref destination) = Unsafe.As<byte, long>(ref source);
        Unsafe.As<byte, long>(ref Unsafe.Add(ref destination, sizeof(long))) = Unsafe.As<byte, long>(ref Unsafe.Add(ref source, sizeof(long)));
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
    private static void Copy1Bytes(ref byte source, ref byte destination) => destination = source;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static AlignmentResult Align(ref byte source, ref byte destination, nuint byteCount)
    {
        Debug.Assert(byteCount >= 64);

        ref byte alignedDestination = ref MemoryHelpers.Align(ref destination, 64, AlignmentMethod.Add);
        nuint alignmentDifference = (nuint)Unsafe.AsPointer(ref alignedDestination) - (nuint)Unsafe.AsPointer(ref destination);
        if (alignmentDifference == 0)
        {
            return new(ref source, ref destination, byteCount);
        }

        Copy64Bytes(ref source, ref destination);
        source = Unsafe.Add(ref source, alignmentDifference);
        destination = Unsafe.Add(ref destination, alignmentDifference);
        byteCount -= alignmentDifference;
        return new(
            ref Unsafe.Add(ref source, alignmentDifference),
            ref Unsafe.Add(ref destination, alignmentDifference),
            byteCount - alignmentDifference
        );
    }

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
    private static void Copy512Bytes_NoInline(ref byte destination, ref byte source) => Copy512Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy256Bytes__NoInline(ref byte destination, ref byte source) => Copy256Bytes(ref source, ref destination);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Copy128Bytes__NoInline(ref byte destination, ref byte source) => Copy128Bytes(ref source, ref destination);
}
