using HLE.Collections.Concurrent;

namespace HLE.Marshalling;

public static class ConcurrentStackMarshal<T>
{
    public static T[] GetBuffer(ConcurrentStack<T> stack)
    {
        return stack._buffer;
    }
}
