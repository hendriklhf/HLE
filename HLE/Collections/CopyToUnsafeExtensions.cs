using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public static unsafe class CopyToUnsafeExtensions
{
    /// <inheritdoc cref="CopyToUnsafe{T}(ReadOnlySpan{T},Span{T})"/>
    public static void CopyToUnsafe<T>(this T[] source, Span<T> destination) => CopyToUnsafe((ReadOnlySpan<T>)source, destination);

    /// <inheritdoc cref="CopyToUnsafe{T}(ReadOnlySpan{T},Span{T})"/>
    public static void CopyToUnsafe<T>(this Span<T> source, Span<T> destination) => CopyToUnsafe((ReadOnlySpan<T>)source, destination);

    /// <summary>
    /// Copies the <paramref name="source"/> to the <paramref name="destination"/> without checking if enough space is available, so the caller has to verify that it is safe.
    /// </summary>
    /// <param name="source">The items that will be copied.</param>
    /// <param name="destination">The destination where the items will be copied to.</param>
    /// <typeparam name="T">The type of the items that will be copied.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToUnsafe<T>(this ReadOnlySpan<T> source, Span<T> destination)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            source.CopyTo(MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(destination), source.Length));
            return;
        }

        ref T sourceReference = ref MemoryMarshal.GetReference(source);
        ref byte sourceReferenceAsByte = ref Unsafe.As<T, byte>(ref sourceReference);

        ref T destinationReference = ref MemoryMarshal.GetReference(destination);
        ref byte destinationReferenceAsByte = ref Unsafe.As<T, byte>(ref destinationReference);

        Unsafe.CopyBlock(ref destinationReferenceAsByte, ref sourceReferenceAsByte, (uint)(sizeof(T) * source.Length));
    }
}
