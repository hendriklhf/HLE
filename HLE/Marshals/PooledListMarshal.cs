using System;
using HLE.Collections;

namespace HLE.Marshals;

public static class PooledListMarshal<T> where T : IEquatable<T>
{
    public static T[] GetBuffer(PooledList<T> list)
    {
        return PooledBufferWriterMarshal<T>.GetBuffer(list._bufferWriter);
    }
}
