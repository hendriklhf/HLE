using System;
using System.Diagnostics.Contracts;
using HLE.Collections;

namespace HLE.Marshalling;

public static class PooledListMarshal<T>
{
    [Pure]
    public static T[] GetBuffer(PooledList<T> list) => list._buffer.Array;

    public static void SetCount(PooledList<T> list, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        list.Count = count;
    }
}
