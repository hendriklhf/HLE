using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    public static unsafe int IndexOfDifference<T>(ReadOnlySpan<T> first, ReadOnlySpan<T> second)
        where T : IEquatable<T>
    {
        ref T firstRef = ref MemoryMarshal.GetReference(first);
        ref T secondRef = ref MemoryMarshal.GetReference(second);

        return sizeof(T) switch
        {
            sizeof(byte) => IndexOfDifference(ref Unsafe.As<T, byte>(ref firstRef), first.Length, ref Unsafe.As<T, byte>(ref secondRef), second.Length),
            sizeof(ushort) => IndexOfDifference(ref Unsafe.As<T, ushort>(ref firstRef), first.Length, ref Unsafe.As<T, ushort>(ref secondRef), second.Length),
            sizeof(uint) => IndexOfDifference(ref Unsafe.As<T, uint>(ref firstRef), first.Length, ref Unsafe.As<T, uint>(ref secondRef), second.Length),
            sizeof(ulong) => IndexOfDifference(ref Unsafe.As<T, ulong>(ref firstRef), first.Length, ref Unsafe.As<T, ulong>(ref secondRef), second.Length),
            _ => IndexOfDifferenceFallback(first, second)
        };
    }

    public static int IndexOfDifference<T>(ref T first, int firstLength, ref T second, int secondLength)
        where T : unmanaged, IEquatable<T>
    {
        Debug.Assert(Vector<T>.IsSupported, "Support of the generic type has to be ensured before calling this method.");

        int length = Math.Min(firstLength, secondLength);
        int start = 0;

        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            while (length >= Vector512<T>.Count)
            {
                Vector512<T> firstVector = Vector512.LoadUnsafe(ref first);
                Vector512<T> secondVector = Vector512.LoadUnsafe(ref second);
                Vector512<T> equalsVector = Vector512.Equals(firstVector, secondVector);

                ulong equalsBits = ulong.MaxValue & ~equalsVector.ExtractMostSignificantBits();
                int index = BitOperations.TrailingZeroCount(equalsBits);
                if (index != Vector512<T>.Count)
                {
                    return start + index;
                }

                first = ref Unsafe.Add(ref first, Vector512<T>.Count);
                second = ref Unsafe.Add(ref second, Vector512<T>.Count);
                length -= Vector512<T>.Count;
                start += Vector512<T>.Count;
            }
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            while (length >= Vector256<T>.Count)
            {
                Vector256<T> firstVector = Vector256.LoadUnsafe(ref first);
                Vector256<T> secondVector = Vector256.LoadUnsafe(ref second);
                Vector256<T> equalsVector = Vector256.Equals(firstVector, secondVector);

                uint equalsBits = uint.MaxValue & ~equalsVector.ExtractMostSignificantBits();
                int index = BitOperations.TrailingZeroCount(equalsBits);
                if (index != Vector256<T>.Count)
                {
                    return start + index;
                }

                first = ref Unsafe.Add(ref first, Vector256<T>.Count);
                second = ref Unsafe.Add(ref second, Vector256<T>.Count);
                length -= Vector256<T>.Count;
                start += Vector256<T>.Count;
            }
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            while (length >= Vector128<T>.Count)
            {
                Vector128<T> firstVector = Vector128.LoadUnsafe(ref first);
                Vector128<T> secondVector = Vector128.LoadUnsafe(ref second);
                Vector128<T> equalsVector = Vector128.Equals(firstVector, secondVector);

                uint equalsBits = uint.MaxValue & ~equalsVector.ExtractMostSignificantBits();
                int index = BitOperations.TrailingZeroCount(equalsBits);
                if (index != Vector128<T>.Count)
                {
                    return start + index;
                }

                first = ref Unsafe.Add(ref first, Vector128<T>.Count);
                second = ref Unsafe.Add(ref second, Vector128<T>.Count);
                length -= Vector128<T>.Count;
                start += Vector128<T>.Count;
            }
        }

        for (int i = 0; i < length; i++)
        {
            if (!first.Equals(second))
            {
                return start + i;
            }

            first = ref Unsafe.Add(ref first, 1);
            second = ref Unsafe.Add(ref second, 1);
        }

        return -1;
    }

    private static int IndexOfDifferenceFallback<T>(ReadOnlySpan<T> first, ReadOnlySpan<T> second)
        where T : IEquatable<T>
    {
        ref T firstRef = ref MemoryMarshal.GetReference(first);
        ref T secondRef = ref MemoryMarshal.GetReference(second);
        int length = Math.Min(first.Length, second.Length);

        for (int i = 0; i < length; i++)
        {
            if (!firstRef.Equals(secondRef))
            {
                return i;
            }

            firstRef = ref Unsafe.Add(ref firstRef, 1);
            secondRef = ref Unsafe.Add(ref secondRef, 1);
        }

        return -1;
    }
}
