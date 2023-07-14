using System;
using HLE.Collections.Concurrent;

namespace HLE.Marshals;

public static class ConcurrentPooledListMarshal<T> where T : IEquatable<T>
{
    public static T[] GetBuffer(ConcurrentPooledList<T> list)
    {
        return PooledListMarshal<T>.GetBuffer(list._list);
    }
}
