using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    public static unsafe void Xor<T>(Span<T> values, T mask) where T : IBitwiseOperators<T, T, T>
    {
        EnsureValidIntegerType<T>();

        ref T reference = ref MemoryMarshal.GetReference(values);

        switch (sizeof(T))
        {
            case sizeof(byte):
                Xor(ref Unsafe.As<T, byte>(ref reference), values.Length, Unsafe.As<T, byte>(ref mask));
                return;
            case sizeof(ushort):
                Xor(ref Unsafe.As<T, ushort>(ref reference), values.Length, Unsafe.As<T, ushort>(ref mask));
                return;
            case sizeof(uint):
                Xor(ref Unsafe.As<T, uint>(ref reference), values.Length, Unsafe.As<T, uint>(ref mask));
                return;
            case sizeof(ulong):
                Xor(ref Unsafe.As<T, ulong>(ref reference), values.Length, Unsafe.As<T, ulong>(ref mask));
                return;
        }

        int length = values.Length;
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref reference, i) ^= mask;
        }
    }

    public static void Xor<T>(ref T values, int length, T mask) where T : IBitwiseOperators<T, T, T>
    {
        EnsureValidIntegerType<T>();

        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> maskVector = Vector512.Create(mask);
            while (length >= Vector512<T>.Count)
            {
                Vector512<T> vector = Vector512.LoadUnsafe(ref values);
                vector ^= maskVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector512<T>.Count);
                length -= Vector512<T>.Count;
            }

            if (length < Vector512<T>.Count >> 1)
            {
                goto RemainderLoop;
            }

            int remainingStart = Vector512<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector512<T> remainder = Vector512.LoadUnsafe(ref values);
            remainder ^= maskVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> maskVector = Vector256.Create(mask);
            while (length >= Vector256<T>.Count)
            {
                Vector256<T> vector = Vector256.LoadUnsafe(ref values);
                vector ^= maskVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector256<T>.Count);
                length -= Vector256<T>.Count;
            }

            if (length < Vector256<T>.Count >> 1)
            {
                goto RemainderLoop;
            }

            int remainingStart = Vector256<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector256<T> remainder = Vector256.LoadUnsafe(ref values);
            remainder ^= maskVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> maskVector = Vector128.Create(mask);
            while (length >= Vector128<T>.Count)
            {
                Vector128<T> vector = Vector128.LoadUnsafe(ref values);
                vector ^= maskVector;
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector128<T>.Count);
                length -= Vector128<T>.Count;
            }

            if (length < Vector128<T>.Count >> 1)
            {
                goto RemainderLoop;
            }

            int remainingStart = Vector128<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            Vector128<T> remainder = Vector128.LoadUnsafe(ref values);
            remainder ^= maskVector;
            remainder.StoreUnsafe(ref values);
            return;
        }

        RemainderLoop:
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref values, i) ^= mask;
        }
    }

    public static unsafe void Xor<T>(Span<T> values, ReadOnlySpan<T> mask) where T : IBitwiseOperators<T, T, T>
    {
        EnsureValidIntegerType<T>();

        if (values.Length != mask.Length)
        {
            ThrowLengthsAreNotEqual();
        }

        ref T reference = ref MemoryMarshal.GetReference(values);
        ref T maskReference = ref MemoryMarshal.GetReference(mask);

        switch (sizeof(T))
        {
            case sizeof(byte):
                Xor(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref maskReference), values.Length);
                return;
            case sizeof(ushort):
                Xor(ref Unsafe.As<T, ushort>(ref reference), ref Unsafe.As<T, ushort>(ref maskReference), values.Length);
                return;
            case sizeof(uint):
                Xor(ref Unsafe.As<T, uint>(ref reference), ref Unsafe.As<T, uint>(ref maskReference), values.Length);
                return;
            case sizeof(ulong):
                Xor(ref Unsafe.As<T, ulong>(ref reference), ref Unsafe.As<T, ulong>(ref maskReference), values.Length);
                return;
        }

        int length = values.Length;
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref reference, i) ^= Unsafe.Add(ref maskReference, i);
        }
    }

    public static void Xor<T>(ref T values, ref T mask, int length) where T : IBitwiseOperators<T, T, T>
    {
        EnsureValidIntegerType<T>();

        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            while (length >= Vector512<T>.Count)
            {
                Vector512<T> vector = Vector512.LoadUnsafe(ref values);
                vector ^= Vector512.LoadUnsafe(ref mask);
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector512<T>.Count);
                mask = ref Unsafe.Add(ref mask, Vector512<T>.Count);
                length -= Vector512<T>.Count;
            }

            if (length < Vector512<T>.Count >> 1)
            {
                goto RemainderLoop;
            }

            int remainingStart = Vector512<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            mask = ref Unsafe.Subtract(ref mask, remainingStart);
            Vector512<T> remainder = Vector512.LoadUnsafe(ref values);
            remainder ^= Vector512.LoadUnsafe(ref mask);
            remainder.StoreUnsafe(ref values);
            return;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            while (length >= Vector256<T>.Count)
            {
                Vector256<T> vector = Vector256.LoadUnsafe(ref values);
                vector ^= Vector256.LoadUnsafe(ref mask);
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector256<T>.Count);
                mask = ref Unsafe.Add(ref mask, Vector256<T>.Count);
                length -= Vector256<T>.Count;
            }

            if (length < Vector256<T>.Count >> 1)
            {
                goto RemainderLoop;
            }

            int remainingStart = Vector256<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            mask = ref Unsafe.Subtract(ref mask, remainingStart);
            Vector256<T> remainder = Vector256.LoadUnsafe(ref values);
            remainder ^= Vector256.LoadUnsafe(ref mask);
            remainder.StoreUnsafe(ref values);
            return;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            while (length >= Vector128<T>.Count)
            {
                Vector128<T> vector = Vector128.LoadUnsafe(ref values);
                vector ^= Vector128.LoadUnsafe(ref mask);
                vector.StoreUnsafe(ref values);

                values = ref Unsafe.Add(ref values, Vector128<T>.Count);
                mask = ref Unsafe.Add(ref mask, Vector128<T>.Count);
                length -= Vector128<T>.Count;
            }

            if (length < Vector128<T>.Count >> 1)
            {
                goto RemainderLoop;
            }

            int remainingStart = Vector128<T>.Count - length;
            values = ref Unsafe.Subtract(ref values, remainingStart);
            mask = ref Unsafe.Subtract(ref mask, remainingStart);
            Vector128<T> remainder = Vector128.LoadUnsafe(ref values);
            remainder ^= Vector128.LoadUnsafe(ref mask);
            remainder.StoreUnsafe(ref values);
            return;
        }

        RemainderLoop:
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref values, i) ^= Unsafe.Add(ref mask, i);
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowLengthsAreNotEqual()
        => throw new InvalidOperationException("The length of the values and the mask have to be the same.");
}
