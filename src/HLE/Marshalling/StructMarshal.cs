using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Marshalling;

public static unsafe class StructMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetBytes<T>(ref T item) where T : struct
        => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref item), sizeof(T));

    /// <inheritdoc cref="EqualsBitwise{TLeft,TRight}(ref TLeft,ref TRight)"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TLeft, TRight>(TLeft left, TRight right)
        where TLeft : struct
        where TRight : struct
        => EqualsBitwise(ref left, ref right);

    /// <summary>
    /// Compares two structs bitwise, even if they might not be bitwise equatable, because they contain managed types.
    /// </summary>
    /// <param name="left">A struct.</param>
    /// <param name="right">Another struct.</param>
    /// <typeparam name="TLeft">The type of the left struct.</typeparam>
    /// <typeparam name="TRight">The type of the right struct.</typeparam>
    /// <returns>True, if they have the same bitwise memory layout, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TLeft, TRight>(ref TLeft left, ref TRight right)
        where TLeft : struct
        where TRight : struct
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

    [Pure]
    public static bool IsBitwiseEquatable<T>() // where T : unmanaged
        => typeof(T) == typeof(byte) ||
           typeof(T) == typeof(sbyte) ||
           typeof(T) == typeof(short) ||
           typeof(T) == typeof(ushort) ||
           typeof(T) == typeof(int) ||
           typeof(T) == typeof(uint) ||
           typeof(T) == typeof(long) ||
           typeof(T) == typeof(ulong) ||
           typeof(T) == typeof(nint) ||
           typeof(T) == typeof(nuint) ||
           typeof(T) == typeof(Int128) ||
           typeof(T) == typeof(UInt128) ||
           typeof(T) == typeof(char) ||
           typeof(T).IsEnum ||
           typeof(T).IsAssignableTo(typeof(IBitwiseEquatable<T>));
}
