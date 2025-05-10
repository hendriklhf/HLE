using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static partial class SpanHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear<T, TElementCount>(T[] array, TElementCount elementCount)
        where TElementCount : unmanaged, IBinaryInteger<TElementCount>
    {
        Debug.Assert(TElementCount.CreateSaturating(array.Length) >= elementCount);
        Clear(ref MemoryMarshal.GetArrayDataReference(array), elementCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Clear<T, TElementCount>(T* items, TElementCount elementCount)
        where TElementCount : unmanaged, IBinaryInteger<TElementCount>
        => Clear(ref Unsafe.AsRef<T>(items), elementCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Clear<T, TElementCount>(ref T items, TElementCount elementCount)
        where TElementCount : unmanaged, IBinaryInteger<TElementCount>
    {
        ValidateElementCountType<TElementCount>();

        if (typeof(TElementCount) == typeof(sbyte) || typeof(TElementCount) == typeof(byte))
        {
            MemoryMarshal.CreateSpan(ref items, Unsafe.BitCast<TElementCount, byte>(elementCount)).Clear();
            return;
        }

        if (typeof(TElementCount) == typeof(short) || typeof(TElementCount) == typeof(ushort))
        {
            MemoryMarshal.CreateSpan(ref items, Unsafe.BitCast<TElementCount, ushort>(elementCount)).Clear();
            return;
        }

        if (typeof(TElementCount) == typeof(int) || (sizeof(int) == sizeof(nint) && typeof(TElementCount) == typeof(nint)))
        {
            MemoryMarshal.CreateSpan(ref items, Unsafe.BitCast<TElementCount, int>(elementCount)).Clear();
            return;
        }

        if (typeof(TElementCount) == typeof(uint) || (sizeof(uint) == sizeof(nuint) && typeof(TElementCount) == typeof(nuint)))
        {
            uint count = Unsafe.BitCast<TElementCount, uint>(elementCount);
            if (count > int.MaxValue)
            {
                MemoryMarshal.CreateSpan(ref items, int.MaxValue).Clear();
                items = ref Unsafe.Add(ref items, int.MaxValue);
                count -= int.MaxValue;
            }

            MemoryMarshal.CreateSpan(ref items, (int)count).Clear();
            return;
        }

        if (typeof(TElementCount) == typeof(long) || typeof(TElementCount) == typeof(nint) ||
            typeof(TElementCount) == typeof(ulong) || typeof(TElementCount) == typeof(nuint))
        {
            ulong count = Unsafe.BitCast<TElementCount, ulong>(elementCount);
            if (count <= int.MaxValue)
            {
                Clear(ref items, (int)count);
                return;
            }

            do
            {
                Clear(ref items, int.MaxValue);
                items = ref Unsafe.Add(ref items, int.MaxValue);
                count -= int.MaxValue;
            }
            while (count >= int.MaxValue);

            if (count != 0)
            {
                Clear(ref items, (int)count);
            }

            return;
        }

        ThrowHelper.ThrowUnreachableException();
    }
}
