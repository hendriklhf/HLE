using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static unsafe partial class SpanHelpers
{
    /// <inheritdoc cref="Memmove{T,TElementCount}(ref T,ref T,TElementCount)"/>
    public static void Memmove<T, TElementCount>(T* destination, T* source, TElementCount elementCount)
        where TElementCount : unmanaged, IBinaryInteger<TElementCount>
        => Memmove(ref Unsafe.AsRef<T>(destination), ref Unsafe.AsRef<T>(source), elementCount);

    /// <summary>
    /// Copies the given amount of elements from the source into the destination.
    /// </summary>
    /// <typeparam name="T">The element type that will be copied.</typeparam>
    /// <typeparam name="TElementCount">The type that represents the amount of elements that will be copied.</typeparam>
    /// <param name="destination">The destination of the elements.</param>
    /// <param name="source">The source of the elements.</param>
    /// <param name="elementCount">The amount of elements that will be copied from source to destination.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Memmove<T, TElementCount>(ref T destination, ref T source, TElementCount elementCount)
        where TElementCount : unmanaged, IBinaryInteger<TElementCount>
    {
        ValidateElementCountType<TElementCount>();

        if (typeof(TElementCount) == typeof(sbyte) || typeof(TElementCount) == typeof(short) ||
            typeof(TElementCount) == typeof(int) || typeof(TElementCount) == typeof(long) ||
            typeof(TElementCount) == typeof(nint))
        {
            ArgumentOutOfRangeException.ThrowIfNegative(elementCount);
        }

        if (typeof(TElementCount) == typeof(sbyte) || typeof(TElementCount) == typeof(byte))
        {
            MemoryMarshal.CreateReadOnlySpan(ref source, Unsafe.BitCast<TElementCount, byte>(elementCount))
                .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));
            return;
        }

        if (typeof(TElementCount) == typeof(short) || typeof(TElementCount) == typeof(ushort))
        {
            MemoryMarshal.CreateReadOnlySpan(ref source, Unsafe.BitCast<TElementCount, ushort>(elementCount))
                .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));
            return;
        }

        if (typeof(TElementCount) == typeof(int) || (sizeof(int) == sizeof(nint) && typeof(TElementCount) == typeof(nint)))
        {
            MemoryMarshal.CreateReadOnlySpan(ref source, Unsafe.BitCast<TElementCount, int>(elementCount))
                .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));
            return;
        }

        if (typeof(TElementCount) == typeof(uint) || (sizeof(uint) == sizeof(nuint) && typeof(TElementCount) == typeof(nuint)))
        {
            uint count = Unsafe.BitCast<TElementCount, uint>(elementCount);
            if (count > int.MaxValue)
            {
                MemoryMarshal.CreateReadOnlySpan(ref source, int.MaxValue)
                    .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));
                source = ref Unsafe.Add(ref source, int.MaxValue);
                destination = ref Unsafe.Add(ref destination, int.MaxValue);
                count -= int.MaxValue;
            }

            MemoryMarshal.CreateReadOnlySpan(ref source, (int)count)
                .CopyTo(MemoryMarshal.CreateSpan(ref destination, int.MaxValue));
            return;
        }

        if (typeof(TElementCount) == typeof(long) || typeof(TElementCount) == typeof(nint) ||
            typeof(TElementCount) == typeof(ulong) || typeof(TElementCount) == typeof(nuint))
        {
            ulong count = Unsafe.BitCast<TElementCount, ulong>(elementCount);
            if (count <= int.MaxValue)
            {
                Memmove(ref destination, ref source, (int)count);
                return;
            }

            do
            {
                Memmove(ref destination, ref source, int.MaxValue);
                source = ref Unsafe.Add(ref source, int.MaxValue);
                destination = ref Unsafe.Add(ref destination, int.MaxValue);
                count -= int.MaxValue;
            }
            while (count >= int.MaxValue);

            if (count != 0)
            {
                Memmove(ref destination, ref source, (int)count);
            }

            return;
        }

        ThrowHelper.ThrowUnreachableException();
    }
}
