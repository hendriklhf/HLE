using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static unsafe class MemoryHelper
{
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
    public static bool UseStackAlloc<T>(int elementCount)
    {
        int totalByteSize = sizeof(T) * elementCount;
        return totalByteSize <= s_maximumStackallocSize;
    }
}
