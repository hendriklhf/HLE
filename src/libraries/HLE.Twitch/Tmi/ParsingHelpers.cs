using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Collections;

namespace HLE.Twitch.Tmi;

internal static class ParsingHelpers
{
    public static unsafe int IndicesOf<T>(ReadOnlySpan<T> span, T item, Span<int> destination, int maximumAmountOfIndicesNeeded)
        where T : unmanaged, IEquatable<T>
    {
        if (span.Length == 0)
        {
            return 0;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return IndicesOfNonOptimizedFallback(span, item, destination, maximumAmountOfIndicesNeeded);
        }

        ref T reference = ref MemoryMarshal.GetReference(span);
        return sizeof(T) switch
        {
            sizeof(byte) => IndicesOf(ref Unsafe.As<T, byte>(ref reference), span.Length, Unsafe.BitCast<T, byte>(item), destination,
                maximumAmountOfIndicesNeeded),
            sizeof(ushort) => IndicesOf(ref Unsafe.As<T, ushort>(ref reference), span.Length, Unsafe.BitCast<T, ushort>(item), destination,
                maximumAmountOfIndicesNeeded),
            sizeof(uint) => IndicesOf(ref Unsafe.As<T, uint>(ref reference), span.Length, Unsafe.BitCast<T, uint>(item), destination,
                maximumAmountOfIndicesNeeded),
            sizeof(ulong) => IndicesOf(ref Unsafe.As<T, ulong>(ref reference), span.Length, Unsafe.BitCast<T, ulong>(item), destination,
                maximumAmountOfIndicesNeeded),
            _ => IndicesOfNonOptimizedFallback(span, item, destination, maximumAmountOfIndicesNeeded)
        };
    }

    private static int IndicesOf<T>(ref T items, int length, T item, Span<int> destination, int maximumAmountOfIndicesNeeded)
        where T : unmanaged, IEquatable<T>
    {
        int indicesLength = 0;
        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> searchVector = Vector512.Create(item);
            int startIndex = 0;
            while (length - startIndex >= Vector512<T>.Count)
            {
                Vector512<T> itemsVector = Vector512.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                ulong equals = Vector512.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals != 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    destination[indicesLength++] = startIndex + index;
                    if (indicesLength == maximumAmountOfIndicesNeeded)
                    {
                        return maximumAmountOfIndicesNeeded;
                    }

                    equals &= equals - 1;
                }

                startIndex += Vector512<T>.Count;
            }

            ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
            int remainingLength = length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
                {
                    destination[indicesLength++] = startIndex + i;
                    if (indicesLength == maximumAmountOfIndicesNeeded)
                    {
                        return maximumAmountOfIndicesNeeded;
                    }
                }
            }

            return indicesLength;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> searchVector = Vector256.Create(item);
            int startIndex = 0;
            while (length - startIndex >= Vector256<T>.Count)
            {
                Vector256<T> itemsVector = Vector256.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                uint equals = Vector256.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals != 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    destination[indicesLength++] = startIndex + index;
                    if (indicesLength == maximumAmountOfIndicesNeeded)
                    {
                        return maximumAmountOfIndicesNeeded;
                    }

                    equals &= equals - 1;
                }

                startIndex += Vector256<T>.Count;
            }

            ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
            int remainingLength = length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
                {
                    destination[indicesLength++] = startIndex + i;
                    if (indicesLength == maximumAmountOfIndicesNeeded)
                    {
                        return maximumAmountOfIndicesNeeded;
                    }
                }
            }

            return indicesLength;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> searchVector = Vector128.Create(item);
            int startIndex = 0;
            while (length - startIndex >= Vector128<T>.Count)
            {
                Vector128<T> itemsVector = Vector128.LoadUnsafe(ref Unsafe.Add(ref items, startIndex));
                uint equals = Vector128.Equals(itemsVector, searchVector).ExtractMostSignificantBits();
                while (equals != 0)
                {
                    int index = BitOperations.TrailingZeroCount(equals);
                    destination[indicesLength++] = startIndex + index;
                    if (indicesLength == maximumAmountOfIndicesNeeded)
                    {
                        return maximumAmountOfIndicesNeeded;
                    }

                    equals &= equals - 1;
                }

                startIndex += Vector128<T>.Count;
            }

            ref T remainingItemsReference = ref Unsafe.Add(ref items, startIndex);
            int remainingLength = length - startIndex;
            for (int i = 0; i < remainingLength; i++)
            {
                if (item.Equals(Unsafe.Add(ref remainingItemsReference, i)))
                {
                    destination[indicesLength++] = startIndex + i;
                    if (indicesLength == maximumAmountOfIndicesNeeded)
                    {
                        return maximumAmountOfIndicesNeeded;
                    }
                }
            }

            return indicesLength;
        }

        return IndicesOfNonOptimizedFallback(MemoryMarshal.CreateReadOnlySpan(ref items, length), item, destination, maximumAmountOfIndicesNeeded);
    }

    private static int IndicesOfNonOptimizedFallback<T>(ReadOnlySpan<T> span, T item, Span<int> destination, int maximumAmountOfIndicesNeeded)
        where T : IEquatable<T>
    {
        int indicesLength = 0;
        int indexOfItem = span.IndexOf(item);
        int spanStartIndex = indexOfItem;
        while (indexOfItem >= 0)
        {
            destination[indicesLength++] = spanStartIndex;
            if (indicesLength == maximumAmountOfIndicesNeeded)
            {
                return maximumAmountOfIndicesNeeded;
            }

            indexOfItem = span.SliceUnsafe(++spanStartIndex).IndexOf(item);
            spanStartIndex += indexOfItem;
        }

        return indicesLength;
    }
}
