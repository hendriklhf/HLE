using HLE.Collections.Concurrent;

namespace HLE.Marshalling;

public static class ConcurrentPooledListMarshal
{
    public static T[] GetBuffer<T>(ConcurrentPooledList<T> list) => list._list._buffer.Array;
}
