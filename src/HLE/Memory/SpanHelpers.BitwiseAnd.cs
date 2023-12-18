using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    public static unsafe void BitwiseAnd<T>(Span<T> values, T mask) where T : IBitwiseOperators<T, T, T>
    {
        ref T reference = ref MemoryMarshal.GetReference(values);

        switch (sizeof(T))
        {
            case sizeof(byte):
                BitwiseAnd(ref Unsafe.As<T, byte>(ref reference), values.Length, Unsafe.As<T, byte>(ref mask));
                return;
            case sizeof(ushort):
                BitwiseAnd(ref Unsafe.As<T, ushort>(ref reference), values.Length, Unsafe.As<T, ushort>(ref mask));
                return;
            case sizeof(uint):
                BitwiseAnd(ref Unsafe.As<T, uint>(ref reference), values.Length, Unsafe.As<T, uint>(ref mask));
                return;
            case sizeof(ulong):
                BitwiseAnd(ref Unsafe.As<T, ulong>(ref reference), values.Length, Unsafe.As<T, ulong>(ref mask));
                return;
        }

        int length = values.Length;
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref reference, i) &= mask;
        }
    }

    public static void BitwiseAnd<T>(ref T values, int length, T mask) where T : IBitwiseOperators<T, T, T>
    {
        Debug.Assert(Vector<T>.IsSupported, "Support of the generic type has to be ensured before calling this method.");

        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> maksVector = Vector512.Create(mask);
            while (length >= Vector512<T>.Count)
            {
                Vector512<T> vector = Vector512.LoadUnsafe(ref values);
                vector &= maksVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector512<T>.Count);
                length -= Vector512<T>.Count;
            }

            if (length < Vector512<T>.Count >> 1)
            {
                goto Loop;
            }

            int remainingStart = Vector512<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector512<T> remainder = Vector512.LoadUnsafe(ref values);
            remainder &= maksVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> maskVector = Vector256.Create(mask);
            while (length >= Vector256<T>.Count)
            {
                Vector256<T> vector = Vector256.LoadUnsafe(ref values);
                vector &= maskVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector256<T>.Count);
                length -= Vector256<T>.Count;
            }

            if (length < Vector256<T>.Count >> 1)
            {
                goto Loop;
            }

            int remainingStart = Vector256<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector256<T> remainder = Vector256.LoadUnsafe(ref values);
            remainder &= maskVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> maskVector = Vector128.Create(mask);
            while (length >= Vector128<T>.Count)
            {
                Vector128<T> vector = Vector128.LoadUnsafe(ref values);
                vector &= maskVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector128<T>.Count);
                length -= Vector128<T>.Count;
            }

            if (length < Vector128<T>.Count >> 1)
            {
                goto Loop;
            }

            int remainingStart = Vector128<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector128<T> remainder = Vector128.LoadUnsafe(ref values);
            remainder &= maskVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        Loop:
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref values, i) &= mask;
        }
    }
}
