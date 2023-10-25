using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Marshalling;
using HLE.Memory;

namespace HLE.Collections;

public static partial class CollectionHelper
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
            return IndicesOf(span, item);
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

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, T item) where T : IEquatable<T>
        => IndicesOf(CollectionsMarshal.AsSpan(list), item);

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, T item) where T : IEquatable<T>
        => IndicesOf(array.AsSpan(), item);

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> span, T item) where T : IEquatable<T>
        => IndicesOf((ReadOnlySpan<T>)span, item);

    public static int IndicesOf<T>(this Span<T> span, T item, Span<int> destination) where T : IEquatable<T>
        => IndicesOf((ReadOnlySpan<T>)span, item, destination);

    [Pure]
    [SkipLocalsInit]
    public static int[] IndicesOf<T>(this ReadOnlySpan<T> span, T item) where T : IEquatable<T>
    {
        int length;
        if (!MemoryHelper.UseStackAlloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(span.Length);
            length = IndicesOf(span, item, indicesBuffer.AsSpan());
            return indicesBuffer[..length].ToArray();
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, item, indices);
        return indices[..length].ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int IndicesOf<T>(this ReadOnlySpan<T> span, T item, Span<int> destination) where T : IEquatable<T>
    {
        if (span.Length == 0)
        {
            return 0;
        }

        if (!RuntimeMarshal<T>.IsBitwiseEquatable())
        {
            return IndicesOfNonOptimizedFallback(span, item, destination);
        }

        return sizeof(T) switch
        {
            sizeof(byte) => IndicesOf(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, Unsafe.As<T, byte>(ref item), destination),
            sizeof(ushort) => IndicesOf(ref Unsafe.As<T, ushort>(ref MemoryMarshal.GetReference(span)), span.Length, Unsafe.As<T, ushort>(ref item), destination),
            sizeof(uint) => IndicesOf(ref Unsafe.As<T, uint>(ref MemoryMarshal.GetReference(span)), span.Length, Unsafe.As<T, uint>(ref item), destination),
            sizeof(ulong) => IndicesOf(ref Unsafe.As<T, ulong>(ref MemoryMarshal.GetReference(span)), span.Length, Unsafe.As<T, ulong>(ref item), destination),
            _ => IndicesOfNonOptimizedFallback(span, item, destination)
        };
    }

    private static int IndicesOf<T>(ref T items, int length, T item, Span<int> destination) where T : IEquatable<T>
    {
        int indicesLength = 0;
        int vector512Count = Vector512<T>.Count;
        if (Vector512.IsHardwareAccelerated && length >= vector512Count)
        {
            Vector512<T> searchVector = Vector512.Create(item);
            int startIndex = 0;
            while (length - startIndex >= vector512Count)
            {
                Vector512<T> itemsVector = Vector512.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                ulong equals = Vector512.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals > 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    destination[indicesLength++] = startIndex + index;
                    equals &= equals - 1;
                }

                startIndex += vector512Count;
            }

            ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
            int remainingLength = length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
                {
                    destination[indicesLength++] = startIndex + i;
                }
            }

            return indicesLength;
        }

        int vector256Count = Vector256<T>.Count;
        if (Vector256.IsHardwareAccelerated && length >= vector256Count)
        {
            Vector256<T> searchVector = Vector256.Create(item);
            int startIndex = 0;
            while (length - startIndex >= vector256Count)
            {
                Vector256<T> itemsVector = Vector256.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                uint equals = Vector256.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals > 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    destination[indicesLength++] = startIndex + index;
                    equals &= equals - 1;
                }

                startIndex += vector256Count;
            }

            ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
            int remainingLength = length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
                {
                    destination[indicesLength++] = startIndex + i;
                }
            }

            return indicesLength;
        }

        int vector128Count = Vector256<T>.Count;
        if (Vector128.IsHardwareAccelerated && length >= vector128Count)
        {
            Vector128<T> searchVector = Vector128.Create(item);
            int startIndex = 0;
            while (length - startIndex >= vector128Count)
            {
                Vector128<T> itemsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                uint equals = Vector128.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals > 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    destination[indicesLength++] = startIndex + index;
                    equals &= equals - 1;
                }

                startIndex += vector128Count;
            }

            ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
            int remainingLength = length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
                {
                    destination[indicesLength++] = startIndex + i;
                }
            }

            return indicesLength;
        }

        int vector64Count = Vector256<T>.Count;
        if (Vector64.IsHardwareAccelerated && length >= vector64Count)
        {
            Vector64<T> searchVector = Vector64.Create(item);
            int startIndex = 0;
            while (length - startIndex >= vector64Count)
            {
                Vector64<T> itemsVector = Vector64.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                uint equals = Vector64.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals > 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    destination[indicesLength++] = startIndex + index;
                    equals &= equals - 1;
                }

                startIndex += vector64Count;
            }

            ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
            int remainingLength = length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
                {
                    destination[indicesLength++] = startIndex + i;
                }
            }

            return indicesLength;
        }

        return IndicesOfNonOptimizedFallback(MemoryMarshal.CreateReadOnlySpan(ref items, length), item, destination);
    }

    private static int IndicesOfNonOptimizedFallback<T>(ReadOnlySpan<T> span, T item, Span<int> destination) where T : IEquatable<T>
    {
        int indicesLength = 0;
        int indexOfItem = span.IndexOf(item);
        int spanStartIndex = indexOfItem;
        while (indexOfItem >= 0)
        {
            destination[indicesLength++] = spanStartIndex;
            indexOfItem = span.SliceUnsafe(++spanStartIndex).IndexOf(item);
            spanStartIndex += indexOfItem;
        }

        return indicesLength;
    }
}
