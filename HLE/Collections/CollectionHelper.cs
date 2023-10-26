using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Marshalling;
using HLE.Memory;
using HLE.Strings;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

namespace HLE.Collections;

/// <summary>
/// A class to help with any kind of collections.
/// </summary>
public static partial class CollectionHelper
{
    [Pure]
    [SkipLocalsInit]
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
                using RentedArray<char> rentedBuffer = ArrayPool<char>.Shared.RentAsRentedArray(calculatedResultLength);
                charsWritten = StringHelper.Join(separator, chars, rentedBuffer.AsSpan());
                return new(rentedBuffer[..charsWritten]);
            }

            Span<char> buffer = stackalloc char[calculatedResultLength];
            charsWritten = StringHelper.Join(separator, chars, buffer);
            return new(buffer[..charsWritten]);
        }

        return string.Join(separator, collection);
    }

    [Pure]
    [SkipLocalsInit]
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
                using RentedArray<char> rentedBuffer = ArrayPool<char>.Shared.RentAsRentedArray(calculatedResultLength);
                charsWritten = StringHelper.Join(separator, chars, rentedBuffer.AsSpan());
                return new(rentedBuffer[..charsWritten]);
            }

            Span<char> buffer = stackalloc char[calculatedResultLength];
            charsWritten = StringHelper.Join(separator, chars, buffer);
            return new(buffer[..charsWritten]);
        }

        return string.Join(separator, collection);
    }

    [Pure]
    public static string ConcatToString<T>(this IEnumerable<T> collection)
    {
        if (typeof(T) == typeof(char))
        {
            if (collection.TryGetReadOnlySpan(out ReadOnlySpan<char> chars))
            {
                return new(chars);
            }

            IEnumerable<char> charCollection = Unsafe.As<IEnumerable<T>, IEnumerable<char>>(ref collection);
            if (charCollection.TryGetNonEnumeratedCount(out int elementCount))
            {
                if (elementCount == 0)
                {
                    return string.Empty;
                }

                using RentedArray<char> rentedBuffer = ArrayPool<char>.Shared.RentAsRentedArray(elementCount);
                if (charCollection.TryNonEnumeratedCopyTo(rentedBuffer.Array))
                {
                    return new(rentedBuffer.AsSpan());
                }

                using PooledStringBuilder builderWithCapacity = new(elementCount);
                foreach (char c in charCollection)
                {
                    builderWithCapacity.Append(c);
                }

                return builderWithCapacity.ToString();
            }

            using PooledStringBuilder builder = new();
            foreach (char c in charCollection)
            {
                builder.Append(c);
            }

            return builder.ToString();
        }

        if (typeof(T) == typeof(string))
        {
            if (!collection.TryGetReadOnlySpan(out ReadOnlySpan<string> strings))
            {
                IEnumerable<string> stringCollection = Unsafe.As<IEnumerable<T>, IEnumerable<string>>(ref collection);
                if (stringCollection.TryGetNonEnumeratedCount(out int elementCount))
                {
                    using PooledStringBuilder enumerableBuilder = new(15 * elementCount);
                    foreach (string str in stringCollection)
                    {
                        enumerableBuilder.Append(str);
                    }

                    return enumerableBuilder.ToString();
                }
            }

            int stringsLength = strings.Length;
            if (stringsLength == 0)
            {
                return string.Empty;
            }

            int pseudoAverageStringLength = (strings[0].Length + strings[stringsLength >> 1].Length + strings[^1].Length) / 3;
            using PooledStringBuilder builder = new(pseudoAverageStringLength * stringsLength);

            ref string stringsReference = ref MemoryMarshal.GetReference(strings);
            for (int i = 0; i < stringsLength; i++)
            {
                string str = Unsafe.Add(ref stringsReference, i);
                builder.Append(str);
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
        List<T> result = new(list.Count);
        CopyWorker<T> copyWorker = new(list);
        copyWorker.CopyTo(result);
        Replace(CollectionsMarshal.AsSpan(result), predicate, replacement);
        return result;
    }

    [Pure]
    public static T[] Replace<T>(this T[] array, Func<T, bool> predicate, T replacement)
    {
        T[] copy = GC.AllocateUninitializedArray<T>(array.Length);
        CopyWorker<T>.Copy(array, copy);
        Replace(copy.AsSpan(), predicate, replacement);
        return copy;
    }

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

    [Pure]
    public static unsafe T[] Replace<T>(this IEnumerable<T> collection, delegate*<T, bool> predicate, T replacement)
    {
        T[] array = collection.ToArray();
        Replace(array.AsSpan(), predicate, replacement);
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
        T[] copy = GC.AllocateUninitializedArray<T>(array.Length);
        CopyWorker<T>.Copy(array, copy);
        Replace(copy.AsSpan(), predicate, replacement);
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
    public static RangeEnumerator GetEnumerator(this Range range) => new(range);

    public static void FillAscending(this int[] array, int start = 0) => FillAscending(array.AsSpan(), start);

    public static void FillAscending(this Span<int> span, int start = 0)
    {
        int vector512Count = Vector512<int>.Count;
        if (Vector512.IsHardwareAccelerated && span.Length >= vector512Count)
        {
            Vector512<int> ascendingValueAdditions = Vector512.Create(
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15
            );
            while (span.Length >= vector512Count)
            {
                Vector512<int> startValues = Vector512.Create(start);
                Vector512<int> values = Vector512.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector512Count;
                span = span.SliceUnsafe(vector512Count);
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + i;
            }

            return;
        }

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
                span = span.SliceUnsafe(vector256Count);
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
                span = span.SliceUnsafe(vector64Count);
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

    public static void FillAscending(this char[] array, char start = '\0') => FillAscending(array.AsSpan(), start);

    public static void FillAscending(this Span<char> span, char start = '\0') => FillAscending(MemoryMarshal.Cast<char, ushort>(span), start);

    public static void FillAscending(this ushort[] array, ushort start = 0) => FillAscending(array.AsSpan(), start);

    public static void FillAscending(this Span<ushort> span, ushort start = 0)
    {
        ushort vector512Count = (ushort)Vector512<ushort>.Count;
        if (Vector512.IsHardwareAccelerated && span.Length >= vector512Count)
        {
            Vector512<ushort> ascendingValueAdditions = Vector512.Create(
                (ushort)0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
                30, 31
            );

            while (span.Length >= vector512Count)
            {
                Vector512<ushort> startValues = Vector512.Create(start);
                Vector512<ushort> values = Vector512.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector512Count;
                span = span.SliceUnsafe(vector512Count);
            }

            for (ushort i = 0; i < span.Length; i++)
            {
                span[i] = (ushort)(start + i);
            }

            return;
        }

        ushort vector256Count = (ushort)Vector256<ushort>.Count;
        if (Vector256.IsHardwareAccelerated && span.Length >= vector256Count)
        {
            Vector256<ushort> ascendingValueAdditions = Vector256.Create(
                (ushort)0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 11, 12, 13, 14, 15
            );

            while (span.Length >= vector256Count)
            {
                Vector256<ushort> startValues = Vector256.Create(start);
                Vector256<ushort> values = Vector256.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += vector256Count;
                span = span.SliceUnsafe(vector256Count);
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
                span = span.SliceUnsafe(vector64Count);
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

        span = ReadOnlySpan<T>.Empty;
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
                span = Span<T>.Empty;
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
            memory = SpanMarshal.AsMemoryUnsafe(CollectionsMarshal.AsSpan(list));
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

    /// <summary>
    /// Computes the sum of all elements without checking for arithmetic overflow.
    /// </summary>
    /// <param name="collection">The elements that will be summed up.</param>
    /// <returns>The sum of all elements.</returns>
    public static T SumUnchecked<T>(this IEnumerable<T> collection) where T : INumber<T>
    {
        if (collection.TryGetReadOnlySpan<T>(out ReadOnlySpan<T> span))
        {
            return span.SumUnchecked();
        }

        T sum = T.Zero;
        if (collection.TryGetNonEnumeratedCount(out int elementCount))
        {
            switch (collection)
            {
                case IIndexAccessible<T> indexAccessible:
                {
                    for (int i = 0; i < elementCount; i++)
                    {
                        sum += indexAccessible[i];
                    }

                    return sum;
                }
                case IReadOnlyList<T> readOnlyList:
                {
                    for (int i = 0; i < elementCount; i++)
                    {
                        sum += readOnlyList[i];
                    }

                    return sum;
                }
                case IList<T> list:
                {
                    for (int i = 0; i < elementCount; i++)
                    {
                        sum += list[i];
                    }

                    return sum;
                }
            }
        }

        foreach (T item in collection)
        {
            sum += item;
        }

        return sum;
    }

    /// <summary>
    /// Computes the sum of all elements without checking for arithmetic overflow.
    /// </summary>
    /// <param name="list">The elements that will be summed up.</param>
    /// <returns>The sum of all elements.</returns>
    [Pure]
    public static T SumUnchecked<T>(this List<T> list) where T : INumber<T>
        => SumUnchecked(CollectionsMarshal.AsSpan(list));

    /// <summary>
    /// Computes the sum of all elements without checking for arithmetic overflow.
    /// </summary>
    /// <param name="array">The elements that will be summed up.</param>
    /// <returns>The sum of all elements.</returns>
    [Pure]
    public static T SumUnchecked<T>(this T[] array) where T : INumber<T>
        => SumUnchecked(array.AsSpan());

    /// <inheritdoc cref="SumUnchecked{T}(System.ReadOnlySpan{T})"/>
    [Pure]
    public static T SumUnchecked<T>(this Span<T> span) where T : INumber<T>
        => SumUnchecked((ReadOnlySpan<T>)span);

    /// <summary>
    /// Computes the sum of all elements without checking for arithmetic overflow.
    /// </summary>
    /// <param name="span">The elements that will be summed up.</param>
    /// <returns>The sum of all elements.</returns>
    [Pure]
    public static T SumUnchecked<T>(this ReadOnlySpan<T> span) where T : INumber<T>
    {
        switch (span.Length)
        {
            case 0:
                return T.Zero;
            case 1:
                return span[0];
            case 2:
                return span[0] + span[1];
        }

        T sum = T.Zero;
        int vector512Count = Vector512<T>.Count;
        if (Vector512.IsHardwareAccelerated && span.Length >= vector512Count)
        {
            Vector512<T> vectorSum = Vector512<T>.Zero;
            while (span.Length >= vector512Count)
            {
                ref T valuesReference = ref MemoryMarshal.GetReference(span);
                Vector512<T> vector = Vector512.LoadUnsafe(ref valuesReference);
                vectorSum += vector;
                span = span.SliceUnsafe(vector512Count);
            }

            sum = Vector512.Sum(vectorSum);
            for (int i = 0; i < span.Length; i++)
            {
                sum += span[i];
            }

            return sum;
        }

        int vector256Count = Vector256<T>.Count;
        if (Vector256.IsHardwareAccelerated && span.Length >= vector256Count)
        {
            Vector256<T> vectorSum = Vector256<T>.Zero;
            while (span.Length >= vector256Count)
            {
                ref T valuesReference = ref MemoryMarshal.GetReference(span);
                vectorSum += Vector256.LoadUnsafe(ref valuesReference);
                span = span.SliceUnsafe(vector256Count);
            }

            sum = Vector256.Sum(vectorSum);
            for (int i = 0; i < span.Length; i++)
            {
                sum += span[i];
            }

            return sum;
        }

        int vector128Count = Vector128<T>.Count;
        if (Vector128.IsHardwareAccelerated && span.Length >= vector128Count)
        {
            Vector128<T> vectorSum = Vector128<T>.Zero;
            while (span.Length >= vector128Count)
            {
                ref T valuesReference = ref MemoryMarshal.GetReference(span);
                vectorSum += Vector128.LoadUnsafe(ref valuesReference);
                span = span.SliceUnsafe(vector128Count);
            }

            sum = Vector128.Sum(vectorSum);
            for (int i = 0; i < span.Length; i++)
            {
                sum += span[i];
            }

            return sum;
        }

        int vector64Count = Vector64<T>.Count;
        if (Vector64.IsHardwareAccelerated && span.Length >= vector64Count)
        {
            Vector64<T> vectorSum = Vector64<T>.Zero;
            while (span.Length >= vector64Count)
            {
                ref T valuesReference = ref MemoryMarshal.GetReference(span);
                vectorSum += Vector64.LoadUnsafe(ref valuesReference);
                span = span.SliceUnsafe(vector64Count);
            }

            sum = Vector64.Sum(vectorSum);
            for (int i = 0; i < span.Length; i++)
            {
                sum += span[i];
            }

            return sum;
        }

        for (int i = 0; i < span.Length; i++)
        {
            sum += span[i];
        }

        return sum;
    }
}
