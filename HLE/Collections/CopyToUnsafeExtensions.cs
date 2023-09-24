using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Collections;

public static class CopyToUnsafeExtensions
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
        ref T sourceReference = ref MemoryMarshal.GetReference(source);
        ref T destinationReference = ref MemoryMarshal.GetReference(destination);
        CopyWorker<T>.Memmove(ref destinationReference, ref sourceReference, (nuint)source.Length);
    }
}
