using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    public static void CopyChecked<T>(T[] source, List<T> destination, int offset = 0)
        => CopyChecked((ReadOnlySpan<T>)source, destination, offset);

    public static void CopyChecked<T>(T[] source, T[] destination)
        => CopyChecked((ReadOnlySpan<T>)source, destination);

    public static void CopyChecked<T>(T[] source, Span<T> destination)
        => CopyChecked((ReadOnlySpan<T>)source, destination);

    public static void CopyChecked<T>(Span<T> source, List<T> destination, int offset = 0)
        => CopyChecked((ReadOnlySpan<T>)source, destination, offset);

    public static void CopyChecked<T>(Span<T> source, T[] destination)
        => CopyChecked((ReadOnlySpan<T>)source, destination);

    public static void CopyChecked<T>(Span<T> source, Span<T> destination)
        => CopyChecked((ReadOnlySpan<T>)source, destination);

    public static void CopyChecked<T>(ReadOnlySpan<T> source, List<T> destination, int offset = 0)
    {
        if (source.Length == 0)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        int newListCount = checked(source.Length + offset);
        if (newListCount > Array.MaxLength)
        {
            ThrowCopiedItemsWouldExceedMaxArrayLength();
        }

        if (newListCount > destination.Count)
        {
            CollectionsMarshal.SetCount(destination, newListCount);
        }

        ref T dest = ref Unsafe.Add(ref ListMarshal.GetReference(destination), offset);
        Copy(source, ref dest);

        return;

        [DoesNotReturn]
        static void ThrowCopiedItemsWouldExceedMaxArrayLength()
            => throw new InvalidOperationException(
                $"The amount of items to be copied into the {typeof(List<T>)} would exceed " +
                "the maximum array length, thus can't be copied to the destination."
            );
    }

    public static void CopyChecked<T>(ReadOnlySpan<T> source, T[] destination)
        => CopyChecked(source, destination.AsSpan());

    public static void CopyChecked<T>(ReadOnlySpan<T> source, Span<T> destination)
    {
        if (source.Length == 0)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(source.Length, destination.Length);

        Memmove(ref MemoryMarshal.GetReference(destination), ref MemoryMarshal.GetReference(source), source.Length);
    }
}
