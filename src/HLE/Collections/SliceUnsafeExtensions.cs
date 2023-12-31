using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public static class SliceUnsafeExtensions
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> SliceUnsafe<T>(this Span<T> span, int start, int length)
    {
        ref T reference = ref MemoryMarshal.GetReference(span);
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref reference, start), length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> SliceUnsafe<T>(this Span<T> span, int start) => span.SliceUnsafe(start, span.Length - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> SliceUnsafe<T>(this Span<T> span, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(span.Length);
        return span.SliceUnsafe(start, length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceUnsafe<T>(this ReadOnlySpan<T> span, int start, int length)
    {
        ref T reference = ref MemoryMarshal.GetReference(span);
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref reference, start), length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceUnsafe<T>(this ReadOnlySpan<T> span, int start)
        => span.SliceUnsafe(start, span.Length - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceUnsafe<T>(this ReadOnlySpan<T> span, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(span.Length);
        return span.SliceUnsafe(start, length);
    }
}
