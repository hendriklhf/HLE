using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using HLE.Numerics;

namespace HLE.Memory;

public static unsafe class MemoryHelpers
{
    private static readonly int s_maximumStackallocSize = Environment.Is64BitProcess ? MaximumStackallocSize64 : MaximumStackallocSize32;

    private const int MaximumStackallocSize32 = 2048;
    private const int MaximumStackallocSize64 = 8192;

    /// <summary>
    /// Determines whether to use a stack or a heap allocation by passing a generic type and the element count.
    /// The maximum stack allocation size is set to 8192 bytes for 64-bit processes and to 2048 bytes for 32-bit processes.
    /// </summary>
    /// <param name="elementCount">The number of elements wanted to be stack allocated.</param>
    /// <typeparam name="T">The type of the <see langword="stackalloc"/>.</typeparam>
    /// <returns>True, if a <see langword="stackalloc"/> can be used, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool UseStackalloc<T>(int elementCount)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        int totalByteSize = sizeof(T) * elementCount;
        bool result = totalByteSize <= s_maximumStackallocSize;
        Debug.WriteLine($"{typeof(MemoryHelpers)}.{nameof(UseStackalloc)}<{typeof(T)}>({elementCount}) -> {result}");
        return result;
    }

    [Pure]
    public static bool IsAligned(void* pointer, nuint alignment)
        => IsAligned(ref Unsafe.AsRef<byte>(pointer), alignment);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAligned<T>(ref T reference, nuint alignment)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        if (BitOperations.PopCount(alignment) != 1)
        {
            ThrowAlignmentNeedsToBeNonZeroPowerOfTwo();
        }

        nuint value = (nuint)Unsafe.AsPointer(ref reference);
        return (value & (alignment - 1)) == 0;
    }

    [Pure]
    public static T* Align<T>(T* pointer, nuint alignment, AlignmentMethod method)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        => (T*)Unsafe.AsPointer(ref Align(ref Unsafe.AsRef<T>(pointer), alignment, method));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Align<T>(ref T reference, nuint alignment, AlignmentMethod method)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        if (BitOperations.PopCount(alignment) != 1)
        {
            ThrowAlignmentNeedsToBeNonZeroPowerOfTwo();
        }

        nuint address = (nuint)Unsafe.AsPointer(ref reference);
        switch (method)
        {
            case AlignmentMethod.Add:
                return ref Unsafe.AsRef<T>((void*)(address + alignment - (address % alignment)));
            case AlignmentMethod.Subtract:
                return ref Unsafe.AsRef<T>((void*)(address & ~(alignment - 1)));
            default:
                ThrowHelper.ThrowInvalidEnumValue(method);
                return ref Unsafe.NullRef<T>();
        }
    }

    [DoesNotReturn]
    private static void ThrowAlignmentNeedsToBeNonZeroPowerOfTwo()
        => throw new ArgumentException("The alignment needs to be a non-zero power of 2.");
}
