using System;
using HLE.Collections.Concurrent;

namespace HLE.Marshalling;

public static class ConcurrentPooledListMarshal<T> where T : IEquatable<T>
{
    public static T[] GetBuffer(ConcurrentPooledList<T> list)
    {
        return PooledListMarshal<T>.GetBuffer(list._list);
    }

    public static void SetCount(ConcurrentPooledList<T> list, int count)
    {
        PooledBufferWriterMarshal<T>.SetCount(list._list._bufferWriter, count);
    }

    public static Memory<T> AsMemory(ConcurrentPooledList<T> list)
    {
        return PooledListMarshal<T>.AsMemory(list._list);
    }
}
