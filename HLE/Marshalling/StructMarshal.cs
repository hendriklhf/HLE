using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static unsafe class StructMarshal<T> where T : struct
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetBytes(ref T item)
        => MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref item), sizeof(T));

    /// <inheritdoc cref="EqualsBitwise{TOther}(ref T,ref TOther)"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TOther>(T left, TOther right) where TOther : struct
        => EqualsBitwise(ref left, ref right);

    /// <summary>
    /// Compares two structs bitwise, even if they might not be bitwise equatable, because they contain managed types.
    /// </summary>
    /// <param name="left">A struct.</param>
    /// <param name="right">Another struct.</param>
    /// <typeparam name="TOther">The type of the other struct.</typeparam>
    /// <returns>True, if they have the same bitwise memory layout, otherwise false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsBitwise<TOther>(ref T left, ref TOther right) where TOther : struct
    {
        if (sizeof(T) != sizeof(TOther))
        {
            return false;
        }

        switch (sizeof(T))
        {
            case sizeof(byte):
                return Unsafe.As<T, byte>(ref left) == Unsafe.As<TOther, byte>(ref right);
            case sizeof(short):
                return Unsafe.As<T, short>(ref left) == Unsafe.As<TOther, short>(ref right);
            case sizeof(int):
                return Unsafe.As<T, int>(ref left) == Unsafe.As<TOther, int>(ref right);
            case sizeof(long):
                return Unsafe.As<T, long>(ref left) == Unsafe.As<TOther, long>(ref right);
        }

        if (Unsafe.AreSame(ref Unsafe.As<T, byte>(ref left), ref Unsafe.As<TOther, byte>(ref right)))
        {
            return true;
        }

        ReadOnlySpan<byte> leftBytes = GetBytes(ref left);
        ReadOnlySpan<byte> rightBytes = StructMarshal<TOther>.GetBytes(ref right);
        return leftBytes.SequenceEqual(rightBytes);
    }
}
