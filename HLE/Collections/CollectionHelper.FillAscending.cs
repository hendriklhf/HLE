using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Collections;

public static partial class CollectionHelper
{
    public static void FillAscending<T>(this T[] array) where T : INumber<T>
        => FillAscending(array, T.Zero);

    public static void FillAscending<T>(this Span<T> span) where T : INumber<T>
        => FillAscending(span, T.Zero);

    public static void FillAscending<T>(this T[] array, T start) where T : INumber<T>
        => FillAscending(array.AsSpan(), start);

    public static void FillAscending<T>(this Span<T> span, T start) where T : INumber<T>
    {
        if (Vector512.IsHardwareAccelerated && span.Length >= Vector512<T>.Count)
        {
            Vector512<T> ascendingValueAdditions = default;
            CreateAscendingValueVector(ref Unsafe.As<Vector512<T>, T>(ref ascendingValueAdditions), Vector512<T>.Count);
            while (span.Length >= Vector512<T>.Count)
            {
                Vector512<T> startValues = Vector512.Create(start);
                Vector512<T> values = Vector512.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += T.CreateTruncating(Vector512<T>.Count);
                span = span.SliceUnsafe(Vector512<T>.Count);
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + T.CreateTruncating(i);
            }

            return;
        }

        if (Vector256.IsHardwareAccelerated && span.Length >= Vector256<T>.Count)
        {
            Vector256<T> ascendingValueAdditions = default;
            CreateAscendingValueVector(ref Unsafe.As<Vector256<T>, T>(ref ascendingValueAdditions), Vector256<T>.Count);
            while (span.Length >= Vector256<T>.Count)
            {
                Vector256<T> startValues = Vector256.Create(start);
                Vector256<T> values = Vector256.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += T.CreateTruncating(Vector256<T>.Count);
                span = span.SliceUnsafe(Vector256<T>.Count);
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + T.CreateTruncating(i);
            }

            return;
        }

        if (Vector128.IsHardwareAccelerated && span.Length >= Vector128<T>.Count)
        {
            Vector128<T> ascendingValueAdditions = default;
            CreateAscendingValueVector(ref Unsafe.As<Vector128<T>, T>(ref ascendingValueAdditions), Vector128<T>.Count);
            while (span.Length >= Vector128<T>.Count)
            {
                Vector128<T> startValues = Vector128.Create(start);
                Vector128<T> values = Vector128.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += T.CreateTruncating(Vector128<T>.Count);
                span = span.SliceUnsafe(Vector128<T>.Count);
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + T.CreateTruncating(i);
            }

            return;
        }

        if (Vector64.IsHardwareAccelerated && span.Length >= Vector64<T>.Count)
        {
            Vector64<T> ascendingValueAdditions = default;
            CreateAscendingValueVector(ref Unsafe.As<Vector64<T>, T>(ref ascendingValueAdditions), Vector64<T>.Count);
            while (span.Length >= Vector64<T>.Count)
            {
                Vector64<T> startValues = Vector64.Create(start);
                Vector64<T> values = Vector64.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref MemoryMarshal.GetReference(span));
                start += T.CreateTruncating(Vector64<T>.Count);
                span = span.SliceUnsafe(Vector64<T>.Count);
            }

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = start + T.CreateTruncating(i);
            }

            return;
        }

        int spanLength = span.Length;
        ref T firstItem = ref MemoryMarshal.GetReference(span);
        for (int i = 0; i < spanLength; i++)
        {
            Unsafe.Add(ref firstItem, i) = start + T.CreateTruncating(i);
        }
    }

    private static unsafe void CreateAscendingValueVector<T>(ref T vector, int vectorSize)
    {
        switch (sizeof(T))
        {
            case sizeof(byte):
            {
                ref byte reference = ref Unsafe.As<T, byte>(ref vector);
                for (int i = 0; i < vectorSize; i++)
                {
                    Unsafe.Add(ref reference, i) = (byte)i;
                }

                return;
            }
            case sizeof(ushort):
            {
                ref ushort reference = ref Unsafe.As<T, ushort>(ref vector);
                for (int i = 0; i < vectorSize; i++)
                {
                    Unsafe.Add(ref reference, i) = (ushort)i;
                }

                return;
            }
            case sizeof(uint):
            {
                ref uint reference = ref Unsafe.As<T, uint>(ref vector);
                for (int i = 0; i < vectorSize; i++)
                {
                    Unsafe.Add(ref reference, i) = (uint)i;
                }

                return;
            }
            case sizeof(ulong):
            {
                ref ulong reference = ref Unsafe.As<T, ulong>(ref vector);
                for (int i = 0; i < vectorSize; i++)
                {
                    Unsafe.Add(ref reference, i) = (ulong)i;
                }

                return;
            }
        }
    }
}
