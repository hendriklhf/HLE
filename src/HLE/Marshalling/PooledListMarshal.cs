using System;
using System.Diagnostics.Contracts;
using HLE.Collections;

namespace HLE.Marshalling;

public static class PooledListMarshal
{
    [Pure]
    public static T[] GetBuffer<T>(PooledList<T> list) => list.GetBuffer();

    public static void SetCount<T>(PooledList<T> list, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)count, (uint)Array.MaxLength);
        list.Count = count;
    }
}
