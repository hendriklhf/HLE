using System;
using HLE.Collections;

namespace HLE.Marshalling;

public static class PooledListMarshal<T> where T : IEquatable<T>
{
    public static T[] GetBuffer(PooledList<T> list) => list._buffer.Array;
}
