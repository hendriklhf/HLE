using System;
using HLE.Collections;

namespace HLE.Marshals;

public static class ValueQueueMarshal<T>
{
    public static Span<T> GetBuffer(ValueQueue<T> queue)
    {
        return queue._queue;
    }
}
