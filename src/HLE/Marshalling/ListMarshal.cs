using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.IL;

namespace HLE.Marshalling;

public static class ListMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMemory<T>(List<T> list) => GetArray(list).AsMemory(0, list.Count);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReference<T>(List<T> list)
    {
        if (list.Count == 0)
        {
            ThrowListIsEmpty();
        }

        return ref UnsafeIL.GetArrayReference(GetArray(list));
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowListIsEmpty() => throw new InvalidOperationException("The list is empty.");

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetArray<T>(List<T> list) => ObjectMarshal.GetField<T[]>(list, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetArray<T>(List<T> list, T[] array)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(list.Count, array.Length);
        ObjectMarshal.GetField<T[]>(list, 0) = array;
    }
}
