using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static unsafe class MemoryHelper
{
    public static int MaxStackAllocSize { get; set; } = sizeof(nuint) >= sizeof(ulong) ? 10_000 : 2500;

    /// <summary>
    /// Determines whether to use a stack or a heap allocation by passing a generic type and the element count.
    /// The default maximum stack allocation size is set to 10.000 bytes for 64-bit processes and to 2500 bytes for 32-bit processes.
    /// The default maximum can also be changed with the <see cref="MaxStackAllocSize"/> property.
    /// </summary>
    /// <param name="elementCount">The amount of elements that will be multiplied by the type's size.</param>
    /// <typeparam name="T">The type of the <see langword="stackalloc">.</typeparam>
    /// <returns>True, if a stackalloc can be used, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UseStackAlloc<T>(int elementCount)
    {
        int totalByteSize = sizeof(T) * elementCount;
        return totalByteSize <= MaxStackAllocSize;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsMutableSpan<T>(this ReadOnlySpan<T> span)
    {
        return *(Span<T>*)&span;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMutableMemory<T>(this ReadOnlyMemory<T> memory)
    {
        return *(Memory<T>*)&memory;
    }

    /// <inheritdoc cref="AsMemoryDangerous{T}(Span{T})"/>
    public static ReadOnlyMemory<T> AsMemoryDangerous<T>(this ReadOnlySpan<T> span)
    {
        return AsMemoryDangerous(span.AsMutableSpan());
    }

    /// <summary>
    /// Converts a <see cref="Span{T}"/> to a <see cref="Memory{T}"/>. Does not allocate any memory. <br/>
    /// ⚠️ Only works if the span's reference points to the first element of an <see cref="Array"/>. Otherwise this method is potentially dangerous. ⚠️
    /// </summary>
    /// <param name="span">The span that will be converted.</param>
    /// <returns>A memory view over the span.</returns>
    [Pure]
    public static Memory<T> AsMemoryDangerous<T>(this Span<T> span)
    {
        Unsafe.SkipInit(out Memory<T> result);
        byte* spanPointerAsBytePointer = (byte*)&span;
        byte* memoryPointerAsBytePointer = (byte*)&result;

        // pointers to the three fields Memory<T> consists off
        nuint* memoryReferenceField = (nuint*)memoryPointerAsBytePointer;
        int* memoryIndexField = (int*)(memoryPointerAsBytePointer + sizeof(nuint));
        int* memoryLengthField = (int*)(memoryPointerAsBytePointer + sizeof(nuint) + sizeof(int));

        nuint spanReferenceFieldValue = *(nuint*)spanPointerAsBytePointer - (nuint)(sizeof(nuint) << 1);
        *memoryReferenceField = spanReferenceFieldValue;

        *memoryIndexField = 0;

        int memoryLength = *(int*)(spanPointerAsBytePointer + sizeof(nuint));
        *memoryLengthField = memoryLength;

        return result;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawDataPointer<T>(T reference) where T : class?
    {
        return *(nuint*)&reference;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetReferenceFromRawDataPointer<T>(nuint pointer) where T : class?
    {
        return *(T*)&pointer;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetStructBytes<T>(ref T item) where T : struct
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref item), sizeof(T));
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBytes<TLeft, TRight>(TLeft left, TRight right) where TLeft : struct where TRight : struct
    {
        if (sizeof(TLeft) != sizeof(TRight))
        {
            return false;
        }

        ReadOnlySpan<byte> leftBytes = GetStructBytes(ref left);
        ReadOnlySpan<byte> rightBytes = GetStructBytes(ref right);
        return leftBytes.SequenceEqual(rightBytes);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBytes<TLeft, TRight>(ref TLeft left, ref TRight right) where TLeft : struct where TRight : struct
    {
        if (sizeof(TLeft) != sizeof(TRight))
        {
            return false;
        }

        if (Unsafe.AreSame(ref Unsafe.As<TLeft, byte>(ref left), ref Unsafe.As<TRight, byte>(ref right)))
        {
            return true;
        }

        ReadOnlySpan<byte> leftBytes = GetStructBytes(ref left);
        ReadOnlySpan<byte> rightBytes = GetStructBytes(ref right);
        return leftBytes.SequenceEqual(rightBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> ReturnStackAlloced<T>(scoped Span<T> span)
    {
#pragma warning disable CS9080
        return span;
#pragma warning restore CS9080
    }
}
