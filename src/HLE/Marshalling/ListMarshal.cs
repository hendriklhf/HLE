using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Marshalling;

public static partial class ListMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMemory<T>(List<T> list) => list.Count == 0 ? Memory<T>.Empty : GetArray(list).AsMemory(0, list.Count);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReference<T>(List<T> list)
    {
        T[] array = GetArray(list);
        if (array.Length == 0)
        {
            ThrowListIsEmpty();
        }

        return ref MemoryMarshal.GetArrayDataReference(array);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowListIsEmpty() => throw new InvalidOperationException("The list is empty.");

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetArray<T>(List<T> list) => UnsafeAccessor<T>.GetItems(list);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetArray<T>(List<T> list, T[] array)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(list.Count, array.Length);
        UnsafeAccessor<T>.GetItems(list) = array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCount<T>(List<T> list, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, list.Capacity);
        UnsafeAccessor<T>.GetSize(list) = count;
    }

    [Pure]
    public static List<T> ConstructList<T>(List<T> items) => ConstructList(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static List<T> ConstructList<T>(T[] items) => ConstructList(ref MemoryMarshal.GetArrayDataReference(items), items.Length);

    [Pure]
    public static List<T> ConstructList<T>(Span<T> items) => ConstructList(ref MemoryMarshal.GetReference(items), items.Length);

    [Pure]
    public static List<T> ConstructList<T>(ReadOnlySpan<T> items) => ConstructList(ref MemoryMarshal.GetReference(items), items.Length);

    [Pure]
    public static List<T> ConstructList<T>(ref T items, int length)
    {
        Debug.Assert(length != 0);

        List<T> list = [];
        T[] array = GC.AllocateUninitializedArray<T>(length);
        SpanHelpers.Memmove(ref MemoryMarshal.GetArrayDataReference(array), ref items, (uint)length);
        SetArray(list, array);
        SetCount(list, length);
        return list;
    }
}
