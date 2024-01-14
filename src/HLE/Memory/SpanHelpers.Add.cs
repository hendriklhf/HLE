using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    public static unsafe void Add<T>(Span<T> numbers, T addition) where T : INumber<T>
    {
        ref T reference = ref MemoryMarshal.GetReference(numbers);

        switch (sizeof(T))
        {
            case sizeof(byte):
                Add(ref Unsafe.As<T, byte>(ref reference), numbers.Length, Unsafe.As<T, byte>(ref addition));
                return;
            case sizeof(ushort):
                Add(ref Unsafe.As<T, ushort>(ref reference), numbers.Length, Unsafe.As<T, ushort>(ref addition));
                return;
            case sizeof(uint):
                Add(ref Unsafe.As<T, uint>(ref reference), numbers.Length, Unsafe.As<T, uint>(ref addition));
                return;
            case sizeof(ulong):
                Add(ref Unsafe.As<T, ulong>(ref reference), numbers.Length, Unsafe.As<T, ulong>(ref addition));
                return;
        }

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] += addition;
        }
    }

    public static void Add<T>(ref T numbers, int length, T addition) where T : unmanaged, INumber<T>
    {
        Debug.Assert(Vector<T>.IsSupported, "Support has to be ensured before calling this method.");

        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> additionVector = Vector512.Create(addition);
            while (length >= Vector512<T>.Count)
            {
                Vector512<T> vector = Vector512.LoadUnsafe(ref numbers);
                vector += additionVector;
                vector.StoreUnsafe(ref numbers);
                numbers = ref Unsafe.Add(ref numbers, Vector512<T>.Count);
                length -= Vector512<T>.Count;
            }

            goto Loop;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> additionVector = Vector256.Create(addition);
            while (length >= Vector256<T>.Count)
            {
                Vector256<T> vector = Vector256.LoadUnsafe(ref numbers);
                vector += additionVector;
                vector.StoreUnsafe(ref numbers);
                numbers = ref Unsafe.Add(ref numbers, Vector256<T>.Count);
                length -= Vector256<T>.Count;
            }

            goto Loop;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> additionVector = Vector128.Create(addition);
            while (length >= Vector128<T>.Count)
            {
                Vector128<T> vector = Vector128.LoadUnsafe(ref numbers);
                vector += additionVector;
                vector.StoreUnsafe(ref numbers);
                numbers = ref Unsafe.Add(ref numbers, Vector128<T>.Count);
                length -= Vector128<T>.Count;
            }
        }

        Loop:
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref numbers, i) += addition;
        }
    }
}
