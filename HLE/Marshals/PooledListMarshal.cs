using System;
using HLE.Collections;

namespace HLE.Marshals;

public static class PooledListMarshal<T> where T : IEquatable<T>
{
    public static T[] GetBuffer(PooledList<T> list)
    {
        return PooledBufferWriterMarshal<T>.GetBuffer(list._bufferWriter);
    }

    public static void SetCount(PooledList<T> list, int count)
    {
        PooledBufferWriterMarshal<T>.SetCount(list._bufferWriter, count);
    }

    public static Memory<T> AsMemory(PooledList<T> list)
    {
        return list._bufferWriter.WrittenMemory;
    }
}
