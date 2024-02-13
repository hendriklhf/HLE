using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Collections;
using HLE.Marshalling;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    [Pure]
    public static int[] IndicesOf<T>(this List<T> list, T item) where T : IEquatable<T>
        => IndicesOf(CollectionsMarshal.AsSpan(list), item);

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, T item) where T : IEquatable<T>
        => IndicesOf(array.AsSpan(), item);

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> span, T item) where T : IEquatable<T>
        => IndicesOf((ReadOnlySpan<T>)span, item);

    [Pure]
    [SkipLocalsInit]
    public static int[] IndicesOf<T>(this ReadOnlySpan<T> span, T item) where T : IEquatable<T>
    {
        if (span.Length == 0)
        {
            return [];
        }

        int length;
        if (!MemoryHelpers.UseStackalloc<int>(span.Length))
        {
            using RentedArray<int> indicesBuffer = ArrayPool<int>.Shared.RentAsRentedArray(span.Length);
            length = IndicesOf(span, item, indicesBuffer.AsSpan());
            return indicesBuffer.ToArray(..length);
        }

        Span<int> indices = stackalloc int[span.Length];
        length = IndicesOf(span, item, indices);
        return indices.ToArray(..length);
    }

    public static int IndicesOf<T>(this Span<T> span, T item, Span<int> destination) where T : IEquatable<T>
        => IndicesOf((ReadOnlySpan<T>)span, item, destination);

    public static unsafe int IndicesOf<T>(this ReadOnlySpan<T> span, T item, Span<int> destination) where T : IEquatable<T>
    {
        if (!StructMarshal.IsBitwiseEquatable<T>())
        {
            return IndicesOfNonOptimizedFallback(span, item, destination);
        }

        if (span.Length == 0)
        {
            return 0;
        }

        if (destination.Length < span.Length)
        {
            ThrowDestinationTooShort<T>();
        }

        ref T reference = ref MemoryMarshal.GetReference(span);
        ref int destinationRef = ref MemoryMarshal.GetReference(destination);
        return sizeof(T) switch
        {
            sizeof(byte) => IndicesOf(ref Unsafe.As<T, byte>(ref reference), span.Length, Unsafe.As<T, byte>(ref item), ref destinationRef),
            sizeof(ushort) => IndicesOf(ref Unsafe.As<T, ushort>(ref reference), span.Length, Unsafe.As<T, ushort>(ref item), ref destinationRef),
            sizeof(uint) => IndicesOf(ref Unsafe.As<T, uint>(ref reference), span.Length, Unsafe.As<T, uint>(ref item), ref destinationRef),
            sizeof(ulong) => IndicesOf(ref Unsafe.As<T, ulong>(ref reference), span.Length, Unsafe.As<T, ulong>(ref item), ref destinationRef),
            _ => IndicesOfNonOptimizedFallback(span, item, destination)
        };
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDestinationTooShort<T>()
        => throw new InvalidOperationException($"The destination needs to be at least as long as the {nameof(Span<T>)} of items provided.");

    public static int IndicesOf<T>(ref T items, int length, T item, ref int destination) where T : unmanaged, IEquatable<T>
    {
        Debug.Assert(Vector<T>.IsSupported, "Support of the generic type has to be ensured before calling this method.");

        int indicesLength = 0;
        int startIndex = 0;
        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> searchVector = Vector512.Create(item);
            while (length - startIndex >= Vector512<T>.Count)
            {
                Vector512<T> itemsVector = Vector512.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                ulong equals = Vector512.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals != 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    Unsafe.Add(ref destination, indicesLength++) = startIndex + index;
                    equals ^= (1U << index);
                }

                startIndex += Vector512<T>.Count;
            }

            goto RemainingItemsLoop;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> searchVector = Vector256.Create(item);
            while (length - startIndex >= Vector256<T>.Count)
            {
                Vector256<T> itemsVector = Vector256.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                uint equals = Vector256.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals != 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    Unsafe.Add(ref destination, indicesLength++) = startIndex + index;
                    equals ^= (1U << index);
                }

                startIndex += Vector256<T>.Count;
            }

            goto RemainingItemsLoop;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> searchVector = Vector128.Create(item);
            while (length - startIndex >= Vector128<T>.Count)
            {
                Vector128<T> itemsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                uint equals = Vector128.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals != 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    Unsafe.Add(ref destination, indicesLength++) = startIndex + index;
                    equals ^= (1U << index);
                }

                startIndex += Vector128<T>.Count;
            }

            goto RemainingItemsLoop;
        }

        return IndicesOfNonOptimizedFallback(MemoryMarshal.CreateReadOnlySpan(ref items, length), item, MemoryMarshal.CreateSpan(ref destination, length));

        RemainingItemsLoop:
        ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
        int remainingLength = length - startIndex;
        for (int i = 0; i < remainingLength; i++)
        {
            if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
            {
                Unsafe.Add(ref destination, indicesLength++) = startIndex + i;
            }
        }

        return indicesLength;
    }

    private static int IndicesOfNonOptimizedFallback<T>(ReadOnlySpan<T> span, T item, Span<int> destination) where T : IEquatable<T>
    {
        if (span.Length == 0)
        {
            return 0;
        }

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
