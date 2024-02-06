using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static class ListMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMemory<T>(List<T> list) => ObjectMarshal.ReadField<T[]>(list, 0).AsMemory(0, list.Count);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] AsArray<T>(List<T> list) => ObjectMarshal.ReadField<T[]>(list, 0);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReference<T>(List<T> list)
    {
        T[] array = AsArray(list);
        return ref MemoryMarshal.GetArrayDataReference(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetArray<T>(List<T> list, T[] array)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(list.Count, array.Length);
        ObjectMarshal.ReadField<T[]>(list, 0) = array;
    }
}
