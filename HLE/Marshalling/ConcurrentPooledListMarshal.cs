using HLE.Collections.Concurrent;

namespace HLE.Marshalling;

public static class ConcurrentPooledListMarshal<T>
{
    public static T[] GetBuffer(ConcurrentPooledList<T> list) => list._list._buffer.Array;
}
