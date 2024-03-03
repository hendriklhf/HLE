using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using HLE.Numerics;

namespace HLE.Memory;

public static unsafe class MemoryHelpers
{
    /// <summary>
    /// The threshold for which allocating native non-zeroed memory is faster than cleared managed memory.
    /// </summary>
    public const int NonZeroedNativeMemoryThreshold = 512;

    private static readonly int s_maximumStackallocSize = Environment.Is64BitProcess ? 8192 : 2048;

    /// <summary>
    /// Determines whether to use a stack or a heap allocation by passing a generic type and the element count.
    /// The maximum stack allocation size is set to 8192 bytes for 64-bit processes and to 2048 bytes for 32-bit processes.
    /// </summary>
    /// <param name="elementCount">The amount of elements wanted to be stack allocated.</param>
    /// <typeparam name="T">The type of the <see langword="stackalloc"/>.</typeparam>
    /// <returns>True, if a stackalloc can be used, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UseStackalloc<T>(int elementCount)
    {
        int totalByteSize = sizeof(T) * elementCount;
        return totalByteSize <= s_maximumStackallocSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAligned<T>(ref T reference, nuint alignment)
    {
        if (BitOperations.PopCount(alignment) != 1)
        {
            ThrowAlignmentNeedsToBePowerOfTwo();
        }

        nuint value = (nuint)Unsafe.AsPointer(ref reference);
        return (value & (alignment - 1)) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Align<T>(ref T reference, nuint alignment, AlignmentMethod method)
    {
        if (BitOperations.PopCount(alignment) != 1)
        {
            ThrowAlignmentNeedsToBePowerOfTwo();
        }

        nuint value = (nuint)Unsafe.AsPointer(ref reference);
        switch (method)
        {
            case AlignmentMethod.Add:
                return ref Unsafe.AsRef<T>((void*)(value + alignment - (value % alignment)));
            case AlignmentMethod.Subtract:
                return ref Unsafe.AsRef<T>((void*)(value & ~(alignment - 1)));
            default:
                ThrowHelper.ThrowInvalidEnumValue(method);
                return ref Unsafe.NullRef<T>();
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowAlignmentNeedsToBePowerOfTwo() => throw new InvalidOperationException("The alignment needs to be a power of 2.");
}
