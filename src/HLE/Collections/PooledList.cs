using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

public sealed class PooledList
{
    public PooledList() => ThrowHelper.ThrowCalledCollectionBuilderConstructor();

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(IEnumerable<T> items)
    {
        PooledList<T> list = [];
        list.AddRange(items);
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(List<T> items) => new(CollectionsMarshal.AsSpan(items));

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(params T[] items) => new((ReadOnlySpan<T>)items);

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(Span<T> items) => new(items);

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(ReadOnlySpan<T> items) => new(items);

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item)
    {
        PooledList<T> list = new(1);
        MemoryMarshal.GetArrayDataReference(list._buffer!) = item;
        list.Count = 1;
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item0, T item1)
    {
        PooledList<T> list = new(2);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(list._buffer!);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;

        list.Count = 2;
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item0, T item1, T item2)
    {
        PooledList<T> list = new(3);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(list._buffer!);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;

        list.Count = 3;
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3)
    {
        PooledList<T> list = new(4);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(list._buffer!);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;

        list.Count = 4;
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3, T item4)
    {
        PooledList<T> list = new(5);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(list._buffer!);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;
        Unsafe.Add(ref reference, 4) = item4;

        list.Count = 5;
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3, T item4, T item5)
    {
        PooledList<T> list = new(6);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(list._buffer!);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;
        Unsafe.Add(ref reference, 4) = item4;
        Unsafe.Add(ref reference, 5) = item5;

        list.Count = 6;
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3, T item4, T item5, T item6)
    {
        PooledList<T> list = new(7);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(list._buffer!);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;
        Unsafe.Add(ref reference, 4) = item4;
        Unsafe.Add(ref reference, 5) = item5;
        Unsafe.Add(ref reference, 6) = item6;

        list.Count = 7;
        return list;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> Create<T>(T item0, T item1, T item2, T item3, T item4, T item5, T item6, T item7)
    {
        PooledList<T> list = new(8);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(list._buffer!);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;
        Unsafe.Add(ref reference, 4) = item4;
        Unsafe.Add(ref reference, 5) = item5;
        Unsafe.Add(ref reference, 6) = item6;
        Unsafe.Add(ref reference, 7) = item7;

        list.Count = 8;
        return list;
    }
}
