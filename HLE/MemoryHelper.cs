using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE;

public static unsafe class MemoryHelper
{
    public static int MaxStackAllocSize
    {
        get => _maxStackAllocSize;
        set => _maxStackAllocSize = value;
    }

    private static int _maxStackAllocSize = sizeof(nuint) >= sizeof(ulong) ? 1_000_000 : 250_000;

    /// <summary>
    /// Determines whether to use a stack or a heap allocation by passing a generic type and the element count.
    /// The default maximum stack allocation size is set to 1.000.000 bytes for 64-bit processes and to 250.000 bytes for 32-bit processes.
    /// The default maximum can also be changed with the <see cref="MaxStackAllocSize"/> property.
    /// </summary>
    /// <param name="elementCount">The amount of elements that will be multiplied by the type's size.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>True, if a stackalloc can be used, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UseStackAlloc<T>(int elementCount) where T : struct
    {
        int totalByteSize = sizeof(T) * elementCount;
        return totalByteSize <= _maxStackAllocSize;
    }

    /// <summary>
    /// Vectorized memory copy. Will be faster than other memory copy methods for data larger than around 10.000 bytes.
    /// </summary>
    /// <param name="source">The source of the copied data.</param>
    /// <param name="destination">The destination of the copied data.</param>
    /// <param name="elementCount">The amount of elements that will be copied.</param>
    /// <typeparam name="T">The type of elements that will be copied.</typeparam>
    public static void VectorizedCopy<T>(T[] source, T[] destination, int elementCount) where T : struct
    {
        ref T sourceRef = ref MemoryMarshal.GetArrayDataReference(source);
        ref T destinationRef = ref MemoryMarshal.GetArrayDataReference(destination);
        VectorizedCopy(ref sourceRef, ref destinationRef, elementCount);
    }

    /// <summary>
    /// Vectorized memory copy. Will be faster than other memory copy methods for data larger than around 10.000 bytes.
    /// </summary>
    /// <param name="source">The source of the copied data.</param>
    /// <param name="destination">The destination of the copied data.</param>
    /// <param name="elementCount">The amount of elements that will be copied.</param>
    /// <typeparam name="T">The type of elements that will be copied.</typeparam>
    public static void VectorizedCopy<T>(ReadOnlyMemory<T> source, Memory<T> destination, int elementCount) where T : struct
    {
        ref T sourceRef = ref MemoryMarshal.GetReference(source.Span);
        ref T destinationRef = ref MemoryMarshal.GetReference(destination.Span);
        VectorizedCopy(ref sourceRef, ref destinationRef, elementCount);
    }

    /// <summary>
    /// Vectorized memory copy. Will be faster than other memory copy methods for data larger than around 10.000 bytes.
    /// </summary>
    /// <param name="source">The source of the copied data.</param>
    /// <param name="destination">The destination of the copied data.</param>
    /// <param name="elementCount">The amount of elements that will be copied.</param>
    /// <typeparam name="T">The type of elements that will be copied.</typeparam>
    public static void VectorizedCopy<T>(ReadOnlySpan<T> source, Span<T> destination, int elementCount) where T : struct
    {
        ref T sourceRef = ref MemoryMarshal.GetReference(source);
        ref T destinationRef = ref MemoryMarshal.GetReference(destination);
        VectorizedCopy(ref sourceRef, ref destinationRef, elementCount);
    }

    /// <inheritdoc cref="VectorizedCopy{T}(ref T,ref T,int)"/>
    public static void VectorizedCopy<T>(T* source, T* destination, int elementCount) where T : struct
    {
        ref T sourcePtr = ref Unsafe.AsRef<T>(source);
        ref T destinationPtr = ref Unsafe.AsRef<T>(destination);
        VectorizedCopy(ref sourcePtr, ref destinationPtr, elementCount);
    }

    /// <summary>
    /// Vectorized memory copy. Will be faster than other memory copy methods for data larger than around 10.000 bytes.
    /// </summary>
    /// <param name="source">A pointer to the source of the copied data.</param>
    /// <param name="destination">A pointer to the destination of the copied data.</param>
    /// <param name="elementCount">The amount of elements that will be copied.</param>
    /// <typeparam name="T">The type of elements that will be copied.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void VectorizedCopy<T>(ref T source, ref T destination, int elementCount) where T : struct
    {
        ref byte sourceByte = ref Unsafe.As<T, byte>(ref source);
        ref byte destinationByte = ref Unsafe.As<T, byte>(ref destination);
        int byteCount = elementCount * sizeof(T);

        int vectorCount = Vector256<byte>.Count;
        if (Vector256.IsHardwareAccelerated && byteCount > vectorCount)
        {
            while (byteCount >= vectorCount)
            {
                Vector256.LoadUnsafe(ref sourceByte).StoreUnsafe(ref destinationByte);
                sourceByte = ref Unsafe.Add(ref sourceByte, vectorCount);
                destinationByte = ref Unsafe.Add(ref destinationByte, vectorCount);
                byteCount -= vectorCount;
            }

            for (int i = 0; i < byteCount; i++)
            {
                Unsafe.Add(ref destinationByte, i) = Unsafe.Add(ref sourceByte, i);
            }

            return;
        }

        vectorCount = Vector128<byte>.Count;
        if (Vector128.IsHardwareAccelerated && elementCount > vectorCount)
        {
            while (byteCount >= vectorCount)
            {
                Vector128.LoadUnsafe(ref sourceByte).StoreUnsafe(ref destinationByte);
                sourceByte = ref Unsafe.Add(ref sourceByte, vectorCount);
                destinationByte = ref Unsafe.Add(ref destinationByte, vectorCount);
                byteCount -= vectorCount;
            }

            for (int i = 0; i < byteCount; i++)
            {
                Unsafe.Add(ref destinationByte, i) = Unsafe.Add(ref sourceByte, i);
            }

            return;
        }

        vectorCount = Vector64<byte>.Count;
        if (Vector64.IsHardwareAccelerated && elementCount > vectorCount)
        {
            while (byteCount >= vectorCount)
            {
                Vector64.LoadUnsafe(ref sourceByte).StoreUnsafe(ref destinationByte);
                sourceByte = ref Unsafe.Add(ref sourceByte, vectorCount);
                destinationByte = ref Unsafe.Add(ref destinationByte, vectorCount);
                byteCount -= vectorCount;
            }

            for (int i = 0; i < byteCount; i++)
            {
                Unsafe.Add(ref destinationByte, i) = Unsafe.Add(ref sourceByte, i);
            }

            return;
        }

        for (int i = 0; i < byteCount; i++)
        {
            Unsafe.Add(ref destinationByte, i) = Unsafe.Add(ref destinationByte, i);
        }
    }

    [Pure]
    public static Span<T> AsMutableSpan<T>(this ReadOnlySpan<T> span)
    {
        return *(Span<T>*)&span;
    }

    [Pure]
    public static Memory<T> AsMutableMemory<T>(this ReadOnlyMemory<T> memory)
    {
        return *(Memory<T>*)&memory;
    }

    /// <summary>
    /// Converts a <see cref="Span{T}"/> to a <see cref="Memory{T}"/>. ⚠️ Only works if the span's reference is the first element of an <see cref="Array"/>. Otherwise this method is potentially dangerous. ⚠️
    /// </summary>
    /// <param name="span">The span that will be converted.</param>
    /// <returns>A memory view over the span.</returns>
    [Pure]
    internal static Memory<T> AsMemory<T>(this Span<T> span)
    {
        Memory<T> result = default;
        byte* spanPtr = (byte*)&span;
        byte* memoryPtr = (byte*)&result;
        nuint* memoryReference = (nuint*)memoryPtr;
        int* memoryIndex = (int*)(memoryPtr + sizeof(nuint));
        int* memoryLength = (int*)(memoryPtr + sizeof(nuint) + sizeof(int));

        nuint reference = *(nuint*)spanPtr;
        reference -= (nuint)(sizeof(nuint) << 1);
        *memoryReference = reference;

        *memoryIndex = 0;

        int length = *(int*)(spanPtr + sizeof(nuint));
        *memoryLength = length;

        return result;
    }
}
