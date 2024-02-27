using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static class SpanExtensions
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArray<T>(this Span<T> span, int start) => span.ToArray(start, span.Length - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArray<T>(this Span<T> span, int start, int length)
        => ToArray(ref MemoryMarshal.GetReference(span), span.Length, start, length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArray<T>(this Span<T> span, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(span.Length);
        return span.ToArray(start, length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArray<T>(this ReadOnlySpan<T> span, int start) => span.ToArray(start, span.Length - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArray<T>(this ReadOnlySpan<T> span, int start, int length)
        => ToArray(ref MemoryMarshal.GetReference(span), span.Length, start, length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArray<T>(this ReadOnlySpan<T> span, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(span.Length);
        return span.ToArray(start, length);
    }

    [Pure]
    private static T[] ToArray<T>(ref T span, int spanLength, int start, int length)
    {
        if (length == 0)
        {
            return [];
        }

        ref T source = ref Slicer<T>.GetStart(ref span, spanLength, start, length);

        T[] result = GC.AllocateUninitializedArray<T>(length);
        ref T destination = ref MemoryMarshal.GetArrayDataReference(result);
        SpanHelpers<T>.Memmove(ref destination, ref source, (uint)length);
        return result;
    }
}
