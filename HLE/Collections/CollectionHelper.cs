using System;
using System.Collections.Generic;
using System.Linq;
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

    internal static void ForEach<T>(this Span<T> span, Action<T> action)
    {
        for (int i = 0; i < span.Length; i++)
        {
            action(span[i]);
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

    internal static void ForEach<T>(this Span<T> span, Action<T, int> action)
    {
        for (int i = 0; i < span.Length; i++)
        {
            action(span[i], i);
        }
    }

    /// <summary>
    /// Checks if the <see cref="IEnumerable{T}"/> is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="collection">The checked collection.</param>
    /// <returns>True, if null or empty, false otherwise.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    public static bool IsNullOrEmpty<T>(this List<T>? list)
    {
        return list is null or [];
    }

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
    public static T? Random<T>(this IEnumerable<T> collection)
    {
        return collection.ToArray().Random();
    }

    public static T? Random<T>(this List<T> list)
    {
        return CollectionsMarshal.AsSpan(list).Random();
    }

    public static T? Random<T>(this T[] array)
    {
        return array.AsSpan().Random();
    }

    internal static T? Random<T>(this Span<T> span)
    {
        return span.Length == 0 ? default : span[HLE.Random.Int(0, span.Length - 1)];
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/> separated by the <paramref name="separator"/>.
    /// </summary>
    /// <param name="collection">The <see cref="string"/> enumerable that will be converted to a <see cref="string"/>.</param>
    /// <param name="separator">The separator <see cref="char"/>.</param>
    /// <returns>Returns the <paramref name="collection"/> as a <see cref="string"/>.</returns>
    public static string JoinToString(this IEnumerable<string> collection, char separator)
    {
        return string.Join(separator, collection);
    }

    public static string JoinToString(this IEnumerable<string> collection, string separator)
    {
        return string.Join(separator, collection);
    }

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
    public static string JoinToString(this IEnumerable<char> collection, string separator)
    {
        return string.Join(separator, collection);
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static string ConcatToString(this IEnumerable<char> collection)
    {
        return string.Concat(collection);
    }

    /// <summary>
    /// Concatenates every element of the <paramref name="collection"/>
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static string ConcatToString(this IEnumerable<string> collection)
    {
        return string.Concat(collection);
    }

    public static T[] Replace<T>(this IEnumerable<T> collection, Func<T, bool> condition, T replacement)
    {
        return collection.ToArray().Replace(condition, replacement);
    }

    public static List<T> Replace<T>(this List<T> list, Func<T, bool> condition, T replacement)
    {
        List<T> copy = new(list);
        CollectionsMarshal.AsSpan(copy).Replace(condition, replacement);
        return copy;
    }

    public static T[] Replace<T>(this T[] array, Func<T, bool> condition, T replacement)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        copy.AsSpan().Replace(condition, replacement);
        return copy;
    }

    internal static void Replace<T>(this Span<T> span, Func<T, bool> condition, T replacement)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (condition(span[i]))
            {
                span[i] = replacement;
            }
        }
    }

    public static T[] SelectMany<T>(this IEnumerable<IEnumerable<T>> collection)
    {
        return collection.SelectMany(t => t).ToArray();
    }

    public static T[][] Split<T>(this IEnumerable<T> collection, T separator)
    {
        return collection.ToArray().Split(separator);
    }

    public static T[][] Split<T>(this List<T> list, T separator)
    {
        return CollectionsMarshal.AsSpan(list).Split(separator);
    }

    public static T[][] Split<T>(this T[] array, T separator)
    {
        return array.AsSpan().Split(separator);
    }

    internal static T[][] Split<T>(this Span<T> span, T separator)
    {
        bool IsSeparator(T item) => item?.Equals(separator) == true;

        Span<int> indices = span.IndicesOf(IsSeparator);
        List<T[]> result = new(indices.Length + 1);
        int start = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            Span<T> split = span[start..indices[i]];
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

    public static IEnumerable<T> WhereP<T>(this IEnumerable<T> collection, params Func<T, bool>[] predicates)
    {
        return collection.Where(i => predicates.All(p => p(i)));
    }

    public static string RandomString(this IEnumerable<char> collection, int wordLength)
    {
        return collection.ToArray().RandomString(wordLength);
    }

    public static string RandomString(this List<char> list, int wordLength)
    {
        return CollectionsMarshal.AsSpan(list).RandomString(wordLength);
    }

    public static string RandomString(this char[] array, int wordLength)
    {
        return array.AsSpan().RandomString(wordLength);
    }

    internal static string RandomString(this Span<char> span, int wordLength)
    {
        Span<char> result = stackalloc char[wordLength];
        for (int i = 0; i < wordLength; i++)
        {
            result[i] = span.Random();
        }

        return new(result);
    }

    public static int[] IndicesOf<T>(this IEnumerable<T> collection, Func<T, bool> condition)
    {
        return collection.ToArray().IndicesOf(condition);
    }

    public static int[] IndicesOf<T>(this T[] array, Func<T, bool> condition)
    {
        return array.AsSpan().IndicesOf(condition);
    }

    public static int[] IndicesOf<T>(this List<T> list, Func<T, bool> condition)
    {
        return CollectionsMarshal.AsSpan(list).IndicesOf(condition);
    }

    internal static int[] IndicesOf<T>(this Span<T> span, Func<T, bool> condition)
    {
        Span<int> indices = stackalloc int[span.Length];
        int count = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (condition(span[i]))
            {
                indices[count++] = i;
            }
        }

        return indices[..count].ToArray();
    }

    public static bool ContentEquals<T>(this IEnumerable<T> collection, IEnumerable<T> collection2)
    {
        return collection.ToArray().ContentEquals(collection2.ToArray());
    }

    public static bool ContentEquals<T>(this List<T> list, List<T> list2)
    {
        return CollectionsMarshal.AsSpan(list).ContentEquals(CollectionsMarshal.AsSpan(list2));
    }

    public static bool ContentEquals<T>(this T[] array, T[] array2)
    {
        return array.AsSpan().ContentEquals(array2.AsSpan());
    }

    internal static bool ContentEquals<T>(this Span<T> span, Span<T> span2)
    {
        return span.SequenceEqual(span2);
    }

    public static T[] ForEachByRange<T>(this IEnumerable<T> collection, params (Range Range, Action<T> Action)[] operations)
    {
        return collection.ToArray().ForEachByRange(operations);
    }

    public static List<T> ForEachByRange<T>(this List<T> list, params (Range Range, Action<T> Action)[] operations)
    {
        CollectionsMarshal.AsSpan(list).ForEachByRange(operations);
        return list;
    }

    public static T[] ForEachByRange<T>(this T[] array, params (Range Range, Action<T> Action)[] operations)
    {
        array.AsSpan().ForEachByRange(operations);
        return array;
    }

    internal static void ForEachByRange<T>(this Span<T> span, params (Range Range, Action<T> Action)[] operations)
    {
        Span<(Range Range, Action<T> Action)> operationSpan = operations;
        for (int i = 0; i < operationSpan.Length; i++)
        {
            var op = operationSpan[i];
            int start = op.Range.Start.Value;
            int end = op.Range.End.IsFromEnd ? span.Length - op.Range.End.Value - 1 : op.Range.End.Value;
            for (int j = start; j < end; j++)
            {
                op.Action(span[j]);
            }
        }
    }

    public static T[] ForEachByRange<T>(this IEnumerable<T> collection, params (Range Range, Func<T, T> Func)[] operations)
    {
        return collection.ToArray().ForEachByRange(operations);
    }

    public static List<T> ForEachByRange<T>(this List<T> list, params (Range Range, Func<T, T> Func)[] operations)
    {
        CollectionsMarshal.AsSpan(list).ForEachByRange(operations);
        return list;
    }

    public static T[] ForEachByRange<T>(this T[] array, params (Range Range, Func<T, T> Func)[] operations)
    {
        array.AsSpan().ForEachByRange(operations);
        return array;
    }

    internal static void ForEachByRange<T>(this Span<T> span, params (Range Range, Func<T, T> Func)[] operations)
    {
        Span<(Range Range, Func<T, T> Func)> operationSpan = operations;
        for (int i = 0; i < operationSpan.Length; i++)
        {
            var op = operationSpan[i];
            int start = op.Range.Start.Value;
            int end = op.Range.End.IsFromEnd ? span.Length - op.Range.End.Value - 1 : op.Range.End.Value;
            for (int j = start; j < end; j++)
            {
                span[j] = op.Func(span[j]);
            }
        }
    }

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<(TKey Key, TValue Value)> collection) where TKey : notnull
    {
        return collection.ToDictionary(i => i.Key, i => i.Value);
    }

    public static T[] Randomize<T>(this IEnumerable<T> collection)
    {
        return collection.ToArray().Randomize();
    }

    public static List<T> Randomize<T>(this List<T> list)
    {
        List<T> copy = new(list);
        int maxIdx = copy.Count - 1;
        for (int i = 0; i < copy.Count; i++)
        {
            int randomIdx = HLE.Random.Int(0, maxIdx);
            (copy[i], copy[randomIdx]) = (copy[randomIdx], copy[i]);
        }

        return copy;
    }

    public static T[] Randomize<T>(this T[] array)
    {
        T[] copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        copy.AsSpan().Randomize();
        return copy;
    }

    internal static void Randomize<T>(this Span<T> span)
    {
        int maxIdx = span.Length - 1;
        for (int i = 0; i < span.Length; i++)
        {
            int randomIdx = HLE.Random.Int(0, maxIdx);
            (span[i], span[randomIdx]) = (span[randomIdx], span[i]);
        }
    }

    public static RangeEnumerator GetEnumerator(this Range range) => new(range);
}
