using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static unsafe class StructMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetBytes<T>(ref T item) where T : struct
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref item), sizeof(T));
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TLeft, TRight>(TLeft left, TRight right) where TLeft : struct where TRight : struct
    {
        return EqualsBitwise(ref left, ref right);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TLeft, TRight>(ref TLeft left, ref TRight right) where TLeft : struct where TRight : struct
    {
        if (sizeof(TLeft) != sizeof(TRight))
        {
            return false;
        }

        switch (sizeof(TLeft))
        {
            case sizeof(byte):
                return Unsafe.As<TLeft, byte>(ref left) == Unsafe.As<TRight, byte>(ref right);
            case sizeof(short):
                return Unsafe.As<TLeft, short>(ref left) == Unsafe.As<TRight, short>(ref right);
            case sizeof(int):
                return Unsafe.As<TLeft, int>(ref left) == Unsafe.As<TRight, int>(ref right);
            case sizeof(long):
                return Unsafe.As<TLeft, long>(ref left) == Unsafe.As<TRight, long>(ref right);
        }

        if (Unsafe.AreSame(ref Unsafe.As<TLeft, byte>(ref left), ref Unsafe.As<TRight, byte>(ref right)))
        {
            return true;
        }

        ReadOnlySpan<byte> leftBytes = GetBytes(ref left);
        ReadOnlySpan<byte> rightBytes = GetBytes(ref right);
        return leftBytes.SequenceEqual(rightBytes);
    }
}
