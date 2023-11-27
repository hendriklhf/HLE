using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Collections.Concurrent;

public static class ConcurrentPooledList
{
    [Pure]
    public static ConcurrentPooledList<T> Create<T>(IEnumerable<T> items)
    {
        ConcurrentPooledList<T> list = [];
        list.AddRange(items);
        return list;
    }

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(List<T> items)
        => new(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(T[] items)
        => new(items);

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(Span<T> items)
        => new(items);

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(ReadOnlySpan<T> items)
        => new(items);
}
