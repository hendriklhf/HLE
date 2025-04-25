using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using HLE.Collections;
using HLE.Marshalling;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    [Pure]
    public static int[] IndicesOf<T>(this List<T> items, T item) where T : IEquatable<T>
        => IndicesOf(CollectionsMarshal.AsSpan(items), item);

    [Pure]
    public static int[] IndicesOf<T>(this T[] array, T items) where T : IEquatable<T>
        => IndicesOf(array.AsSpan(), items);

    [Pure]
    public static int[] IndicesOf<T>(this Span<T> items, T item) where T : IEquatable<T>
        => IndicesOf((ReadOnlySpan<T>)items, item);

    [Pure]
    [SkipLocalsInit]
    public static int[] IndicesOf<T>(this ReadOnlySpan<T> items, T item) where T : IEquatable<T>
    {
        if (items.Length == 0)
        {
            return [];
        }

        int length;
        if (!MemoryHelpers.UseStackalloc<int>(items.Length))
        {
            int[] indicesBuffer = ArrayPool<int>.Shared.Rent(items.Length);
            length = IndicesOf(items, item, indicesBuffer.AsSpan());
            int[] result = indicesBuffer[..length];
            ArrayPool<int>.Shared.Return(indicesBuffer);
            return result;
        }

        Span<int> indices = stackalloc int[items.Length];
        length = IndicesOf(items, item, indices);
        return indices.ToArray(..length);
    }

    public static int IndicesOf<T>(this Span<T> items, T item, Span<int> destination) where T : IEquatable<T>
        => IndicesOf((ReadOnlySpan<T>)items, item, destination);

    public static unsafe int IndicesOf<T>(this ReadOnlySpan<T> items, T item, Span<int> destination) where T : IEquatable<T>
    {
        if (items.Length == 0)
        {
            return 0;
        }

        if (!StructMarshal.IsBitwiseEquatable<T>())
        {
            return IndicesOfNonOptimizedFallback(items, item, destination);
        }

        if (destination.Length < items.Length)
        {
            ThrowDestinationTooShort();
        }

        ref T reference = ref MemoryMarshal.GetReference(items);
        ref int destinationRef = ref MemoryMarshal.GetReference(destination);
        return sizeof(T) switch
        {
            sizeof(byte) => IndicesOf(ref Unsafe.As<T, byte>(ref reference), items.Length, Unsafe.BitCast<T, byte>(item), ref destinationRef),
            sizeof(ushort) => IndicesOf(ref Unsafe.As<T, ushort>(ref reference), items.Length, Unsafe.BitCast<T, ushort>(item), ref destinationRef),
            sizeof(uint) => IndicesOf(ref Unsafe.As<T, uint>(ref reference), items.Length, Unsafe.BitCast<T, uint>(item), ref destinationRef),
            sizeof(ulong) => IndicesOf(ref Unsafe.As<T, ulong>(ref reference), items.Length, Unsafe.BitCast<T, ulong>(item), ref destinationRef),
            _ => IndicesOfNonOptimizedFallback(items, item, destination)
        };

        [DoesNotReturn]
        static void ThrowDestinationTooShort()
            => throw new InvalidOperationException($"The destination needs to be at least as long as the {typeof(Span<T>)} of items provided.");
    }

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
                    if (Bmi1.X64.IsSupported)
                    {
                        equals = Bmi1.X64.ResetLowestSetBit(equals);
                    }
                    else
                    {
                        equals ^= (1UL << index);
                    }
                }

                startIndex += Vector512<T>.Count;
            }

            goto RemainderLoop;
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
                    if (Bmi1.IsSupported)
                    {
                        equals = Bmi1.ResetLowestSetBit(equals);
                    }
                    else
                    {
                        equals ^= (1U << index);
                    }
                }

                startIndex += Vector256<T>.Count;
            }

            goto RemainderLoop;
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
                    if (Bmi1.IsSupported)
                    {
                        equals = Bmi1.ResetLowestSetBit(equals);
                    }
                    else
                    {
                        equals ^= (1U << index);
                    }
                }

                startIndex += Vector128<T>.Count;
            }

            goto RemainderLoop;
        }

        return IndicesOfNonOptimizedFallback(MemoryMarshal.CreateReadOnlySpan(ref items, length), item, MemoryMarshal.CreateSpan(ref destination, length));

    RemainderLoop:
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
