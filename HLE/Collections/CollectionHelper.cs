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

        ref T firstItem = ref span[0];
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

        ref T firstItem = ref span[0];
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
    public static T? Random<T>(this IEnumerable<T> collection)
    {
        return collection.ToArray().Random();
    }

    [Pure]
    public static T? Random<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list).Random();
    }

    [Pure]
    public static T? Random<T>(this T[] array)
    {
        return array.AsSpan().Random();
    }

    [Pure]
    public static T? Random<T>(this Span<T> span)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return default;
        }

        ref T firstItem = ref span[0];
        int randomIdx = HLE.Random.Int(0, spanLength - 1);
        return Unsafe.Add(ref firstItem, randomIdx);
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

        ref T firstItem = ref span[0];
        for (int i = 0; i < span.Length; i++)
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

        bool IsSeparator(T item) => item?.Equals(separator) == true;

        Span<int> indices = stackalloc int[span.Length];
        int indicesLength = IndicesOf(span, IsSeparator, indices);
        if (indicesLength == 0)
        {
            return new[]
            {
                span.ToArray()
            };
        }

        indices = indices[..indicesLength];

        List<T[]> result = new(indices.Length + 1);
        int start = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            Range range = new(new(start), new(indices[i]));
            Span<T> split = span[range];
            start = indices[i] + 1;
            if (split.Length > 0)
            {
                result.Add(split.ToArray());
            }
        }

        Span<T> end = span[(indices[^1] + 1)..];
        if (end.Length > 0)
        {
            result.Add(end.ToArray());
        }

        return result.ToArray();
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
        for (int i = 0; i < randomStringLength; i++)
        {
            randomString[i] = span.Random();
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
        for (int i = 0; i < span.Length; i++)
        {
            if (condition(span[i]))
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
        ref var firstOperation = ref operationSpan[0];
        for (int i = 0; i < operationSpan.Length; i++)
        {
            ref var op = ref Unsafe.Add(ref firstOperation, i);
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
        ref var firstOperation = ref operationSpan[0];
        ref T firstItem = ref span[0];
        for (int i = 0; i < operationSpan.Length; i++)
        {
            ref var op = ref Unsafe.Add(ref firstOperation, i);
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
        ref T firstItem = ref span[0];
        for (int i = 0; i < span.Length; i++)
        {
            int randomIdx = HLE.Random.Int(0, maxIdx);
            ref T item = ref Unsafe.Add(ref firstItem, i);
            (item, span[randomIdx]) = (span[randomIdx], item);
        }
    }

    public static RangeEnumerator GetEnumerator(this Range range) => new(range);
}
