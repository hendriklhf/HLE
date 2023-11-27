using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

/// <summary>
/// A class to help with any kind of collections.
/// </summary>
public static partial class CollectionHelpers
{
    [Pure]
    [LinqTunnel]
    public static IEnumerable<T> Replace<T>([NoEnumeration] this IEnumerable<T> collection, Func<T, bool> predicate, T replacement)
    {
        foreach (T item in collection)
        {
            yield return predicate(item) ? replacement : item; // TODO: create own enumerator
        }
    }

    public static void Replace<T>(this List<T> list, Func<T, bool> predicate, T replacement)
        => Replace(CollectionsMarshal.AsSpan(list), predicate, replacement);

    public static void Replace<T>(this T[] array, Func<T, bool> predicate, T replacement) => Replace(array.AsSpan(), predicate, replacement);

    public static void Replace<T>(this Span<T> span, Func<T, bool> predicate, T replacement)
    {
        int spanLength = span.Length;
        if (spanLength == 0)
        {
            return;
        }

        ref T spanReference = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            ref T item = ref Unsafe.Add(ref spanReference, i);
            if (predicate(item))
            {
                item = replacement;
            }
        }
    }

    public static unsafe void Replace<T>(this List<T> list, delegate*<T, bool> predicate, T replacement)
        => Replace(CollectionsMarshal.AsSpan(list), predicate, replacement);

    public static unsafe void Replace<T>(this T[] array, delegate*<T, bool> predicate, T replacement)
        => Replace(array.AsSpan(), predicate, replacement);

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
    public static RangeEnumerator GetEnumerator(this Range range) => new(range);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
        => CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out _) = value;

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
        if (typeof(T) == typeof(char) && collection is string str)
        {
            ref char firstChar = ref MemoryMarshal.GetReference(str.AsSpan());
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<char, T>(ref firstChar), str.Length);
            return true;
        }

        if (TryGetSpan<T>(collection, out Span<T> mutableSpan))
        {
            span = mutableSpan;
            return true;
        }

        if (collection is IReadOnlySpanProvider<T> spanProvider)
        {
            span = spanProvider.GetReadOnlySpan();
            return true;
        }

        span = [];
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryGetReadOnlySpan<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out ReadOnlySpan<TTo> span)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetReadOnlySpan<TTo>(resultCollection, out span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetSpan<T>([NoEnumeration] this IEnumerable<T> collection, out Span<T> span)
    {
        if (collection.GetType() == typeof(T[]))
        {
            span = Unsafe.As<IEnumerable<T>, T[]>(ref collection);
            return true;
        }

        switch (collection)
        {
            case List<T> list:
                span = CollectionsMarshal.AsSpan(list);
                return true;
            case ISpanProvider<T> spanProvider:
                span = spanProvider.GetSpan();
                return true;
            default:
                span = [];
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryGetSpan<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out Span<TTo> span)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetSpan<TTo>(resultCollection, out span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetReadOnlyMemory<T>([NoEnumeration] this IEnumerable<T> collection, out ReadOnlyMemory<T> memory)
    {
        if (typeof(T) == typeof(char) && collection is string str)
        {
            ReadOnlyMemory<char> stringMemory = str.AsMemory();
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
    internal static bool TryGetReadOnlyMemory<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out ReadOnlyMemory<TTo> memory)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetReadOnlyMemory<TTo>(resultCollection, out memory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetMemory<T>([NoEnumeration] this IEnumerable<T> collection, out Memory<T> memory)
    {
        if (collection.GetType() == typeof(T[]))
        {
            memory = Unsafe.As<IEnumerable<T>, T[]>(ref collection);
            return true;
        }

        if (collection is List<T> list)
        {
            memory = SpanMarshal.AsMemory(CollectionsMarshal.AsSpan(list));
            return true;
        }

        memory = Memory<T>.Empty;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryGetMemory<TFrom, TTo>([NoEnumeration] this IEnumerable<TFrom> collection, out Memory<TTo> memory)
    {
        IEnumerable<TTo> resultCollection = Unsafe.As<IEnumerable<TFrom>, IEnumerable<TTo>>(ref collection);
        return TryGetMemory<TTo>(resultCollection, out memory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNonEnumeratedCount<T>([NoEnumeration] this IEnumerable<T> collection, out int elementCount)
    {
        if (Enumerable.TryGetNonEnumeratedCount(collection, out elementCount))
        {
            return true;
        }

        if (collection.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            elementCount = span.Length;
            return true;
        }

        switch (collection)
        {
            case ICountable countable:
                elementCount = countable.Count;
                return true;
            case IReadOnlyCollection<T> readOnlyCollection:
                elementCount = readOnlyCollection.Count;
                return true;
            default:
                elementCount = 0;
                return false;
        }
    }

    /// <summary>
    /// Tries to copy the elements of the <see cref="IEnumerable{T}"/> to the <paramref name="destination"/> at a given offset,
    /// while not enumerating the enumerable.
    /// </summary>
    /// <param name="collection">The collection of items that will be tried to be copied to the destination.</param>
    /// <param name="destination">The destination of the copied items.</param>
    /// <param name="offset">The offset to the destination start.</param>
    /// <typeparam name="T">The type of items that will be tried to be copied.</typeparam>
    /// <returns>True, if copying was possible, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryNonEnumeratedCopyTo<T>([NoEnumeration] this IEnumerable<T> collection, T[] destination, int offset = 0)
    {
        if (collection.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            if (span.Length == 0)
            {
                return true;
            }

            span.CopyTo(destination.AsSpan(offset));
            return true;
        }

        switch (collection)
        {
            case ICollection<T> iCollection:
                if (iCollection.Count == 0)
                {
                    return true;
                }

                iCollection.CopyTo(destination, offset);
                return true;
            case ICopyable<T> copyable:
                copyable.CopyTo(destination, offset);
                return true;
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryNonEnumeratedElementAt<T>([NoEnumeration] this IEnumerable<T> collection, int index, [MaybeNullWhen(false)] out T element)
    {
        if (collection.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            element = span[index];
            return true;
        }

        switch (collection)
        {
            case IList<T> list:
                element = list[index];
                return true;
            case IReadOnlyList<T> readOnlyList:
                element = readOnlyList[index];
                return true;
            case IIndexAccessible<T> indexAccessible:
                element = indexAccessible[index];
                return true;
            default:
                element = default;
                return false;
        }
    }

    /// <inheritdoc cref="MoveItem{T}(Span{T},int,int)"/>
    public static void MoveItem<T>(this List<T> list, int sourceIndex, int destinationIndex)
        => MoveItem(CollectionsMarshal.AsSpan(list), sourceIndex, destinationIndex);

    /// <inheritdoc cref="MoveItem{T}(Span{T},int,int)"/>
    public static void MoveItem<T>(this T[] array, int sourceIndex, int destinationIndex)
        => MoveItem(array.AsSpan(), sourceIndex, destinationIndex);

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

        bool areSourceAndDestinationNextToEachOther = Math.Abs(sourceIndex - destinationIndex) == 1;
        if (areSourceAndDestinationNextToEachOther)
        {
            (span[sourceIndex], span[destinationIndex]) = (span[destinationIndex], span[sourceIndex]);
            return;
        }

        T value = span[sourceIndex];
        bool isSourceRightOfDestination = sourceIndex > destinationIndex;
        if (isSourceRightOfDestination)
        {
            span[destinationIndex..sourceIndex].CopyTo(span[(destinationIndex + 1)..]);
        }
        else
        {
            span[(sourceIndex + 1)..(destinationIndex + 1)].CopyTo(span[sourceIndex..]);
        }

        span[destinationIndex] = value;
    }

    /// <summary>
    /// Tries to enumerate a <see cref="IEnumerable{T}"/> and write the elements into a buffer.<br/>
    /// If the amount of elements in <paramref name="collection"/> can be found out, the method will check if there is enough space in the buffer.
    /// If there isn't enough space, the method will return <see langword="false"/> and set <paramref name="writtenElements"/> to <c>0</c>.<br/>
    /// If no amount of elements could be retrieved, the method will start writing elements into the buffer and will do so until it is finished, in which case it will return <see langword="true"/>,
    /// or until it runs out of buffer space, in which case it will return <see langword="false"/>.<br/>
    /// </summary>
    /// <remarks>This method is in some cases much more efficient than calling <c>.ToArray()</c> on an <see cref="IEnumerable{T}"/> and enables using a rented <see cref="Array"/> from an <see cref="ArrayPool{T}"/> to store the elements.</remarks>
    /// <param name="collection">The <see cref="IEnumerable{T}"/> that will be enumerated and elements will be taken from.</param>
    /// <param name="destination">The buffer the elements will be written into.</param>
    /// <param name="writtenElements">The amount of written elements.</param>
    /// <typeparam name="T">The type of elements in the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <returns>True, if a full enumeration into the buffer was possible, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryEnumerateInto<T>(this IEnumerable<T> collection, Span<T> destination, out int writtenElements)
    {
        if (collection.TryGetNonEnumeratedCount(out int elementCount))
        {
            if (elementCount > destination.Length)
            {
                writtenElements = 0;
                return false;
            }

            if (collection.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
            {
                span.CopyTo(destination);
                writtenElements = span.Length;
                return true;
            }

            switch (collection)
            {
                case ICopyable<T> copyable:
                    copyable.CopyTo(destination);
                    writtenElements = elementCount;
                    return true;
                case IIndexAccessible<T> indexAccessible:
                {
                    for (int i = 0; i < elementCount; i++)
                    {
                        destination[i] = indexAccessible[i];
                    }

                    break;
                }
            }
        }

        writtenElements = 0;
        foreach (T item in collection)
        {
            if (writtenElements >= destination.Length)
            {
                return false;
            }

            destination[writtenElements++] = item;
        }

        return true;
    }
}
