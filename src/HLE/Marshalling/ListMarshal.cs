using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static partial class ListMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMemory<T>(List<T> list) => new(GetArray(list), 0, list.Count);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<T> AsReadOnlyMemory<T>(List<T> list) => new(GetArray(list), 0, list.Count);

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

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowListIsEmpty()
            => throw new InvalidOperationException("The list is empty, therefore it is not possible to get a reference to the items.");
    }

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
    public static List<T> ConstructList<T>(T[] items, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, items.Length);

        List<T> list = [];
        SetArray(list, items);
        SetCount(list, count);
        return list;
    }
}
