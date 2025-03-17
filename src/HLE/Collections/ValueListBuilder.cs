using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

[SuppressMessage("ReSharper", "UseCollectionExpression")]
[SuppressMessage("ReSharper", "ConvertToStaticClass", Justification = "collection builders can't be static")]
public sealed class ValueListBuilder
{
    [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed")]
    private ValueListBuilder() => ThrowHelper.ThrowCalledCollectionBuilderConstructor<ValueListBuilder>();

    [Pure]
    public static ValueList<T> Create<T>(IEnumerable<T> items)
    {
        ValueList<T> list = new();
        list.AddRange(items);
        return list;
    }

    [Pure]
    public static ValueList<T> Create<T>(List<T> items) => Create(CollectionsMarshal.AsSpan(items));

    [Pure]
    public static ValueList<T> Create<T>(T[] items) => Create((ReadOnlySpan<T>)items);

    [Pure]
    public static ValueList<T> Create<T>(Span<T> items) => Create((ReadOnlySpan<T>)items);

    [Pure]
    public static ValueList<T> Create<T>(params ReadOnlySpan<T> items)
    {
        ValueList<T> list = new();
        list.AddRange(items);
        return list;
    }

    [Pure]
    public static ValueList<T> Create<T>(T item)
    {
        ValueList<T> list = new(1)
        {
            _buffer = item,
            Count = 1
        };

        return list;
    }

    [Pure]
    public static ValueList<T> Create<T>(T item0, T item1)
    {
        ValueList<T> list = new(2);

        ref T reference = ref list._buffer;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;

        list.Count = 2;
        return list;
    }

    [Pure]
    public static ValueList<T> Create<T>(T item0, T item1, T item2)
    {
        ValueList<T> list = new(3);

        ref T reference = ref list._buffer;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;

        list.Count = 3;
        return list;
    }

    [Pure]
    public static ValueList<T> Create<T>(T item0, T item1, T item2, T item3)
    {
        ValueList<T> list = new(4);

        ref T reference = ref list._buffer;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;

        list.Count = 4;
        return list;
    }

    [Pure]
    public static ValueList<T> Create<T>(T item0, T item1, T item2, T item3, T item4)
    {
        ValueList<T> list = new(5);

        ref T reference = ref list._buffer;
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;
        Unsafe.Add(ref reference, 3) = item3;
        Unsafe.Add(ref reference, 4) = item4;

        list.Count = 5;
        return list;
    }

    [Pure]
    public static ValueList<T> Create<T>(T item0, T item1, T item2, T item3, T item4, T item5)
    {
        ValueList<T> list = new(6);

        ref T reference = ref list._buffer;
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
    public static ValueList<T> Create<T>(T item0, T item1, T item2, T item3, T item4, T item5, T item6)
    {
        ValueList<T> list = new(7);

        ref T reference = ref list._buffer;
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
    public static ValueList<T> Create<T>(T item0, T item1, T item2, T item3, T item4, T item5, T item6, T item7)
    {
        ValueList<T> list = new(8);

        ref T reference = ref list._buffer;
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
