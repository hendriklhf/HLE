using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Memory;
using HLE.Numerics;
using HLE.Strings;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

/// <summary>
/// A class to help with any kind of collections.
/// </summary>
public static class CollectionHelper
{
    [Pure]
    public static T? Random<T>(this IEnumerable<T> collection)
    {
        return TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span) ? Random(span) : Random(collection.ToArray());
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

    [Pure]
    public static string JoinToString<T>(this IEnumerable<T> collection, char separator)
    {
        if (typeof(char) == typeof(T))
        {
            if (!collection.TryGetReadOnlySpan(out ReadOnlySpan<char> chars))
            {
                chars = Unsafe.As<IEnumerable<T>, IEnumerable<char>>(ref collection).ToArray();
            }

            if (chars.Length == 0)
            {
                return string.Empty;
            }

            int charsWritten;
            int calculatedResultLength = chars.Length << 1;
            if (!MemoryHelper.UseStackAlloc<char>(calculatedResultLength))
            {
                using RentedArray<char> rentedBuffer = new(calculatedResultLength);
                charsWritten = StringHelper.Join(chars, separator, rentedBuffer);
                return new(rentedBuffer[..charsWritten]);
            }

            Span<char> buffer = stackalloc char[calculatedResultLength];
            charsWritten = StringHelper.Join(chars, separator, buffer);
            return new(buffer[..charsWritten]);
        }

        return string.Join(separator, collection);
    }

    [Pure]
    public static string JoinToString<T>(this IEnumerable<T> collection, string separator)
    {
        if (typeof(char) == typeof(T))
        {
            if (!collection.TryGetReadOnlySpan(out ReadOnlySpan<char> chars))
            {
                chars = Unsafe.As<IEnumerable<T>, IEnumerable<char>>(ref collection).ToArray();
            }

            if (chars.Length == 0)
            {
                return string.Empty;
            }

            int charsWritten;
            int calculatedResultLength = chars.Length + separator.Length * chars.Length;
            if (!MemoryHelper.UseStackAlloc<char>(calculatedResultLength))
            {
                using RentedArray<char> rentedBuffer = new(calculatedResultLength);
                charsWritten = StringHelper.Join(chars, separator, rentedBuffer);
                return new(rentedBuffer[..charsWritten]);
            }

            Span<char> buffer = stackalloc char[calculatedResultLength];
            charsWritten = StringHelper.Join(chars, separator, buffer);
            return new(buffer[..charsWritten]);
        }

        return string.Join(separator, collection);
    }

    [Pure]
    public static string ConcatToString<T>(this IEnumerable<T> collection)
    {
        if (typeof(T) == typeof(char))
        {
            if (!collection.TryGetReadOnlySpan(out ReadOnlySpan<char> chars))
            {
                chars = Unsafe.As<IEnumerable<T>, IEnumerable<char>>(ref collection).ToArray();
            }

            return new(chars);
        }

        if (typeof(T) == typeof(string))
        {
            if (!collection.TryGetReadOnlySpan(out ReadOnlySpan<string> strings))
            {
                strings = Unsafe.As<IEnumerable<T>, IEnumerable<string>>(ref collection).ToArray();
            }

            if (strings.Length == 0)
            {
                return string.Empty;
            }

            int estimatedAverageStringLength = (strings[0].Length + strings[strings.Length >> 1].Length + strings[^1].Length) / 3;
            using PoolBufferStringBuilder builder = new(estimatedAverageStringLength * strings.Length);
            for (int i = 0; i < strings.Length; i++)
            {
                builder.Append(strings[i]);
            }

            return builder.ToString();
        }

        return string.Concat(collection);
    }

    [Pure]
    [LinqTunnel]
    public static IEnumerable<T> Replace<T>([NoEnumeration] this IEnumerable<T> collection, Func<T, bool> predicate, T replacement)
    {
        foreach (T item in collection)
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
    public static unsafe T[] Replace<T>(this IEnumerable<T> collection, delegate*<T, bool> predicate, T replacement)
    {
        T[] array = collection.ToArray();
        Replace((Span<T>)array, predicate, replacement);
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
    public static T[][] Split<T>(this IEnumerable<T> collection, T separator) where T : IEquatable<T>
    {
        return TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span) ? Split(span, separator) : Split((ReadOnlySpan<T>)collection.ToArray(), separator);
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
    public static int[] IndicesOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        return TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span) ? IndicesOf(span, predicate) : IndicesOf((ReadOnlySpan<T>)collection.ToArray(), predicate);
    }

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, Func<T, bool> predicate)
    {
        return IndicesOf((ReadOnlySpan<T>)CollectionsMarshal.AsSpan(list), predicate);
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
        int length;
        if (!MemoryHelper.UseStackAlloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = new(span.Length);
            length = IndicesOf(span, predicate, indicesBuffer);
            return indicesBuffer[..length].ToArray();
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, predicate, indices);
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
    public static unsafe int[] IndicesOf<T>(this IEnumerable<T> collection, delegate*<T, bool> predicate)
    {
        return TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span) ? IndicesOf(span, predicate) : IndicesOf(collection.ToArray(), predicate);
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
        int length;
        if (!MemoryHelper.UseStackAlloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = new(span.Length);
            length = IndicesOf(span, predicate, indicesBuffer);
            return indicesBuffer[..length].ToArray();
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, predicate, indices);
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
    public static int[] IndicesOf<T>(this IEnumerable<T> collection, T item) where T : IEquatable<T>
    {
        return TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span) ? IndicesOf(span, item) : IndicesOf(collection.ToArray(), item);
    }

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, T item) where T : IEquatable<T>
    {
        return IndicesOf((ReadOnlySpan<T>)CollectionsMarshal.AsSpan(list), item);
    }

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, T item) where T : IEquatable<T>
    {
        return IndicesOf((ReadOnlySpan<T>)array, item);
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
        int length;
        if (!MemoryHelper.UseStackAlloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = new(span.Length);
            length = IndicesOf(span, item, indicesBuffer);
            return indicesBuffer[..length].ToArray();
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, item, indices);
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
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<(TKey, TValue)> collection) where TKey : notnull
    {
        if (!collection.TryGetReadOnlySpan<(TKey, TValue)>(out ReadOnlySpan<(TKey, TValue)> valuePairs))
        {
            return collection.ToDictionary(i => i.Item1, i => i.Item2);
        }

        Dictionary<TKey, TValue> result = new(valuePairs.Length);
        for (int i = 0; i < valuePairs.Length; i++)
        {
            (TKey, TValue) valuePair = valuePairs[i];
            result.Add(valuePair.Item1, valuePair.Item2);
        }

        return result;
    }

    [Pure]
    public static T[] Randomize<T>(this IEnumerable<T> collection)
    {
        T[] result;
        if (collection.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            result = span.ToArray();
            Randomize((Span<T>)result);
            return result;
        }

        result = collection.ToArray();
        Randomize((Span<T>)result);
        return result;
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
    public static T[] RandomCollection<T>(this IEnumerable<T> collection, int length)
    {
        return TryGetReadOnlySpan<T>(collection, out ReadOnlySpan<T> span) ? RandomCollection(span, length) : RandomCollection(collection.ToArray(), length);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out _) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddOrSet<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        if (!dictionary.TryAdd(key, value))
        {
            dictionary[key] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        if (TryGetSpan<T>(collection, out Span<T> mutableSpan))
        {
            span = mutableSpan;
            return true;
        }

        span = ReadOnlySpan<T>.Empty;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetReadOnlySpan<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out ReadOnlySpan<TTo> span)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetReadOnlySpan<TTo>(resultCollection, out span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetSpan<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out Span<TTo> span)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetSpan<TTo>(resultCollection, out span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetReadOnlyMemory<T>([NoEnumeration] this IEnumerable<T> collection, out ReadOnlyMemory<T> memory)
    {
        // ReSharper disable once OperatorIsCanBeUsed
        if (typeof(string) == collection.GetType())
        {
            ReadOnlyMemory<char> stringMemory = Unsafe.As<IEnumerable<T>, string>(ref collection).AsMemory();
            memory = Unsafe.As<ReadOnlyMemory<char>, ReadOnlyMemory<T>>(ref stringMemory);
            return true;
        }

        if (TryGetMemory<T>(collection, out Memory<T> mutableMemory))
        {
            memory = mutableMemory;
            return true;
        }

        memory = ReadOnlyMemory<T>.Empty;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetReadOnlyMemory<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out ReadOnlyMemory<TTo> memory)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetReadOnlyMemory<TTo>(resultCollection, out memory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetMemory<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out Memory<TTo> memory)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetMemory<TTo>(resultCollection, out memory);
    }

    /// <inheritdoc cref="MoveItem{T}(System.Span{T},int,int)"/>
    public static void MoveItem<T>(this List<T> list, int sourceIndex, int destinationIndex)
    {
        MoveItem(CollectionsMarshal.AsSpan(list), sourceIndex, destinationIndex);
    }

    /// <inheritdoc cref="MoveItem{T}(System.Span{T},int,int)"/>
    public static void MoveItem<T>(this T[] array, int sourceIndex, int destinationIndex)
    {
        MoveItem((Span<T>)array, sourceIndex, destinationIndex);
    }

    /// <summary>
    /// Moves an item in the collection from the source to the destination index
    /// and moves the items between the indices to fill the now empty source index.
    /// </summary>
    /// <param name="span">The collection the items will be moved in.</param>
    /// <param name="sourceIndex">The source index of the item that will be moved.</param>
    /// <param name="destinationIndex">The destination index of the moved item.</param>
    public static void MoveItem<T>(this Span<T> span, int sourceIndex, int destinationIndex)
    {
        if (sourceIndex == destinationIndex)
        {
            return;
        }

        if (Math.Abs(sourceIndex - destinationIndex) == 1)
        {
            (span[sourceIndex], span[destinationIndex]) = (span[destinationIndex], span[sourceIndex]);
            return;
        }

        T value = span[sourceIndex];
        if (sourceIndex > destinationIndex)
        {
            span[destinationIndex..sourceIndex].CopyTo(span[(destinationIndex + 1)..]);
        }
        else
        {
            span[(sourceIndex + 1)..(destinationIndex + 1)].CopyTo(span[sourceIndex..]);
        }

        span[destinationIndex] = value;
    }
}
