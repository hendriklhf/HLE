using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

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
    internal static Memory<T> AsMemoryDangerous<T>(this Span<T> span)
    {
        Unsafe.SkipInit(out Memory<T> result);
        byte* spanPointer = (byte*)&span;
        byte* memoryPointer = (byte*)&result;

        // pointers to the three fields Memory<T> consists of
        nuint* memoryReferenceField = (nuint*)memoryPointer;
        int* memoryIndexField = (int*)(memoryPointer + sizeof(nuint));
        int* memoryLengthField = (int*)(memoryPointer + sizeof(nuint) + sizeof(int));

        nuint reference = *(nuint*)spanPointer;
        reference -= (nuint)(sizeof(nuint) << 1);
        *memoryReferenceField = reference;

        *memoryIndexField = 0;

        int length = *(int*)(spanPointer + sizeof(nuint));
        *memoryLengthField = length;

        return result;
    }
}
