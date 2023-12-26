using System;
using HLE.Collections;

namespace HLE.Marshalling;

public static class ValueQueueMarshal
{
    public static Span<T> GetBuffer<T>(ValueQueue<T> queue) => queue._queue;
}
