using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public static class AsSpanUnsafeExtensions
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpanUnsafe<T>(this T[] array, int start) => array.AsSpanUnsafe(start, array.Length - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpanUnsafe<T>(this T[] array, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(array.Length);
        return array.AsSpanUnsafe(start, length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpanUnsafe<T>(this T[] array, int start, int length)
    {
        ref T reference = ref MemoryMarshal.GetArrayDataReference(array);
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref reference, start), length);
    }
}
