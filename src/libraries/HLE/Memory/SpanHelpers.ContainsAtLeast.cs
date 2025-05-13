using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using HLE.Marshalling;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    [Pure]
    public static bool ContainsAtLeast<T>(this List<T> items, T item, int count) where T : unmanaged
        => ContainsAtLeast(CollectionsMarshal.AsSpan(items), item, count);

    [Pure]
    public static bool ContainsAtLeast<T>(this T[] items, T item, int count) where T : unmanaged
        => ContainsAtLeast(items.AsSpan(), item, count);

    [Pure]
    public static bool ContainsAtLeast<T>(this Span<T> items, T item, int count) where T : unmanaged
        => ContainsAtLeast((ReadOnlySpan<T>)items, item, count);

    [Pure]
    public static unsafe bool ContainsAtLeast<T>(this ReadOnlySpan<T> items, T item, int count) where T : unmanaged
    {
        if (items.Length == 0)
        {
            return false;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (!StructMarshal.IsBitwiseEquatable<T>())
        {
            return ContainsAtLeastNonOptimizedFallback(items, item, count);
        }

        ref T reference = ref MemoryMarshal.GetReference(items);
        return sizeof(T) switch
        {
            sizeof(byte) => ContainsAtLeast(ref Unsafe.As<T, byte>(ref reference), items.Length, Unsafe.BitCast<T, byte>(item), count),
            sizeof(ushort) => ContainsAtLeast(ref Unsafe.As<T, ushort>(ref reference), items.Length, Unsafe.BitCast<T, ushort>(item), count),
            sizeof(uint) => ContainsAtLeast(ref Unsafe.As<T, uint>(ref reference), items.Length, Unsafe.BitCast<T, uint>(item), count),
            sizeof(ulong) => ContainsAtLeast(ref Unsafe.As<T, ulong>(ref reference), items.Length, Unsafe.BitCast<T, ulong>(item), count),
            _ => ContainsAtLeastNonOptimizedFallback(items, item, count)
        };
    }

    public static bool ContainsAtLeast<T>(ref T items, int length, T item, int count) where T : unmanaged
    {
        Debug.Assert(Vector<T>.IsSupported, "Support of the generic type has to be ensured before calling this method.");

        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> search = Vector512.Create(item);
            while (length >= Vector512<T>.Count)
            {
                ulong finds = Vector512.Equals(Vector512.LoadUnsafe(ref items), search)
                    .ExtractMostSignificantBits();

                count -= BitOperations.PopCount(finds);
                if (count <= 0)
                {
                    return true;
                }

                items = ref Unsafe.Add(ref items, Vector512<T>.Count);
                length -= Vector512<T>.Count;
            }
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> search = Vector256.Create(item);
            while (length >= Vector256<T>.Count)
            {
                uint finds = Vector256.Equals(Vector256.LoadUnsafe(ref items), search)
                    .ExtractMostSignificantBits();

                count -= BitOperations.PopCount(finds);
                if (count <= 0)
                {
                    return true;
                }

                items = ref Unsafe.Add(ref items, Vector256<T>.Count);
                length -= Vector256<T>.Count;
            }
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> search = Vector128.Create(item);
            while (length >= Vector128<T>.Count)
            {
                uint finds = Vector128.Equals(Vector128.LoadUnsafe(ref items), search)
                    .ExtractMostSignificantBits();

                count -= BitOperations.PopCount(finds);
                if (count <= 0)
                {
                    return true;
                }

                items = ref Unsafe.Add(ref items, Vector128<T>.Count);
                length -= Vector128<T>.Count;
            }
        }

        return ContainsAtLeastNonOptimizedFallback(MemoryMarshal.CreateReadOnlySpan(ref items, length), item, count);
    }

    private static bool ContainsAtLeastNonOptimizedFallback<T>(ReadOnlySpan<T> items, T item, int count)
    {
        Debug.Assert(items.Length != 0);

        for (int i = 0; i < items.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(items[i], item) && --count == 0)
            {
                return true;
            }
        }

        return false;
    }
}
