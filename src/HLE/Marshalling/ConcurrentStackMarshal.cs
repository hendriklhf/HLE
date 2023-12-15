using HLE.Collections.Concurrent;

namespace HLE.Marshalling;

public static class ConcurrentStackMarshal<T>
{
    public static T[] GetBuffer(ConcurrentStack<T> stack) => stack._buffer;
}
