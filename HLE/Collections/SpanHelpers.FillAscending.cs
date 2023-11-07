using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HLE.Collections;

public static partial class SpanHelpers
{
    public static void FillAscending<T>(Span<T> destination) where T : unmanaged, INumber<T>
        => FillAscending(destination, T.Zero);

    public static unsafe void FillAscending<T>(Span<T> destination, T start) where T : unmanaged, INumber<T>
    {
        ref T destinationReference = ref MemoryMarshal.GetReference(destination);

        switch (sizeof(T))
        {
            case sizeof(byte):
                FillAscending(ref Unsafe.As<T, byte>(ref destinationReference), destination.Length, Unsafe.As<T, byte>(ref start));
                return;
            case sizeof(ushort):
                FillAscending(ref Unsafe.As<T, ushort>(ref destinationReference), destination.Length, Unsafe.As<T, ushort>(ref start));
                return;
            case sizeof(uint):
                FillAscending(ref Unsafe.As<T, uint>(ref destinationReference), destination.Length, Unsafe.As<T, uint>(ref start));
                return;
            case sizeof(ulong):
                FillAscending(ref Unsafe.As<T, ulong>(ref destinationReference), destination.Length, Unsafe.As<T, ulong>(ref start));
                return;
        }

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = start++;
        }
    }

    public static void FillAscending<T>(ref T destination, int length, T start) where T : unmanaged, INumber<T>
    {
        if (Vector512.IsHardwareAccelerated && length >= Vector512<T>.Count)
        {
            Vector512<T> ascendingValueAdditions = Vector512<T>.Zero;
            CreateAscendingValueVector(ref Unsafe.As<Vector512<T>, T>(ref ascendingValueAdditions), Vector512<T>.Count);
            while (length >= Vector512<T>.Count)
            {
                Vector512<T> startValues = Vector512.Create(start);
                Vector512<T> values = Vector512.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref destination);

                destination = ref Unsafe.Add(ref destination, Vector512<T>.Count);
                start += T.CreateTruncating(Vector512<T>.Count);
                length -= Vector512<T>.Count;
            }

            goto Loop;
        }

        if (Vector256.IsHardwareAccelerated && length >= Vector256<T>.Count)
        {
            Vector256<T> ascendingValueAdditions = Vector256<T>.Zero;
            CreateAscendingValueVector(ref Unsafe.As<Vector256<T>, T>(ref ascendingValueAdditions), Vector256<T>.Count);
            while (length >= Vector256<T>.Count)
            {
                Vector256<T> startValues = Vector256.Create(start);
                Vector256<T> values = Vector256.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref destination);

                destination = ref Unsafe.Add(ref destination, Vector256<T>.Count);
                start += T.CreateTruncating(Vector256<T>.Count);
                length -= Vector256<T>.Count;
            }

            goto Loop;
        }

        if (Vector128.IsHardwareAccelerated && length >= Vector128<T>.Count)
        {
            Vector128<T> ascendingValueAdditions = Vector128<T>.Zero;
            CreateAscendingValueVector(ref Unsafe.As<Vector128<T>, T>(ref ascendingValueAdditions), Vector128<T>.Count);
            while (length >= Vector128<T>.Count)
            {
                Vector128<T> startValues = Vector128.Create(start);
                Vector128<T> values = Vector128.Add(startValues, ascendingValueAdditions);
                values.StoreUnsafe(ref destination);

                destination = ref Unsafe.Add(ref destination, Vector128<T>.Count);
                start += T.CreateTruncating(Vector128<T>.Count);
                length -= Vector128<T>.Count;
            }
        }

        Loop:
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref destination, i) = start + T.CreateTruncating(i);
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
