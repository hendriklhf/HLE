using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;
using HLE.Memory;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;
#if RELEASE
using System.Linq;
#endif

namespace HLE.Collections;

/// <summary>
/// A class to help with any kind of collections.
/// </summary>
public static partial class CollectionHelpers
{
    [Pure]
    [LinqTunnel]
    public static ReplaceEnumerable<T> Replace<T>([NoEnumeration] this IEnumerable<T> enumerable, Func<T, bool> predicate, T replacement)
        => new(enumerable, predicate, replacement);

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
    public static bool TryGetReadOnlySpan<T>([NoEnumeration] this IEnumerable<T> enumerable, out ReadOnlySpan<T> span)
    {
        if (typeof(T) == typeof(char) && enumerable is string str)
        {
            ref char firstChar = ref StringMarshal.GetReference(str);
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<char, T>(ref firstChar), str.Length);
            return true;
        }

        if (TryGetSpan(enumerable, out Span<T> mutableSpan))
        {
            span = mutableSpan;
            return true;
        }

        switch (enumerable)
        {
            case ImmutableArray<T> immutableArray:
                span = immutableArray.AsSpan();
                return true;
            case IReadOnlySpanProvider<T> spanProvider:
                span = spanProvider.GetReadOnlySpan();
                return true;
            case FrozenSet<T> frozenSet:
                span = frozenSet.Items.AsSpan();
                return true;
            default:
                span = [];
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetSpan<T>([NoEnumeration] this IEnumerable<T> enumerable, out Span<T> span)
    {
        switch (enumerable)
        {
            case T[] array:
                span = array;
                return true;
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
    public static bool TryGetReadOnlyMemory<T>([NoEnumeration] this IEnumerable<T> enumerable, out ReadOnlyMemory<T> memory)
    {
        if (typeof(T) == typeof(char) && enumerable is string str)
        {
            ReadOnlyMemory<char> stringMemory = str.AsMemory();
            memory = Unsafe.As<ReadOnlyMemory<char>, ReadOnlyMemory<T>>(ref stringMemory);
            return true;
        }

        if (TryGetMemory(enumerable, out Memory<T> mutableMemory))
        {
            memory = mutableMemory;
            return true;
        }

        switch (enumerable)
        {
            case ImmutableArray<T> immutableArray:
                memory = ImmutableCollectionsMarshal.AsArray(immutableArray);
                return true;
            case IReadOnlyMemoryProvider<T> readOnlyMemoryProvider:
                memory = readOnlyMemoryProvider.GetReadOnlyMemory();
                return true;
            case FrozenSet<T> frozenSet:
                memory = frozenSet.Items.AsMemory();
                return true;
        }

        memory = ReadOnlyMemory<T>.Empty;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetMemory<T>([NoEnumeration] this IEnumerable<T> enumerable, out Memory<T> memory)
    {
        switch (enumerable)
        {
            case T[] array:
                memory = array;
                return true;
            case List<T> list:
                memory = ListMarshal.AsMemory(list);
                return true;
            case IMemoryProvider<T> memoryProvider:
                memory = memoryProvider.GetMemory();
                return true;
            default:
                memory = Memory<T>.Empty;
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNonEnumeratedCount<T>([NoEnumeration] this IEnumerable<T> enumerable, out int elementCount)
    {
#if RELEASE // this will prevent reaching the bottom branches, so it will be removed for test runs as it is runtime code
        if (Enumerable.TryGetNonEnumeratedCount(enumerable, out elementCount))
        {
            return true;
        }
#endif

        if (enumerable.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            elementCount = span.Length;
            return true;
        }

        switch (enumerable)
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
    /// <param name="enumerable">The collection of items that will be tried to be copied to the destination.</param>
    /// <param name="destination">The destination of the copied items.</param>
    /// <param name="offset">The offset to the destination start.</param>
    /// <param name="elementsCopied">The amount of elements that have been copied.</param>
    /// <typeparam name="T">The type of items that will be tried to be copied.</typeparam>
    /// <returns>True, if copying was possible, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryNonEnumeratedCopyTo<T>([NoEnumeration] this IEnumerable<T> enumerable, T[] destination, int offset, out int elementsCopied)
    {
        if (enumerable.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            if (span.Length == 0)
            {
                elementsCopied = 0;
                return true;
            }

            span.CopyTo(destination.AsSpan(offset));
            elementsCopied = span.Length;
            return true;
        }

        switch (enumerable)
        {
            case ICollection<T> collection:
                if (collection.Count == 0)
                {
                    elementsCopied = 0;
                    return true;
                }

                collection.CopyTo(destination, offset);
                elementsCopied = collection.Count;
                return true;
            case ICopyable<T> copyable:
                copyable.CopyTo(destination, offset);
                elementsCopied = copyable.Count;
                return true;
            default:
                elementsCopied = 0;
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNonEnumeratedElementAt<T>([NoEnumeration] this IEnumerable<T> enumerable, int index, [MaybeNullWhen(false)] out T element)
    {
        if (enumerable.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            element = span[index];
            return true;
        }

        switch (enumerable)
        {
            case IList<T> list: // TODO: IList<T> might implement IReadOnlyList<T> in .NET 9 so this can be deleted maybe
                element = list[index];
                return true;
            case IReadOnlyList<T> readOnlyList:
                element = readOnlyList[index];
                return true;
            case IIndexable<T> indexAccessible:
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
    /// <typeparam name="T">The element type of the <see cref="Span{T}"/>.</typeparam>
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
    /// If the amount of elements in <paramref name="enumerable"/> can be found out, the method will check if there is enough space in the buffer.
    /// If there isn't enough space, the method will return <see langword="false"/> and set <paramref name="writtenElements"/> to <c>0</c>.<br/>
    /// If no amount of elements could be retrieved, the method will start writing elements into the buffer and will do so until it is finished, in which case it will return <see langword="true"/>,
    /// or until it runs out of buffer space, in which case it will return <see langword="false"/>.<br/>
    /// </summary>
    /// <remarks>This method is in some cases much more efficient than calling <c>.ToArray()</c> on an <see cref="IEnumerable{T}"/> and enables using a rented <see cref="Array"/> from an <see cref="ArrayPool{T}"/> to store the elements.</remarks>
    /// <param name="enumerable">The <see cref="IEnumerable{T}"/> that will be enumerated and elements will be taken from.</param>
    /// <param name="destination">The buffer the elements will be written into.</param>
    /// <param name="writtenElements">The amount of written elements.</param>
    /// <typeparam name="T">The type of elements in the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <returns>True, if a full enumeration into the buffer was possible, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryEnumerateInto<T>(this IEnumerable<T> enumerable, Span<T> destination, out int writtenElements)
    {
        if (enumerable.TryGetNonEnumeratedCount(out int elementCount))
        {
            if (elementCount > destination.Length)
            {
                writtenElements = 0;
                return false;
            }

            if (enumerable.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
            {
                span.CopyTo(destination);
                writtenElements = span.Length;
                return true;
            }

            switch (enumerable)
            {
                case ICopyable<T> copyable:
                    copyable.CopyTo(destination);
                    writtenElements = elementCount;
                    return true;
                case IIndexable<T> indexAccessible:
                    for (int i = 0; i < elementCount; i++)
                    {
                        destination[i] = indexAccessible[i];
                    }

                    break;
            }
        }

        writtenElements = 0;
        foreach (T item in enumerable)
        {
            if (writtenElements >= destination.Length)
            {
                return false;
            }

            destination[writtenElements++] = item;
        }

        return true;
    }

    [Pure]
    [MustDisposeResource]
    public static PooledList<T> ToPooledList<T>(this IEnumerable<T> enumerable) => [.. enumerable];

    [Pure]
    [MustDisposeResource]
    public static PooledBufferWriter<T> ToPooledBufferWriter<T>(this IEnumerable<T> enumerable)
    {
        PooledBufferWriter<T> writer = new();
        writer.Write(enumerable);
        return writer;
    }

    [Pure]
    [MustDisposeResource]
    public static RentedArray<T> ToRentedArray<T>(this IEnumerable<T> enumerable)
    {
        T[] array;
        if (enumerable.TryGetReadOnlySpan(out ReadOnlySpan<T> span))
        {
            array = ArrayPool<T>.Shared.RentExact(span.Length);
            SpanHelpers<T>.Copy(span, array);
            return new(array, ArrayPool<T>.Shared);
        }

        switch (enumerable)
        {
            case ICopyable<T> copyable:
                array = ArrayPool<T>.Shared.RentExact(copyable.Count);
                copyable.CopyTo(array);
                return new(array, ArrayPool<T>.Shared);
            case ICollection<T> collection:
                array = ArrayPool<T>.Shared.RentExact(collection.Count);
                collection.CopyTo(array, 0);
                return new(array, ArrayPool<T>.Shared);
        }

        using PooledList<T> list = enumerable.ToPooledList();
        array = ArrayPool<T>.Shared.RentExact(list.Count);
        SpanHelpers<T>.Copy(list.AsSpan(), array);
        return new(array, ArrayPool<T>.Shared);
    }
}
