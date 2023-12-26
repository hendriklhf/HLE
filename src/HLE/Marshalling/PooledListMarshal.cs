using System;
using System.Diagnostics.Contracts;
using HLE.Collections;

namespace HLE.Marshalling;

public static class PooledListMarshal
{
    [Pure]
    public static T[] GetBuffer<T>(PooledList<T> list) => list._buffer.Array;

    public static void SetCount<T>(PooledList<T> list, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, Array.MaxLength);
        list.Count = count;
    }
}
