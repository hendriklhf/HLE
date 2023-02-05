using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE;

public static unsafe class Utils
{
    public static int MaxStackAllocSize
    {
        get => _maxStackAllocSize;
        set => _maxStackAllocSize = value;
    }

    private static int _maxStackAllocSize = sizeof(nuint) >= sizeof(ulong) ? 1_000_000 : 250_000;

    /// <summary>
    /// Determines whether to use a stackalloc or an array by passing a generic type and the element count.
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
}
