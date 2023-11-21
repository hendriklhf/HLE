using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Collections;

public static partial class SpanHelpers
{
    public static void BitwiseAnd<T>(Span<T> values, T and) where T : IBitwiseOperators<T, T, T>
        => BitwiseAnd(ref MemoryMarshal.GetReference(values), values.Length, and);

    public static void BitwiseAnd<T>(ref T values, int length, T and) where T : IBitwiseOperators<T, T, T>
    {
        int vector512Count = Vector512<T>.Count;
        if (Vector512.IsHardwareAccelerated && length >= vector512Count)
        {
            Vector512<T> andVector = Vector512.Create(and);
            while (length >= vector512Count)
            {
                Vector512<T> vector = Vector512.LoadUnsafe(ref values);
                vector &= andVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, vector512Count);
                length -= vector512Count;
            }

            if (length <= Vector512<T>.Count >> 2)
            {
                goto Loop;
            }

            int remainingStart = vector512Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector512<T> remainder = Vector512.LoadUnsafe(ref values);
            remainder &= andVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        int vector256Count = Vector256<T>.Count;
        if (Vector256.IsHardwareAccelerated && length >= vector256Count)
        {
            Vector256<T> andVector = Vector256.Create(and);
            while (length >= vector256Count)
            {
                Vector256<T> vector = Vector256.LoadUnsafe(ref values);
                vector &= andVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, vector256Count);
                length -= vector256Count;
            }

            if (length <= Vector256<T>.Count >> 2)
            {
                goto Loop;
            }

            int remainingStart = vector256Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector256<T> remainder = Vector256.LoadUnsafe(ref values);
            remainder &= andVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        int vector128Count = Vector128<T>.Count;
        if (Vector128.IsHardwareAccelerated && length >= vector128Count)
        {
            Vector128<T> andVector = Vector128.Create(and);
            while (length >= vector128Count)
            {
                Vector128<T> vector = Vector128.LoadUnsafe(ref values);
                vector &= andVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, vector128Count);
                length -= vector128Count;
            }

            if (length <= Vector128<T>.Count >> 2)
            {
                goto Loop;
            }

            int remainingStart = vector128Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector128<T> remainder = Vector128.LoadUnsafe(ref values);
            remainder &= andVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        Loop:
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref values, i) &= and;
        }
    }
}
