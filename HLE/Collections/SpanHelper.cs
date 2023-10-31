using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Collections;

public static class SpanHelper
{
    public static void Add<T>(Span<T> numbers, T addition) where T : INumber<T>
        => Add(ref MemoryMarshal.GetReference(numbers), numbers.Length, addition);

    public static void Add<T>(ref T numbers, int length, T addition) where T : INumber<T>
    {
        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> additionVector = Vector512.Create(addition);
            Vector512<T> vector = Vector512.LoadUnsafe(ref numbers);
            vector += additionVector;
            vector.StoreUnsafe(ref numbers);
            numbers = ref Unsafe.Add(ref numbers, Vector512<T>.Count);
            length -= Vector512<T>.Count;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> additionVector = Vector256.Create(addition);
            Vector256<T> vector = Vector256.LoadUnsafe(ref numbers);
            vector += additionVector;
            vector.StoreUnsafe(ref numbers);
            numbers = ref Unsafe.Add(ref numbers, Vector256<T>.Count);
            length -= Vector256<T>.Count;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> additionVector = Vector128.Create(addition);
            Vector128<T> vector = Vector128.LoadUnsafe(ref numbers);
            vector += additionVector;
            vector.StoreUnsafe(ref numbers);
            numbers = ref Unsafe.Add(ref numbers, Vector128<T>.Count);
            length -= Vector128<T>.Count;
        }

        if (Vector64.IsHardwareAccelerated && length >= Vector64<T>.Count)
        {
            Vector64<T> additionVector = Vector64.Create(addition);
            Vector64<T> vector = Vector64.LoadUnsafe(ref numbers);
            vector += additionVector;
            vector.StoreUnsafe(ref numbers);
            numbers = ref Unsafe.Add(ref numbers, Vector64<T>.Count);
            length -= Vector64<T>.Count;
        }

        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref numbers, i) += addition;
        }
    }
}
