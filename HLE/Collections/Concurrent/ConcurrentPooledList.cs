using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace HLE.Collections.Concurrent;

public static class ConcurrentPooledList
{
    [Pure]
    public static ConcurrentPooledList<T> Create<T>(IEnumerable<T> items) where T : IEquatable<T>
    {
        ConcurrentPooledList<T> list = [];
        list.AddRange(items);
        return list;
    }

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(List<T> items) where T : IEquatable<T>
        => new(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(T[] items) where T : IEquatable<T>
        => new(items);

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(Span<T> items) where T : IEquatable<T>
        => new(items);

    [Pure]
    public static ConcurrentPooledList<T> Create<T>(ReadOnlySpan<T> items) where T : IEquatable<T>
        => new(items);
}
