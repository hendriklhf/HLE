using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Collections;

public static partial class CollectionHelpers
{
    [Pure]
    public static int[] IndicesOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        if (TryGetReadOnlySpan(enumerable, out ReadOnlySpan<T> span))
        {
            return IndicesOf(span, predicate);
        }

        using ValueList<int> indices = enumerable.TryGetNonEnumeratedCount(out int elementCount) ? new(elementCount) : new();
        switch (enumerable)
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
            case IIndexable<T> indexAccessible:
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
                int currentIndex = 0;
                foreach (T item in enumerable)
                {
                    if (predicate(item))
                    {
                        indices.Add(currentIndex);
                    }

                    currentIndex++;
                }

                break;
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
        if (!MemoryHelpers.UseStackalloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(span.Length);
            length = IndicesOf(span, predicate, indicesBuffer.AsSpan());
            return indicesBuffer.ToArray(..length);
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, predicate, indices);
        return indices.ToArray(..length);
    }

    public static int IndicesOf<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate, Span<int> destination)
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
            destination[length] = i;
            length += predicate(Unsafe.Add(ref firstItem, i)).AsByte();
        }

        return length;
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this IEnumerable<T> enumerable, delegate*<T, bool> predicate)
    {
        if (TryGetReadOnlySpan(enumerable, out ReadOnlySpan<T> span))
        {
            return IndicesOf(span, predicate);
        }

        using ValueList<int> indices = enumerable.TryGetNonEnumeratedCount(out int elementCount) ? new(elementCount) : new();
        switch (enumerable)
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
            case IIndexable<T> indexAccessible:
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
                int currentIndex = 0;
                foreach (T item in enumerable)
                {
                    if (predicate(item))
                    {
                        indices.Add(currentIndex);
                    }

                    currentIndex++;
                }

                break;
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
        if (!MemoryHelpers.UseStackalloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(span.Length);
            length = IndicesOf(span, predicate, indicesBuffer.AsSpan());
            return indicesBuffer.ToArray(..length);
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, predicate, indices);
        return indices.ToArray(..length);
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
            destination[length] = i;
            bool equals = predicate(Unsafe.Add(ref firstItem, i));
            length += equals.AsByte();
        }

        return length;
    }

    [Pure]
    public static int[] IndicesOf<T>(this IEnumerable<T> enumerable, T item) where T : IEquatable<T>
    {
        if (TryGetReadOnlySpan(enumerable, out ReadOnlySpan<T> span))
        {
            return span.IndicesOf(item);
        }

        using ValueList<int> indices = enumerable.TryGetNonEnumeratedCount(out int elementCount) ? new(elementCount) : new();
        switch (enumerable)
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
            case IIndexable<T> indexAccessible:
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
                int currentIndex = 0;
                foreach (T t in enumerable)
                {
                    if (item.Equals(t))
                    {
                        indices.Add(currentIndex);
                    }

                    currentIndex++;
                }

                break;
        }

        return indices.ToArray();
    }
}
