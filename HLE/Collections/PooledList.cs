using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public sealed class PooledList
{
    [Pure]
    public static PooledList<T> Create<T>(IEnumerable<T> items) where T : IEquatable<T>
    {
        PooledList<T> list = [];
        list.AddRange(items);
        return list;
    }

    [Pure]
    public static PooledList<T> Create<T>(List<T> items) where T : IEquatable<T>
        => new(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static PooledList<T> Create<T>(params T[] items) where T : IEquatable<T>
        => new((ReadOnlySpan<T>)items);

    [Pure]
    public static PooledList<T> Create<T>(Span<T> items) where T : IEquatable<T>
        => new(items);

    [Pure]
    public static PooledList<T> Create<T>(ReadOnlySpan<T> items) where T : IEquatable<T>
        => new(items);

    [Pure]
    public static PooledList<T> Create<T>(T item) where T : IEquatable<T>
    {
        PooledList<T> list = new(1);
        list._buffer.Reference = item;
        list.Count = 1;
        return list;
    }

    [Pure]
    public static PooledList<T> Create<T>(T item0, T item1) where T : IEquatable<T>
    {
        PooledList<T> list = new(2);

        ref T reference = ref list._buffer.Reference;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;

        list.Count = 2;
        return list;
    }

    [Pure]
    public static PooledList<T> Create<T>(T item0, T item1, T item2) where T : IEquatable<T>
    {
        PooledList<T> list = new(3);

        ref T reference = ref list._buffer.Reference;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;

        list.Count = 3;
        return list;
    }

    [Pure]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3) where T : IEquatable<T>
    {
        PooledList<T> list = new(4);

        ref T reference = ref list._buffer.Reference;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;

        list.Count = 4;
        return list;
    }

    [Pure]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3, T item4) where T : IEquatable<T>
    {
        PooledList<T> list = new(5);

        ref T reference = ref list._buffer.Reference;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;
        Unsafe.Add(ref reference, 4) = item4;

        list.Count = 5;
        return list;
    }

    [Pure]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3, T item4, T item5) where T : IEquatable<T>
    {
        PooledList<T> list = new(6);

        ref T reference = ref list._buffer.Reference;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;
        Unsafe.Add(ref reference, 4) = item4;
        Unsafe.Add(ref reference, 5) = item5;

        list.Count = 6;
        return list;
    }
}
