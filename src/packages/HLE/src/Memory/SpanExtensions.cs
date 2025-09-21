using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

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

        ReadOnlySpan<T> source = Slicer.SliceReadOnly(ref span, spanLength, start, length);
        T[] result = GC.AllocateUninitializedArray<T>(length);
        SpanHelpers.Copy(source, result);
        return result;
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this Span<T> span)
    {
        if (span.Length == 0)
        {
            return [];
        }

        T[] items = GC.AllocateUninitializedArray<T>(span.Length);
        SpanHelpers.Copy(span, items);
        return ListMarshal.ConstructList(items, span.Length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this ReadOnlySpan<T> span)
    {
        if (span.Length == 0)
        {
            return [];
        }

        T[] items = GC.AllocateUninitializedArray<T>(span.Length);
        SpanHelpers.Copy(span, items);
        return ListMarshal.ConstructList(items, span.Length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this Span<T> span, int start) => span.ToList(start, span.Length - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this Span<T> span, int start, int length)
        => ToList(ref MemoryMarshal.GetReference(span), span.Length, start, length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this Span<T> span, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(span.Length);
        return span.ToList(start, length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this ReadOnlySpan<T> span, int start) => span.ToList(start, span.Length - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this ReadOnlySpan<T> span, int start, int length)
        => ToList(ref MemoryMarshal.GetReference(span), span.Length, start, length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> ToList<T>(this ReadOnlySpan<T> span, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(span.Length);
        return span.ToList(start, length);
    }

    [Pure]
    private static List<T> ToList<T>(ref T span, int spanLength, int start, int length)
    {
        if (length == 0)
        {
            return [];
        }

        ReadOnlySpan<T> source = Slicer.SliceReadOnly(ref span, spanLength, start, length);
        T[] buffer = GC.AllocateUninitializedArray<T>(length);
        SpanHelpers.Copy(source, buffer);
        return ListMarshal.ConstructList(buffer, length);
    }
}
