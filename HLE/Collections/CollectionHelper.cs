using System;
using System.Collections.Generic;
using System.Linq;

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
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        T[] arr = collection.ToArray();
        foreach (T item in arr)
        {
            action(item);
        }

        return arr;
    }

    /// <summary>
    /// Will loop through an <see cref="IEnumerable{T}"/> and performs the given <paramref name="action"/> on each element.<br/>
    /// The <see cref="int"/> parameter of <paramref name="action"/> is the index of the current item in the loop.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="collection">The <see cref="IEnumerable{T}"/> that will be looped through.</param>
    /// <param name="action">The action that will be performed.</param>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
    {
        T[] arr = collection.ToArray();
        for (int i = 0; i < arr.Length; i++)
        {
            action(arr[i], i);
        }

        return arr;
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

    /// <summary>
    /// Return a random element from the <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="collection"/>.</typeparam>
    /// <param name="collection">The collection the random element will be taken from.</param>
    /// <returns>A random element or <see langword="null"/> if the <paramref name="collection"/> doesn't contain any elements.</returns>
    public static T? Random<T>(this IEnumerable<T> collection)
    {
        T[] arr = collection.ToArray();
        return arr.Length == 0 ? default : arr[HLE.Random.Int(0, arr.Length - 1)];
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

    public static IEnumerable<T> Swap<T>(this IEnumerable<T> collection, int idx, int idx2)
    {
        T[] arr = collection.ToArray();
        (arr[idx2], arr[idx]) = (arr[idx], arr[idx2]);
        return arr;
    }

    public static IEnumerable<T> Replace<T>(this IEnumerable<T> collection, Func<T, bool> condition, T replacement)
    {
        T[] arr = collection.ToArray();
        for (int i = 0; i < arr.Length; i++)
        {
            if (condition(arr[i]))
            {
                arr[i] = replacement;
            }
        }

        return arr;
    }

    public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> collection)
    {
        return collection.SelectMany(t => t);
    }

    public static IEnumerable<T[]> Split<T>(this IEnumerable<T> collection, T separator)
    {
        bool IsSeparator(T item) => item?.Equals(separator) == true;

        List<T[]> result = new();
        T[] arr = collection.ToArray();
        int[] idc = arr.IndicesOf(IsSeparator).ToArray();

        int start = 0;
        foreach (int i in idc)
        {
            T[] split = arr[start..i];
            start = i + 1;
            if (split.Length > 0)
            {
                result.Add(split);
            }
        }

        T[] end = arr[(idc[^1] + 1)..];
        if (end.Length > 0)
        {
            result.Add(arr[(idc[^1] + 1)..]);
        }

        return result;
    }

    public static IEnumerable<T> WhereP<T>(this IEnumerable<T> collection, params Func<T, bool>[] predicates)
    {
        return collection.Where(i => predicates.All(p => p(i)));
    }

    public static string RandomString(this IEnumerable<char> collection, int wordLength)
    {
        char[] arr = collection.ToArray();
        Span<char> span = stackalloc char[wordLength];
        for (int i = 0; i < wordLength; i++)
        {
            span[i] = arr.Random();
        }

        return new(span);
    }

    public static IEnumerable<int> IndicesOf<T>(this IEnumerable<T> collection, Func<T, bool> condition)
    {
        T[] arr = collection.ToArray();
        Span<int> indices = stackalloc int[arr.Length];
        int count = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (condition(arr[i]))
            {
                indices[count++] = i;
            }
        }

        return indices[..count].ToArray();
    }

    public static bool ContentEquals<T>(this IEnumerable<T> collection, IEnumerable<T> collection2)
    {
        T[] arr = collection.ToArray();
        T[] arr2 = collection2.ToArray();
        if (arr.Length != arr2.Length)
        {
            return false;
        }

        if (arr.Length == 0 || arr2.Length == 0)
        {
            return true;
        }

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i]?.Equals(arr2[i]) == false)
            {
                return false;
            }
        }

        return true;
    }

    public static IEnumerable<T> ForEachByRange<T>(this IEnumerable<T> collection, params (Range Range, Action<T> Action)[] operations)
    {
        T[] arr = collection.ToArray();
        foreach (var op in operations)
        {
            int start = op.Range.Start.Value;
            int end = op.Range.End.IsFromEnd ? arr.Length - op.Range.End.Value - 1 : op.Range.End.Value;
            for (int i = start; i <= end; i++)
            {
                op.Action(arr[i]);
            }
        }

        return arr;
    }

    public static IEnumerable<T> ForEachByRange<T>(this IEnumerable<T> collection, params (Range Range, Func<T, T> Func)[] operations)
    {
        T[] arr = collection.ToArray();
        foreach (var op in operations)
        {
            int start = op.Range.Start.Value;
            int end = op.Range.End.IsFromEnd ? arr.Length - op.Range.End.Value - 1 : op.Range.End.Value;
            for (int i = start; i <= end; i++)
            {
                arr[i] = op.Func(arr[i]);
            }
        }

        return arr;
    }

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<(TKey Key, TValue Value)> collection) where TKey : notnull
    {
        return collection.ToDictionary(i => i.Key, i => i.Value);
    }

    public static IEnumerable<T> Randomize<T>(this IEnumerable<T> collection)
    {
        T[] arr = collection.ToArray();
        if (arr.Length == 0)
        {
            return arr;
        }

        for (int i = 0; i < arr.Length; i++)
        {
            int randomIdx = HLE.Random.Int(0, arr.Length - 1);
            (arr[i], arr[randomIdx]) = (arr[randomIdx], arr[i]);
        }

        return arr;
    }

    public static RangeEnumerator GetEnumerator(this Range range) => new(range);
}
