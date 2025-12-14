using System;
using System.Buffers;
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
#if NET9_0_OR_GREATER
            sizeof(byte) => IndicesOf(ref Unsafe.As<T, byte>(ref reference), items.Length, Unsafe.BitCast<T, byte>(item), ref destinationRef),
            sizeof(ushort) => IndicesOf(ref Unsafe.As<T, ushort>(ref reference), items.Length, Unsafe.BitCast<T, ushort>(item), ref destinationRef),
            sizeof(uint) => IndicesOf(ref Unsafe.As<T, uint>(ref reference), items.Length, Unsafe.BitCast<T, uint>(item), ref destinationRef),
            sizeof(ulong) => IndicesOf(ref Unsafe.As<T, ulong>(ref reference), items.Length, Unsafe.BitCast<T, ulong>(item), ref destinationRef),
#else
            sizeof(byte) => IndicesOf(ref Unsafe.As<T, byte>(ref reference), items.Length, Unsafe.As<T, byte>(ref item), ref destinationRef),
            sizeof(ushort) => IndicesOf(ref Unsafe.As<T, ushort>(ref reference), items.Length, Unsafe.As<T, ushort>(ref item), ref destinationRef),
            sizeof(uint) => IndicesOf(ref Unsafe.As<T, uint>(ref reference), items.Length, Unsafe.As<T, uint>(ref item), ref destinationRef),
            sizeof(ulong) => IndicesOf(ref Unsafe.As<T, ulong>(ref reference), items.Length, Unsafe.As<T, ulong>(ref item), ref destinationRef),
#endif
            _ => IndicesOfNonOptimizedFallback(items, item, destination)
        };

        [DoesNotReturn]
        static void ThrowDestinationTooShort()
            => throw new InvalidOperationException($"The destination needs to be at least as long as the {typeof(Span<T>)} of items provided.");
    }

    public static int IndicesOf<T>(ref T items, int length, T item, ref int destination) where T : unmanaged, IEquatable<T>
    {
        Debug.Assert(Vector<T>.IsSupported, "Support of the generic type has to be ensured before calling this method.");
        Debug.Assert(length >= 0);

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
                        equals ^= (1U << index);
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

    [MethodImpl(MethodImplOptions.NoInlining)]
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

#if NET10_0_OR_GREATER
    public static int IndicesOfCompressImpl<T>(ref T items, int length, T item, ref int destination)
        where T : unmanaged, IEquatable<T>, INumber<T>, IMinMaxValue<T>
    {
        Debug.Assert(Vector<T>.IsSupported, "Support of the generic type has to be ensured before calling this method.");
        Debug.Assert(length >= 0);

        int indicesLength = 0;
        int startIndex = 0;
        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> searchVector = Vector512.Create(item);
            while (length - startIndex >= Vector512<T>.Count)
            {
                Vector512<T> itemsVector = Vector512.LoadUnsafe(ref Unsafe.Add(ref items, (uint)startIndex));
                Vector512<T> equalsValues = Vector512.Equals(itemsVector, searchVector);
                ulong equalsBits = equalsValues.ExtractMostSignificantBits();
                int count = BitOperations.PopCount(equalsBits);

                if (count != 0)
                {
                    Vector512<T> indices = Vector512<T>.Indices + Vector512.Create(T.CreateTruncating(startIndex));
                    indicesLength += count;
                    Vector512<T> compressed = Compress512(Vector512<T>.Zero, equalsValues, indices);
                    Store512(compressed, ref Unsafe.As<int, uint>(ref destination));
                    destination = ref Unsafe.Add(ref destination, (uint)count);
                }

                startIndex += Vector512<T>.Count;
            }

            goto RemainderLoop;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            ThrowHelper.ThrowNotImplementedException();
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            ThrowHelper.ThrowNotImplementedException();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanUseIndicesOfCompressImpl<T>(int inputLength) where T : IEquatable<T>, INumber<T>, IMinMaxValue<T>
        => Avx512F.IsSupported && Avx512Vbmi2.IsSupported &&
           // as it is saturating, it can't be checked for greater or equal, even though it would be a valid index
           T.MaxValue > T.CreateSaturating(inputLength - 1);

    [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "RedundantUnsafeContext")]
    private static unsafe Vector512<T> Compress512<T>(Vector512<T> source, Vector512<T> mask, Vector512<T> value) where T : unmanaged =>
        sizeof(T) switch
        {
            sizeof(byte) => Avx512Vbmi2.Compress(source.AsByte(), mask.AsByte(), value.AsByte()).As<byte, T>(),
            sizeof(ushort) => Avx512Vbmi2.Compress(source.AsUInt16(), mask.AsUInt16(), value.AsUInt16()).As<ushort, T>(),
            sizeof(uint) => Avx512F.Compress(source.AsUInt32(), mask.AsUInt32(), value.AsUInt32()).As<uint, T>(),
            sizeof(ulong) => Avx512F.Compress(source.AsUInt64(), mask.AsUInt64(), value.AsUInt64()).As<ulong, T>(),
            _ => throw new UnreachableException()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void Store512<T>(Vector512<T> source, ref uint destination)
        where T : unmanaged
    {
        switch (sizeof(T))
        {
            case sizeof(byte):
            {
                Vector512<byte> vector = source.AsByte();
                (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(vector);
                Store512(lower, ref destination);
                Store512(upper, ref Unsafe.Add(ref destination, Vector512<ushort>.Count));
                break;
            }
            case sizeof(ushort):
            {
                Vector512<ushort> vector = source.AsUInt16();
                (Vector512<uint> lower, Vector512<uint> upper) = Vector512.Widen(vector);
                Store512(lower, ref destination);
                Store512(upper, ref Unsafe.Add(ref destination, Vector512<uint>.Count));
                break;
            }
            case sizeof(uint):
            {
                Vector512<uint> vector = source.AsUInt32();
                vector.StoreUnsafe(ref destination);
                break;
            }
            case sizeof(ulong):
            {
                Vector512<ulong> vector = source.AsUInt64();
                Vector512<uint> narrow = Vector512.Narrow(vector, Vector512<ulong>.Zero);
                narrow.StoreUnsafe(ref destination);
                break;
            }
            default:
                ThrowHelper.ThrowUnreachableException();
                break;
        }
    }
#endif
}
