using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Memory;
using HLE.Numerics;

namespace HLE.Collections;

/// <summary>
/// A class to help with any kind of collections.
/// </summary>
public static class CollectionHelper
{
    [Pure]
    public static TContent? Random<TCollection, TContent>(this TCollection collection) where TCollection : IEnumerable<TContent>
    {
        return TryGetReadOnlySpan(collection, out ReadOnlySpan<TContent> span) ? Random(span) : Random(collection.ToArray());
    }

    [Pure]
    public static ref T? Random<T>(this List<T> list)
    {
        return ref Random(CollectionsMarshal.AsSpan(list));
    }

    [Pure]
    public static ref T? Random<T>(this T[] array)
    {
        return ref Random((ReadOnlySpan<T>)array);
    }

    [Pure]
    public static ref T? Random<T>(this Span<T> span)
    {
        return ref Random((ReadOnlySpan<T>)span);
    }

    [Pure]
    public static ref T? Random<T>(this ReadOnlySpan<T> span)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return ref Unsafe.NullRef<T>()!;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        int randomIdx = System.Random.Shared.Next(0, spanLength);
        return ref Unsafe.Add(ref firstItem, randomIdx)!;
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/> separated by the <paramref name="separator"/>.
    /// </summary>
    /// <param name="collection">The <see cref="string"/> enumerable that will be converted to a <see cref="string"/>.</param>
    /// <param name="separator">The separator <see cref="char"/>.</param>
    /// <returns>Returns the <paramref name="collection"/> as a <see cref="string"/>.</returns>
    [Pure]
    public static string JoinToString<TCollection, TContent>(this TCollection collection, char separator) where TCollection : IEnumerable<TContent>
    {
        return string.Join(separator, collection);
    }

    [Pure]
    public static string JoinToString<TCollection, TContent>(this TCollection collection, string separator) where TCollection : IEnumerable<TContent>
    {
        return string.Join(separator, collection);
    }

    [Pure]
    public static string ConcatToString<TCollection, TContent>(this TCollection collection) where TCollection : IEnumerable<TContent>
    {
        return string.Concat(collection);
    }

    [Pure]
    public static IEnumerable<TContent> Replace<TCollection, TContent>(this TCollection collection, Func<TContent, bool> predicate, TContent replacement) where TCollection : IEnumerable<TContent>
    {
        foreach (TContent item in collection)
        {
            yield return predicate(item) ? replacement : item;
        }
    }

    [Pure]
    public static List<T> Replace<T>(this List<T> list, Func<T, bool> predicate, T replacement)
    {
        List<T> copy = new(list);
        Replace(CollectionsMarshal.AsSpan(copy), predicate, replacement);
        return copy;
    }

    [Pure]
    public static T[] Replace<T>(this T[] array, Func<T, bool> predicate, T replacement)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        Replace((Span<T>)copy, predicate, replacement);
        return copy;
    }

    public static void Replace<T>(this Span<T> span, Func<T, bool> predicate, T replacement)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            ref T item = ref Unsafe.Add(ref firstItem, i);
            if (predicate(item))
            {
                item = replacement;
            }
        }
    }

    [Pure]
    public static unsafe TContent[] Replace<TCollection, TContent>(this TCollection collection, delegate*<TContent, bool> predicate, TContent replacement) where TCollection : IEnumerable<TContent>
    {
        TContent[] array = collection.ToArray();
        Replace((Span<TContent>)array, predicate, replacement);
        return array;
    }

    [Pure]
    public static unsafe List<T> Replace<T>(this List<T> list, delegate*<T, bool> predicate, T replacement)
    {
        List<T> copy = new(list);
        Replace(CollectionsMarshal.AsSpan(copy), predicate, replacement);
        return copy;
    }

    [Pure]
    public static unsafe T[] Replace<T>(this T[] array, delegate*<T, bool> predicate, T replacement)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        Replace((Span<T>)copy, predicate, replacement);
        return copy;
    }

    public static unsafe void Replace<T>(this Span<T> span, delegate*<T, bool> predicate, T replacement)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            ref T item = ref Unsafe.Add(ref firstItem, i);
            if (predicate(item))
            {
                item = replacement;
            }
        }
    }

    [Pure]
    public static TContent[][] Split<TCollection, TContent>(this TCollection collection, TContent separator) where TCollection : IEnumerable<TContent> where TContent : IEquatable<TContent>
    {
        return TryGetReadOnlySpan(collection, out ReadOnlySpan<TContent> span) ? Split(span, separator) : Split((ReadOnlySpan<TContent>)collection.ToArray(), separator);
    }

    [Pure]
    public static T[][] Split<T>(this List<T> list, T separator) where T : IEquatable<T>
    {
        return Split((ReadOnlySpan<T>)CollectionsMarshal.AsSpan(list), separator);
    }

    [Pure]
    public static T[][] Split<T>(this T[] array, T separator) where T : IEquatable<T>
    {
        return Split((ReadOnlySpan<T>)array, separator);
    }

    [Pure]
    public static T[][] Split<T>(this Span<T> span, T separator) where T : IEquatable<T>
    {
        return Split((ReadOnlySpan<T>)span, separator);
    }

    [Pure]
    public static T[][] Split<T>(this ReadOnlySpan<T> span, T separator) where T : IEquatable<T>
    {
        if (span.Length == 0)
        {
            return Array.Empty<T[]>();
        }

        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
        int indicesLength = IndicesOf(span, separator, indices);
        if (indicesLength == 0)
        {
            return new[]
            {
                span.ToArray()
            };
        }

        T[][] result = new T[indicesLength + 1][];
        ref T[] firstResultValue = ref MemoryMarshal.GetArrayDataReference(result);
        int resultLength = 0;
        int start = 0;
        ref int firstIndex = ref MemoryMarshal.GetReference(indices);
        for (int i = 0; i < indicesLength; i++)
        {
            int index = Unsafe.Add(ref firstIndex, i);
            ReadOnlySpan<T> split = span[start..index];
            start = index + 1;
            if (split.Length > 0)
            {
                Unsafe.Add(ref firstResultValue, resultLength++) = split.ToArray();
            }
        }

        ReadOnlySpan<T> end = span[(indices[indicesLength - 1] + 1)..];
        if (end.Length > 0)
        {
            Unsafe.Add(ref firstResultValue, resultLength) = end.ToArray();
        }

        return result[..resultLength];
    }

    [Pure]
    public static int[] IndicesOf<TCollection, TContent>(this TCollection collection, Func<TContent, bool> predicate) where TCollection : IEnumerable<TContent>
    {
        if (TryGetReadOnlySpan(collection, out ReadOnlySpan<TContent> span))
        {
            return IndicesOf(span, predicate);
        }

        using PoolBufferList<int> indices = new(50, 25);
        int index = 0;
        foreach (TContent item in collection)
        {
            if (predicate(item))
            {
                indices.Add(index);
            }

            index++;
        }

        return indices.ToArray();
    }

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, Func<T, bool> predicate)
    {
        return IndicesOf(CollectionsMarshal.AsSpan(list), predicate);
    }

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, Func<T, bool> predicate)
    {
        return IndicesOf((ReadOnlySpan<T>)array, predicate);
    }

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> span, Func<T, bool> predicate)
    {
        return IndicesOf((ReadOnlySpan<T>)span, predicate);
    }

    public static int IndicesOf<T>(this Span<T> span, Func<T, bool> predicate, Span<int> indices)
    {
        return IndicesOf((ReadOnlySpan<T>)span, predicate, indices);
    }

    [Pure]
    public static int[] IndicesOf<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
        int length = IndicesOf(span, predicate, indices);
        return indices[..length].ToArray();
    }

    public static int IndicesOf<T>(this ReadOnlySpan<T> span, Func<T, bool> predicate, Span<int> indices)
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
            if (predicate(Unsafe.Add(ref firstItem, i)))
            {
                indices[length++] = i;
            }
        }

        return length;
    }

    [Pure]
    public static unsafe int[] IndicesOf<TCollection, TContent>(this TCollection collection, delegate*<TContent, bool> predicate) where TCollection : IEnumerable<TContent>
    {
        return TryGetReadOnlySpan(collection, out ReadOnlySpan<TContent> span) ? IndicesOf(span, predicate) : IndicesOf(collection.ToArray(), predicate);
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this List<T> list, delegate*<T, bool> predicate)
    {
        return IndicesOf((ReadOnlySpan<T>)CollectionsMarshal.AsSpan(list), predicate);
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this T[] array, delegate*<T, bool> predicate)
    {
        return IndicesOf((ReadOnlySpan<T>)array, predicate);
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this Span<T> span, delegate*<T, bool> predicate)
    {
        return IndicesOf((ReadOnlySpan<T>)span, predicate);
    }

    public static unsafe int IndicesOf<T>(this Span<T> span, delegate*<T, bool> predicate, Span<int> indices)
    {
        return IndicesOf((ReadOnlySpan<T>)span, predicate, indices);
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this ReadOnlySpan<T> span, delegate*<T, bool> predicate)
    {
        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
        int length = IndicesOf(span, predicate, indices);
        return indices[..length].ToArray();
    }

    public static unsafe int IndicesOf<T>(this ReadOnlySpan<T> span, delegate*<T, bool> predicate, Span<int> indices)
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
            indices[length] = i;
            length += equalsAsByte;
        }

        return length;
    }

    [Pure]
    public static int[] IndicesOf<TCollection, TContent>(this TCollection collection, TContent item) where TCollection : IEnumerable<TContent> where TContent : IEquatable<TContent>
    {
        if (TryGetReadOnlySpan(collection, out ReadOnlySpan<TContent> span))
        {
            return IndicesOf(span, item);
        }

        using PoolBufferList<int> indices = new(50, 25);
        int index = 0;
        foreach (TContent t in collection)
        {
            if (t.Equals(item))
            {
                indices.Add(index);
            }

            index++;
        }

        return indices.ToArray();
    }

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, T item) where T : IEquatable<T>
    {
        return IndicesOf(CollectionsMarshal.AsSpan(list), item);
    }

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, T item) where T : IEquatable<T>
    {
        return IndicesOf((Span<T>)array, item);
    }

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> span, T item) where T : IEquatable<T>
    {
        return IndicesOf((ReadOnlySpan<T>)span, item);
    }

    public static int IndicesOf<T>(this Span<T> span, T item, Span<int> indices) where T : IEquatable<T>
    {
        return IndicesOf((ReadOnlySpan<T>)span, item, indices);
    }

    [Pure]
    public static int[] IndicesOf<T>(this ReadOnlySpan<T> span, T item) where T : IEquatable<T>
    {
        Span<int> indices = MemoryHelper.UseStackAlloc<int>(span.Length) ? stackalloc int[span.Length] : new int[span.Length];
        int length = IndicesOf(span, item, indices);
        return indices[..length].ToArray();
    }

    public static int IndicesOf<T>(this ReadOnlySpan<T> span, T item, Span<int> indices) where T : IEquatable<T>
    {
        if (span.Length == 0)
        {
            return 0;
        }

        int indicesLength = 0;
        int idx = span.IndexOf(item);
        int spanStartIndex = idx;
        while (idx != -1)
        {
            indices[indicesLength++] = spanStartIndex;
            idx = span[++spanStartIndex..].IndexOf(item);
            spanStartIndex += idx;
        }

        return indicesLength;
    }

    [Pure]
    public static Dictionary<TKey, TValue> ToDictionary<TCollection, TKey, TValue>(this TCollection collection) where TCollection : IEnumerable<(TKey, TValue)> where TKey : notnull
    {
        return collection.ToDictionary(i => i.Item1, i => i.Item2);
    }

    [Pure]
    public static TContent[] Randomize<TCollection, TContent>(this TCollection collection) where TCollection : IEnumerable<TContent>
    {
        TContent[] array = collection.ToArray();
        Randomize((Span<TContent>)array);
        return array;
    }

    [Pure]
    public static List<T> Randomize<T>(this List<T> list)
    {
        List<T> copy = new(list);
        Randomize(CollectionsMarshal.AsSpan(copy));
        return copy;
    }

    [Pure]
    public static T[] Randomize<T>(this T[] array)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        Randomize((Span<T>)copy);
        return copy;
    }

    public static void Randomize<T>(this Span<T> span)
    {
        if (span.Length <= 1)
        {
            return;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < span.Length; i++)
        {
            int randomIdx = System.Random.Shared.Next(0, span.Length);
            ref T item = ref Unsafe.Add(ref firstItem, i);
            (item, span[randomIdx]) = (span[randomIdx], item);
        }
    }

    [Pure]
    public static TContent[] RandomCollection<TCollection, TContent>(this TCollection collection, int length) where TCollection : IEnumerable<TContent>
    {
        return TryGetReadOnlySpan(collection, out ReadOnlySpan<TContent> span) ? RandomCollection(span, length) : RandomCollection(collection.ToArray(), length);
    }

    [Pure]
    public static T[] RandomCollection<T>(this T[] array, int length)
    {
        return RandomCollection((Span<T>)array, length);
    }

    [Pure]
    public static T[] RandomCollection<T>(this List<T> list, int length)
    {
        return RandomCollection(CollectionsMarshal.AsSpan(list), length);
    }

    [Pure]
    public static T[] RandomCollection<T>(this Span<T> span, int length)
    {
        return RandomCollection((ReadOnlySpan<T>)span, length);
    }

    [Pure]
    public static T[] RandomCollection<T>(this ReadOnlySpan<T> span, int length)
    {
        if (span.Length == 0)
        {
            return Array.Empty<T>();
        }

        T[] result = new T[length];
        if (!MemoryHelper.UseStackAlloc<int>(length))
        {
            using RentedArray<int> randomIndicesBuffer = new(length);
            System.Random.Shared.Fill(randomIndicesBuffer.Span);
            for (int i = 0; i < length; i++)
            {
                int randomIndex = NumberHelper.SetSignBitToZero(randomIndicesBuffer[i]) % span.Length;
                result[i] = span[randomIndex];
            }

            return result;
        }

        Span<int> randomIndices = stackalloc int[length];
        System.Random.Shared.Fill(randomIndices);
        for (int i = 0; i < length; i++)
        {
            int randomIndex = NumberHelper.SetSignBitToZero(randomIndices[i]) % span.Length;
            result[i] = span[randomIndex];
        }

        return result;
    }

    [Pure]
    public static RangeEnumerator GetEnumerator(this Range range) => new(range);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void FillAscending(this Span<int> span, int start = 0)
    {
#if NET8_0_OR_GREATER
        int vector512Count = Vector512<int>.Count;
        if (Vector512.IsHardwareAccelerated && span.Length >= vector512Count)
        {
            var ascendingValueAdditions = Vector512.Create(0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15);
            while (span.Length >= vector512Count)
            {
                Vector512<int> startValues = Vector512.Create(start);
                Vector512<int> values = Vector512.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector512Count;
                span = span[vector512Count..];
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + i;
            }

            return;
        }
#endif

        int vector256Count = Vector256<int>.Count;
        if (Vector256.IsHardwareAccelerated && span.Length >= vector256Count)
        {
            Vector256<int> ascendingValueAdditions = Vector256.Create(0, 1, 2, 3, 4, 5, 6, 7);
            while (span.Length >= vector256Count)
            {
                Vector256<int> startValues = Vector256.Create(start);
                Vector256<int> values = Vector256.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector256Count;
                span = span[vector256Count..];
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + i;
            }

            return;
        }

        int vector128Count = Vector128<int>.Count;
        if (Vector128.IsHardwareAccelerated && span.Length >= vector128Count)
        {
            Vector128<int> ascendingValueAdditions = Vector128.Create(0, 1, 2, 3);
            while (span.Length >= vector128Count)
            {
                Vector128<int> startValues = Vector128.Create(start);
                Vector128<int> values = Vector128.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector128Count;
                span = span[vector128Count..];
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + i;
            }

            return;
        }

        int vector64Count = Vector64<int>.Count;
        if (Vector64.IsHardwareAccelerated && span.Length >= vector64Count)
        {
            Vector64<int> ascendingValueAdditions = Vector64.Create(0, 1);
            while (span.Length >= vector64Count)
            {
                Vector64<int> startValues = Vector64.Create(start);
                Vector64<int> values = Vector64.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector64Count;
                span = span[vector64Count..];
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + i;
            }

            return;
        }

        int spanLength = span.Length;
        ref int firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            Unsafe.Add(ref firstItem, i) = start + i;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void FillAscending(this Span<ushort> span, ushort start = 0)
    {
#if NET8_0_OR_GREATER
        ushort vector512Count = (ushort)Vector512<ushort>.Count;
        if (Vector512.IsHardwareAccelerated && span.Length >= vector512Count)
        {
            Vector512<ushort> ascendingValueAdditions = Vector512.Create((ushort)0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
                30, 31);
            while (span.Length >= vector512Count)
            {
                Vector512<ushort> startValues = Vector512.Create(start);
                Vector512<ushort> values = Vector512.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector512Count;
                span = span[vector512Count..];
            }

            for (ushort i = 0; i < span.Length; i++)
            {
                span[i] = (ushort)(start + i);
            }

            return;
        }
#endif

        ushort vector256Count = (ushort)Vector256<ushort>.Count;
        if (Vector256.IsHardwareAccelerated && span.Length >= vector256Count)
        {
            Vector256<ushort> ascendingValueAdditions = Vector256.Create((ushort)0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15);
            while (span.Length >= vector256Count)
            {
                Vector256<ushort> startValues = Vector256.Create(start);
                Vector256<ushort> values = Vector256.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector256Count;
                span = span[vector256Count..];
            }

            for (ushort i = 0; i < span.Length; i++)
            {
                span[i] = (ushort)(start + i);
            }

            return;
        }

        ushort vector128Count = (ushort)Vector128<ushort>.Count;
        if (Vector128.IsHardwareAccelerated && span.Length >= vector128Count)
        {
            Vector128<ushort> ascendingValueAdditions = Vector128.Create((ushort)0, 1, 2, 3, 4, 5, 6, 7);
            while (span.Length >= vector128Count)
            {
                Vector128<ushort> startValues = Vector128.Create(start);
                Vector128<ushort> values = Vector128.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector128Count;
                span = span[vector128Count..];
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (ushort)(start + i);
            }

            return;
        }

        ushort vector64Count = (ushort)Vector64<ushort>.Count;
        if (Vector64.IsHardwareAccelerated && span.Length >= vector64Count)
        {
            Vector64<ushort> ascendingValueAdditions = Vector64.Create((ushort)0, 1, 2, 3);
            while (span.Length >= vector64Count)
            {
                Vector64<ushort> startValues = Vector64.Create(start);
                Vector64<ushort> values = Vector64.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector64Count;
                span = span[vector64Count..];
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = (ushort)(start + i);
            }

            return;
        }

        int spanLength = span.Length;
        ref ushort firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            Unsafe.Add(ref firstItem, i) = (ushort)(start + i);
        }
    }

    public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out _) = value;
    }

    public static void AddOrSet<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        if (!dictionary.TryAdd(key, value))
        {
            dictionary[key] = value;
        }
    }

    public static bool TryGetReadOnlySpan<T>([NoEnumeration] this IEnumerable<T> collection, out ReadOnlySpan<T> span)
    {
        // ReSharper disable once OperatorIsCanBeUsed
        if (typeof(string) == collection.GetType())
        {
            string str = Unsafe.As<IEnumerable<T>, string>(ref collection);
            ref char firstChar = ref MemoryMarshal.GetReference(str.AsSpan());
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<char, T>(ref firstChar), str.Length);
            return true;
        }

        if (TryGetSpan(collection, out Span<T> mutableSpan))
        {
            span = mutableSpan;
            return true;
        }

        span = ReadOnlySpan<T>.Empty;
        return false;
    }

    public static bool TryGetSpan<T>([NoEnumeration] this IEnumerable<T> collection, out Span<T> span)
    {
        if (typeof(T[]) == collection.GetType())
        {
            span = Unsafe.As<IEnumerable<T>, T[]>(ref collection);
            return true;
        }

        if (typeof(List<T>) == collection.GetType())
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<IEnumerable<T>, List<T>>(ref collection));
            return true;
        }

        span = Span<T>.Empty;
        return false;
    }

    public static bool TryGetReadOnlyMemory<T>([NoEnumeration] this IEnumerable<T> collection, out ReadOnlyMemory<T> memory)
    {
        // ReSharper disable once OperatorIsCanBeUsed
        if (typeof(string) == collection.GetType())
        {
            ReadOnlyMemory<char> stringMemory = Unsafe.As<IEnumerable<T>, string>(ref collection).AsMemory();
            memory = Unsafe.As<ReadOnlyMemory<char>, ReadOnlyMemory<T>>(ref stringMemory);
            return true;
        }

        if (TryGetMemory(collection, out Memory<T> mutableMemory))
        {
            memory = mutableMemory;
            return true;
        }

        memory = ReadOnlyMemory<T>.Empty;
        return false;
    }

    public static bool TryGetMemory<T>([NoEnumeration] this IEnumerable<T> collection, out Memory<T> memory)
    {
        if (typeof(T[]) == collection.GetType())
        {
            memory = Unsafe.As<IEnumerable<T>, T[]>(ref collection);
            return true;
        }

        if (typeof(List<T>) == collection.GetType())
        {
            memory = CollectionsMarshal.AsSpan(Unsafe.As<IEnumerable<T>, List<T>>(ref collection)).AsMemoryDangerous();
            return true;
        }

        memory = Memory<T>.Empty;
        return false;
    }
}
