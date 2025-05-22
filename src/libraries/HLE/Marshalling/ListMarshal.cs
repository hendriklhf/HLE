using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !NET9_0_OR_GREATER
using System.Diagnostics;
using System.Reflection;
#endif

namespace HLE.Marshalling;

public static partial class ListMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Memory<T> AsMemory<T>(List<T> list) => new(GetArray(list), 0, list.Count);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlyMemory<T> AsReadOnlyMemory<T>(List<T> list) => new(GetArray(list), 0, list.Count);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetReference<T>(List<T> list)
    {
        T[] array = GetArray(list);
        if (array.Length == 0)
        {
            ThrowListIsEmpty();
        }

        return ref MemoryMarshal.GetArrayDataReference(array);

        [DoesNotReturn]
        static void ThrowListIsEmpty()
            => throw new InvalidOperationException("The list is empty, therefore it is not possible to get a reference to the items.");
    }

    [Pure]
#if NET9_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static T[] GetArray<T>(List<T> list)
#if NET9_0_OR_GREATER
        => UnsafeAccessor<T>.GetItems(list);
#else
    {
        FieldInfo? field = typeof(List<T>)
            .GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        ArgumentNullException.ThrowIfNull(field);

        object? value = field.GetValue(list);
        Debug.Assert(value is T[]);
        return Unsafe.As<T[]>(value);
    }
#endif

#if NET9_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void SetArray<T>(List<T> list, T[] array)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(list.Count, array.Length);
#if NET9_0_OR_GREATER
        UnsafeAccessor<T>.GetItems(list) = array;
#else
        FieldInfo? field = typeof(List<T>)
            .GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        ArgumentNullException.ThrowIfNull(field);

        field.SetValue(list, array);
#endif
    }

#if NET9_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void SetCount<T>(List<T> list, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, list.Capacity);
#if NET9_0_OR_GREATER
        UnsafeAccessor<T>.GetSize(list) = count;
#else
        FieldInfo? field = typeof(List<T>)
            .GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance);

        ArgumentNullException.ThrowIfNull(field);

        field.SetValue(list, count);
#endif
    }

    [Pure]
    public static List<T> ConstructList<T>(T[] items, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, items.Length);

        List<T> list = [];
        SetArray(list, items);
        SetCount(list, count);
        return list;
    }
}
