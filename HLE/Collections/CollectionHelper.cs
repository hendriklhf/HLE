using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

/// <summary>
/// A class to help with any kind of collections.
/// </summary>
public static class CollectionHelper
{
    /// <summary>
    /// Will loop through an <see cref="IEnumerable{T}"/> and performs the given <paramref name="action"/> on each element.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="collection">The <see cref="IEnumerable{T}"/> that will be looped through.</param>
    /// <param name="action">The action that will be performed.</param>
    [Pure]
    public static T[] ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        return collection.ToArray().ForEach(action);
    }

    public static List<T> ForEach<T>(this List<T> list, Action<T> action)
    {
        CollectionsMarshal.AsSpan(list).ForEach(action);
        return list;
    }

    public static T[] ForEach<T>(this T[] array, Action<T> action)
    {
        Span<T> span = array;
        span.ForEach(action);
        return array;
    }

    public static void ForEach<T>(this Span<T> span, Action<T> action)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            action(Unsafe.Add(ref firstItem, i));
        }
    }

    /// <summary>
    /// Will loop through an <see cref="IEnumerable{T}"/> and performs the given <paramref name="action"/> on each element.<br/>
    /// The <see cref="int"/> parameter of <paramref name="action"/> is the index of the current item in the loop.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="collection">The <see cref="IEnumerable{T}"/> that will be looped through.</param>
    /// <param name="action">The action that will be performed.</param>
    public static T[] ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
    {
        return collection.ToArray().ForEach(action);
    }

    public static List<T> ForEach<T>(this List<T> list, Action<T, int> action)
    {
        CollectionsMarshal.AsSpan(list).ForEach(action);
        return list;
    }

    public static T[] ForEach<T>(this T[] array, Action<T, int> action)
    {
        array.AsSpan().ForEach(action);
        return array;
    }

    public static void ForEach<T>(this Span<T> span, Action<T, int> action)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            action(Unsafe.Add(ref firstItem, i), i);
        }
    }

    /// <summary>
    /// Checks if the <see cref="IEnumerable{T}"/> is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="collection">The checked collection.</param>
    /// <returns>True, if null or empty, false otherwise.</returns>
    [Pure]
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    [Pure]
    public static bool IsNullOrEmpty<T>(this List<T>? list)
    {
        return list is null or [];
    }

    [Pure]
    public static bool IsNullOrEmpty<T>(this T[]? array)
    {
        return array is null or [];
    }

    /// <summary>
    /// Return a random element from the <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="collection"/>.</typeparam>
    /// <param name="collection">The collection the random element will be taken from.</param>
    /// <returns>A random element or <see langword="null"/> if the <paramref name="collection"/> doesn't contain any elements.</returns>
    [Pure]
    public static ref T? Random<T>(this IEnumerable<T> collection)
    {
        return ref collection.ToArray().Random();
    }

    [Pure]
    public static ref T? Random<T>(this List<T> list)
    {
        return ref CollectionsMarshal.AsSpan(list).Random();
    }

    [Pure]
    public static ref T? Random<T>(this T[] array)
    {
        return ref array.AsSpan().Random();
    }

    [Pure]
    public static ref T? Random<T>(this Span<T> span)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return ref Unsafe.NullRef<T>()!;
        }

        ref T firstItem = ref MemoryMarshal.GetReference(span);
        int randomIdx = HLE.Random.Int(0, spanLength - 1);
        return ref Unsafe.Add(ref firstItem, randomIdx)!;
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/> separated by the <paramref name="separator"/>.
    /// </summary>
    /// <param name="collection">The <see cref="string"/> enumerable that will be converted to a <see cref="string"/>.</param>
    /// <param name="separator">The separator <see cref="char"/>.</param>
    /// <returns>Returns the <paramref name="collection"/> as a <see cref="string"/>.</returns>
    [Pure]
    public static string JoinToString(this IEnumerable<string> collection, char separator)
    {
        return string.Join(separator, collection);
    }

    [Pure]
    public static string JoinToString(this IEnumerable<string> collection, string separator)
    {
        return string.Join(separator, collection);
    }

    [Pure]
    public static string JoinToString(this IEnumerable<char> collection, char separator)
    {
        return string.Join(separator, collection);
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/> separated by the <paramref name="separator"/>.
    /// </summary>
    /// <param name="collection">The <see cref="IEnumerable{Char}"/> that will be converted to a <see cref="string"/>.</param>
    /// <param name="separator"></param>
    /// <returns></returns>
    [Pure]
    public static string JoinToString(this IEnumerable<char> collection, string separator)
    {
        return string.Join(separator, collection);
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    [Pure]
    public static string ConcatToString(this IEnumerable<char> collection)
    {
        return string.Concat(collection);
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    [Pure]
    public static string ConcatToString(this IEnumerable<string> collection)
    {
        return string.Concat(collection);
    }

    [Pure]
    public static T[] Replace<T>(this IEnumerable<T> collection, Func<T, bool> condition, T replacement)
    {
        return collection.ToArray().Replace(condition, replacement);
    }

    [Pure]
    public static List<T> Replace<T>(this List<T> list, Func<T, bool> condition, T replacement)
    {
        List<T> copy = new(list);
        CollectionsMarshal.AsSpan(copy).Replace(condition, replacement);
        return copy;
    }

    [Pure]
    public static T[] Replace<T>(this T[] array, Func<T, bool> condition, T replacement)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        copy.AsSpan().Replace(condition, replacement);
        return copy;
    }

    public static void Replace<T>(this Span<T> span, Func<T, bool> condition, T replacement)
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
            if (condition(item))
            {
                item = replacement;
            }
        }
    }

    [Pure]
    public static unsafe T[] Replace<T>(this IEnumerable<T> collection, delegate*<T, bool> condition, T replacement)
    {
        return collection.ToArray().Replace(condition, replacement);
    }

    [Pure]
    public static unsafe List<T> Replace<T>(this List<T> list, delegate*<T, bool> condition, T replacement)
    {
        List<T> copy = new(list);
        CollectionsMarshal.AsSpan(copy).Replace(condition, replacement);
        return copy;
    }

    [Pure]
    public static unsafe T[] Replace<T>(this T[] array, delegate*<T, bool> condition, T replacement)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        copy.AsSpan().Replace(condition, replacement);
        return copy;
    }

    public static unsafe void Replace<T>(this Span<T> span, delegate*<T, bool> condition, T replacement)
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
            if (condition(item))
            {
                item = replacement;
            }
        }
    }

    [Pure]
    public static T[][] Split<T>(this IEnumerable<T> collection, T separator)
    {
        return collection.ToArray().Split(separator);
    }

    [Pure]
    public static T[][] Split<T>(this List<T> list, T separator)
    {
        return CollectionsMarshal.AsSpan(list).Split(separator);
    }

    [Pure]
    public static T[][] Split<T>(this T[] array, T separator)
    {
        return array.AsSpan().Split(separator);
    }

    [Pure]
    public static T[][] Split<T>(this Span<T> span, T separator)
    {
        if (span.Length == 0)
        {
            return Array.Empty<T[]>();
        }

        Span<int> indices = stackalloc int[span.Length];
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
            Span<T> split = span[start..index];
            start = index + 1;
            if (split.Length > 0)
            {
                Unsafe.Add(ref firstResultValue, resultLength++) = split.ToArray();
            }
        }

        Span<T> end = span[(indices[indicesLength - 1] + 1)..];
        if (end.Length > 0)
        {
            Unsafe.Add(ref firstResultValue, resultLength) = end.ToArray();
        }

        return result[..resultLength];
    }

    [Pure]
    public static string RandomString(this IEnumerable<char> collection, int wordLength)
    {
        return collection.ToArray().RandomString(wordLength);
    }

    [Pure]
    public static string RandomString(this List<char> list, int wordLength)
    {
        return CollectionsMarshal.AsSpan(list).RandomString(wordLength);
    }

    [Pure]
    public static string RandomString(this char[] array, int wordLength)
    {
        return array.AsSpan().RandomString(wordLength);
    }

    [Pure]
    public static string RandomString(this Span<char> span, int wordLength)
    {
        Span<char> result = stackalloc char[wordLength];
        RandomString(span, result);
        return new(result);
    }

    public static void RandomString(this Span<char> span, Span<char> randomString)
    {
        int randomStringLength = randomString.Length;
        ref char firstChar = ref MemoryMarshal.GetReference(randomString);
        for (int i = 0; i < randomStringLength; i++)
        {
            Unsafe.Add(ref firstChar, i) = span.Random();
        }
    }

    [Pure]
    public static int[] IndicesOf<T>(this IEnumerable<T> collection, Func<T, bool> condition)
    {
        return collection.ToArray().IndicesOf(condition);
    }

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, Func<T, bool> condition)
    {
        return array.AsSpan().IndicesOf(condition);
    }

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, Func<T, bool> condition)
    {
        return CollectionsMarshal.AsSpan(list).IndicesOf(condition);
    }

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> span, Func<T, bool> condition)
    {
        Span<int> indices = stackalloc int[span.Length];
        int length = IndicesOf(span, condition, indices);
        return indices[..length].ToArray();
    }

    public static int IndicesOf<T>(this Span<T> span, Func<T, bool> condition, Span<int> indices)
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
            if (condition(Unsafe.Add(ref firstItem, i)))
            {
                indices[length++] = i;
            }
        }

        return length;
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this IEnumerable<T> collection, delegate*<T, bool> condition)
    {
        return collection.ToArray().IndicesOf(condition);
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this T[] array, delegate*<T, bool> condition)
    {
        return array.AsSpan().IndicesOf(condition);
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this List<T> list, delegate*<T, bool> condition)
    {
        return CollectionsMarshal.AsSpan(list).IndicesOf(condition);
    }

    [Pure]
    public static unsafe int[] IndicesOf<T>(this Span<T> span, delegate*<T, bool> condition)
    {
        Span<int> indices = stackalloc int[span.Length];
        int length = IndicesOf(span, condition, indices);
        return indices[..length].ToArray();
    }

    public static unsafe int IndicesOf<T>(this Span<T> span, delegate*<T, bool> condition, Span<int> indices)
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
            if (condition(Unsafe.Add(ref firstItem, i)))
            {
                indices[length++] = i;
            }
        }

        return length;
    }

    [Pure]
    public static int[] IndicesOf<T>(this IEnumerable<T> collection, T item)
    {
        return collection.ToArray().IndicesOf(item);
    }

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, T item)
    {
        return array.AsSpan().IndicesOf(item);
    }

    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, T item)
    {
        return CollectionsMarshal.AsSpan(list).IndicesOf(item);
    }

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> span, T item)
    {
        Span<int> indices = stackalloc int[span.Length];
        int length = IndicesOf(span, item, indices);
        return indices[..length].ToArray();
    }

    public static int IndicesOf<T>(this Span<T> span, T item, Span<int> indices)
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
            if (Unsafe.Add(ref firstItem, i)?.Equals(item) == true)
            {
                indices[length++] = i;
            }
        }

        return length;
    }

    [Pure]
    public static bool ContentEquals<T>(this IEnumerable<T> collection, IEnumerable<T> collection2)
    {
        return collection.ToArray().ContentEquals(collection2.ToArray());
    }

    [Pure]
    public static bool ContentEquals<T>(this List<T> list, List<T> list2)
    {
        return CollectionsMarshal.AsSpan(list).ContentEquals(CollectionsMarshal.AsSpan(list2));
    }

    [Pure]
    public static bool ContentEquals<T>(this T[] array, T[] array2)
    {
        return array.AsSpan().ContentEquals(array2.AsSpan());
    }

    [Pure]
    public static bool ContentEquals<T>(this Span<T> span, Span<T> span2)
    {
        return span.SequenceEqual(span2);
    }

    public static T[] ForRange<T>(this IEnumerable<T> collection, params (Range Range, Action<T> Action)[] operations)
    {
        return collection.ToArray().ForRange(operations);
    }

    public static List<T> ForRange<T>(this List<T> list, params (Range Range, Action<T> Action)[] operations)
    {
        CollectionsMarshal.AsSpan(list).ForRange(operations);
        return list;
    }

    public static T[] ForRange<T>(this T[] array, params (Range Range, Action<T> Action)[] operations)
    {
        array.AsSpan().ForRange(operations);
        return array;
    }

    public static void ForRange<T>(this Span<T> span, params (Range Range, Action<T> Action)[] operations)
    {
        Span<(Range Range, Action<T> Action)> operationSpan = operations;
        ref var firstOperation = ref MemoryMarshal.GetArrayDataReference(operations);
        for (int i = 0; i < operationSpan.Length; i++)
        {
            var op = Unsafe.Add(ref firstOperation, i);
            int start = op.Range.Start.Value;
            int end = op.Range.End.IsFromEnd ? span.Length - op.Range.End.Value - 1 : op.Range.End.Value;
            for (int j = start; j < end; j++)
            {
                op.Action(span[j]);
            }
        }
    }

    public static T[] ForRange<T>(this IEnumerable<T> collection, params (Range Range, Func<T, T> Func)[] operations)
    {
        return collection.ToArray().ForRange(operations);
    }

    public static List<T> ForRange<T>(this List<T> list, params (Range Range, Func<T, T> Func)[] operations)
    {
        CollectionsMarshal.AsSpan(list).ForRange(operations);
        return list;
    }

    public static T[] ForRange<T>(this T[] array, params (Range Range, Func<T, T> Func)[] operations)
    {
        array.AsSpan().ForRange(operations);
        return array;
    }

    public static void ForRange<T>(this Span<T> span, params (Range Range, Func<T, T> Func)[] operations)
    {
        Span<(Range Range, Func<T, T> Func)> operationSpan = operations;
        ref var firstOperation = ref MemoryMarshal.GetArrayDataReference(operations);
        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < operationSpan.Length; i++)
        {
            var op = Unsafe.Add(ref firstOperation, i);
            int start = op.Range.Start.Value;
            int end = op.Range.End.IsFromEnd ? span.Length - op.Range.End.Value - 1 : op.Range.End.Value;
            for (int j = start; j < end; j++)
            {
                ref T item = ref Unsafe.Add(ref firstItem, j);
                item = op.Func(item);
            }
        }
    }

    [Pure]
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<(TKey Key, TValue Value)> collection) where TKey : notnull
    {
        return collection.ToDictionary(i => i.Key, i => i.Value);
    }

    [Pure]
    public static T[] Randomize<T>(this IEnumerable<T> collection)
    {
        return collection.ToArray().Randomize();
    }

    [Pure]
    public static List<T> Randomize<T>(this List<T> list)
    {
        List<T> copy = new(list);
        CollectionsMarshal.AsSpan(copy).Randomize();
        return copy;
    }

    [Pure]
    public static T[] Randomize<T>(this T[] array)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        copy.AsSpan().Randomize();
        return copy;
    }

    public static void Randomize<T>(this Span<T> span)
    {
        if (span.Length <= 1)
        {
            return;
        }

        int maxIdx = span.Length - 1;
        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < span.Length; i++)
        {
            int randomIdx = HLE.Random.Int(0, maxIdx);
            ref T item = ref Unsafe.Add(ref firstItem, i);
            (item, span[randomIdx]) = (span[randomIdx], item);
        }
    }

    [Pure]
    public static T[] RandomCollection<T>(this IEnumerable<T> collection, int length)
    {
        return collection.ToArray().RandomCollection(length);
    }

    [Pure]
    public static T[] RandomCollection<T>(this T[] array, int length)
    {
        return array.AsSpan().RandomCollection(length);
    }

    [Pure]
    public static T[] RandomCollection<T>(this List<T> list, int length)
    {
        return CollectionsMarshal.AsSpan(list).RandomCollection(length);
    }

    [Pure]
    public static T[] RandomCollection<T>(this Span<T> span, int length)
    {
        if (span.Length == 0)
        {
            return Array.Empty<T>();
        }

        T[] result = new T[length];
        ref T firstItem = ref MemoryMarshal.GetArrayDataReference(result);
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref firstItem, i) = span.Random()!;
        }

        return result;
    }

    public static RangeEnumerator GetEnumerator(this Range range) => new(range);
}
