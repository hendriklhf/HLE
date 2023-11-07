using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Collections;

public static partial class CollectionHelpers
{
    [Pure]
    public static int[] IndicesOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        if (TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span))
        {
            return IndicesOf(span, predicate);
        }

        using PooledList<int> indices = collection.TryGetNonEnumeratedCount(out int elementCount) ? new(elementCount) : new();
        switch (collection)
        {
            case IList<T> iList:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (predicate(iList[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            case IReadOnlyList<T> iReadOnlyList:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (predicate(iReadOnlyList[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            case IIndexAccessible<T> indexAccessible:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (predicate(indexAccessible[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            default:
            {
                int currentIndex = 0;
                foreach (T item in collection)
                {
                    if (predicate(item))
                    {
                        indices.Add(currentIndex);
                    }

                    currentIndex++;
                }

                break;
            }
        }

        return indices.ToArray();
    }

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, Func<T, bool> predicate)
        => IndicesOf(CollectionsMarshal.AsSpan(list), predicate);

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, Func<T, bool> predicate)
        => IndicesOf(array.AsSpan(), predicate);

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> span, Func<T, bool> predicate)
        => IndicesOf((ReadOnlySpan<T>)span, predicate);

    public static int IndicesOf<T>(this Span<T> span, Func<T, bool> predicate, Span<int> destination)
        => IndicesOf((ReadOnlySpan<T>)span, predicate, destination);

    [Pure]
    [SkipLocalsInit]
    public static int[] IndicesOf<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        int length;
        if (!MemoryHelper.UseStackAlloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(span.Length);
            length = IndicesOf(span, predicate, indicesBuffer.AsSpan());
            return indicesBuffer[..length].ToArray();
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, predicate, indices);
        return indices[..length].ToArray();
    }

    public static int IndicesOf<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate, Span<int> destination)
    {
        int length = 0;
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return 0;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            bool equals = predicate(Unsafe.Add(ref firstItem, i));
            int equalsAsByte = Unsafe.As<bool, byte>(ref equals);
            destination[length] = i;
            length += equalsAsByte;
        }

        return length;
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this IEnumerable<T> collection, delegate*<T, bool> predicate)
    {
        if (TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span))
        {
            return IndicesOf(span, predicate);
        }

        using PooledList<int> indices = collection.TryGetNonEnumeratedCount(out int elementCount) ? new(elementCount) : new();
        switch (collection)
        {
            case IList<T> iList:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (predicate(iList[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            case IReadOnlyList<T> iReadOnlyList:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (predicate(iReadOnlyList[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            case IIndexAccessible<T> indexAccessible:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (predicate(indexAccessible[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            default:
            {
                int currentIndex = 0;
                foreach (T item in collection)
                {
                    if (predicate(item))
                    {
                        indices.Add(currentIndex);
                    }

                    currentIndex++;
                }

                break;
            }
        }

        return indices.ToArray();
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this List<T> list, delegate*<T, bool> predicate)
        => IndicesOf(CollectionsMarshal.AsSpan(list), predicate);

    [Pure]
    public static unsafe int[] IndicesOf<T>(this T[] array, delegate*<T, bool> predicate)
        => IndicesOf(array.AsSpan(), predicate);

    [Pure]
    public static unsafe int[] IndicesOf<T>(this Span<T> span, delegate*<T, bool> predicate)
        => IndicesOf((ReadOnlySpan<T>)span, predicate);

    public static unsafe int IndicesOf<T>(this Span<T> span, delegate*<T, bool> predicate, Span<int> destination)
        => IndicesOf((ReadOnlySpan<T>)span, predicate, destination);

    [Pure]
    [SkipLocalsInit]
    public static unsafe int[] IndicesOf<T>(this ReadOnlySpan<T> span, delegate*<T, bool> predicate)
    {
        int length;
        if (!MemoryHelper.UseStackAlloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(span.Length);
            length = IndicesOf(span, predicate, indicesBuffer.AsSpan());
            return indicesBuffer[..length].ToArray();
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, predicate, indices);
        return indices[..length].ToArray();
    }

    public static unsafe int IndicesOf<T>(this ReadOnlySpan<T> span, delegate*<T, bool> predicate, Span<int> destination)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return 0;
        }

        int length = 0;
        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            bool equals = predicate(Unsafe.Add(ref firstItem, i));
            int equalsAsByte = Unsafe.As<bool, byte>(ref equals);
            destination[length] = i;
            length += equalsAsByte;
        }

        return length;
    }

    [Pure]
    public static int[] IndicesOf<T>(this IEnumerable<T> collection, T item) where T : IEquatable<T>
    {
        if (TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span))
        {
            return span.IndicesOf(item);
        }

        using PooledList<int> indices = collection.TryGetNonEnumeratedCount(out int elementCount) ? new(elementCount) : new();
        switch (collection)
        {
            case IList<T> iList:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (item.Equals(iList[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            case IReadOnlyList<T> iReadOnlyList:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (item.Equals(iReadOnlyList[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            case IIndexAccessible<T> indexAccessible:
            {
                for (int i = 0; i < elementCount; i++)
                {
                    if (item.Equals(indexAccessible[i]))
                    {
                        indices.Add(i);
                    }
                }

                break;
            }
            default:
            {
                int currentIndex = 0;
                foreach (T t in collection)
                {
                    if (item.Equals(t))
                    {
                        indices.Add(currentIndex);
                    }

                    currentIndex++;
                }

                break;
            }
        }

        return indices.ToArray();
    }
}
