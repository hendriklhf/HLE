using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    /// <inheritdoc cref="SumUnchecked{T}(ReadOnlySpan{T})"/>
    [Pure]
    public static T SumUnchecked<T>(Span<T> items) where T : IBinaryInteger<T>
        => SumUnchecked((ReadOnlySpan<T>)items);

    /// <summary>
    /// Computes the sum of all elements without checking for arithmetic overflow.
    /// </summary>
    /// <param name="items">The elements that will be summed up.</param>
    /// <returns>The sum of all elements.</returns>
    [Pure]
    public static unsafe T SumUnchecked<T>(this ReadOnlySpan<T> items) where T : IBinaryInteger<T>
    {
        ref T reference = ref MemoryMarshal.GetReference(items);

        switch (sizeof(T))
        {
            case sizeof(byte):
                return T.CreateTruncating(SumUnchecked(ref Unsafe.As<T, byte>(ref reference), items.Length));
            case sizeof(ushort):
                return T.CreateTruncating(SumUnchecked(ref Unsafe.As<T, ushort>(ref reference), items.Length));
            case sizeof(uint):
                return T.CreateTruncating(SumUnchecked(ref Unsafe.As<T, uint>(ref reference), items.Length));
            case sizeof(ulong):
                return T.CreateTruncating(SumUnchecked(ref Unsafe.As<T, ulong>(ref reference), items.Length));
        }

        T sum = T.Zero;
        int length = items.Length;
        for (int i = 0; i < length; i++)
        {
            sum += Unsafe.Add(ref reference, i);
        }

        return sum;
    }

    /// <summary>
    /// Computes the sum of all elements without checking for arithmetic overflow.
    /// </summary>
    /// <param name="items">The address of elements that will be summed up.</param>
    /// <param name="length">The amount of elements at the address.</param>
    /// <returns>The sum of all elements.</returns>
    [Pure]
    public static T SumUnchecked<T>(ref T items, int length) where T : unmanaged, IBinaryInteger<T>
    {
        switch (length)
        {
            case 0:
                return T.Zero;
            case 1:
                return items;
            case 2:
                return items + Unsafe.Add(ref items, 1);
        }

        T sum = T.Zero;
        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> vectorSum = Vector512<T>.Zero;
            while (length >= Vector512<T>.Count)
            {
                vectorSum += Vector512.LoadUnsafe(ref items);
                items = ref Unsafe.Add(ref items, Vector512<T>.Count);
                length -= Vector512<T>.Count;
            }

            sum = Vector512.Sum(vectorSum);
            goto Loop;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> vectorSum = Vector256<T>.Zero;
            while (length >= Vector256<T>.Count)
            {
                vectorSum += Vector256.LoadUnsafe(ref items);
                items = ref Unsafe.Add(ref items, Vector256<T>.Count);
                length -= Vector256<T>.Count;
            }

            sum = Vector256.Sum(vectorSum);
            goto Loop;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> vectorSum = Vector128<T>.Zero;
            while (length >= Vector128<T>.Count)
            {
                vectorSum += Vector128.LoadUnsafe(ref items);
                items = ref Unsafe.Add(ref items, Vector128<T>.Count);
                length -= Vector128<T>.Count;
            }

            sum = Vector128.Sum(vectorSum);
        }

        Loop:
        for (int i = 0; i < length; i++)
        {
            sum += Unsafe.Add(ref items, i);
        }

        return sum;
    }
}
