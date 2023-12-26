using HLE.Collections.Concurrent;

namespace HLE.Marshalling;

public static class ConcurrentStackMarshal
{
    public static T[] GetBuffer<T>(ConcurrentStack<T> stack) => stack._buffer;
}
