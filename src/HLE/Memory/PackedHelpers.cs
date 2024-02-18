using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public static unsafe class PackedHelpers
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short CreateInt16(byte lower, byte upper) => PackTwo<byte, short>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short CreateInt16(sbyte lower, sbyte upper) => PackTwo<sbyte, short>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort CreateUInt16(byte lower, byte upper) => PackTwo<byte, ushort>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort CreateUInt16(sbyte lower, sbyte upper) => PackTwo<sbyte, ushort>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CreateInt32(byte lower0, byte lower1, byte upper0, byte upper1) => PackFour<byte, int>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CreateInt32(sbyte lower0, sbyte lower1, sbyte upper0, sbyte upper1) => PackFour<sbyte, int>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CreateInt32(short lower, short upper) => PackTwo<short, int>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CreateInt32(ushort lower, ushort upper) => PackTwo<ushort, int>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CreateUInt32(byte lower0, byte lower1, byte upper0, byte upper1) => PackFour<byte, uint>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CreateUInt32(sbyte lower0, sbyte lower1, sbyte upper0, sbyte upper1) => PackFour<sbyte, uint>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CreateUInt32(short lower, short upper) => PackTwo<short, uint>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CreateUInt32(ushort lower, ushort upper) => PackTwo<ushort, uint>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CreateInt64(byte lower0, byte lower1, byte lower2, byte lower3, byte upper0, byte upper1, byte upper2, byte upper3)
        => PackEight<byte, long>(lower0, lower1, lower2, lower3, upper0, upper1, upper2, upper3);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CreateInt64(sbyte lower0, sbyte lower1, sbyte lower2, sbyte lower3, sbyte upper0, sbyte upper1, sbyte upper2, sbyte upper3)
        => PackEight<sbyte, long>(lower0, lower1, lower2, lower3, upper0, upper1, upper2, upper3);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CreateInt64(short lower0, short lower1, short upper0, short upper1)
        => PackFour<short, long>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CreateInt64(ushort lower0, ushort lower1, ushort upper0, ushort upper1)
        => PackFour<ushort, long>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CreateInt64(int lower, int upper) => PackTwo<int, long>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CreateInt64(uint lower, uint upper) => PackTwo<uint, long>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateUInt64(byte lower0, byte lower1, byte lower2, byte lower3, byte upper0, byte upper1, byte upper2, byte upper3)
        => PackEight<byte, ulong>(lower0, lower1, lower2, lower3, upper0, upper1, upper2, upper3);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateUInt64(sbyte lower0, sbyte lower1, sbyte lower2, sbyte lower3, sbyte upper0, sbyte upper1, sbyte upper2, sbyte upper3)
        => PackEight<sbyte, ulong>(lower0, lower1, lower2, lower3, upper0, upper1, upper2, upper3);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateUInt64(short lower0, short lower1, short upper0, short upper1)
        => PackFour<short, ulong>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateUInt64(ushort lower0, ushort lower1, ushort upper0, ushort upper1)
        => PackFour<ushort, ulong>(lower0, lower1, upper0, upper1);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateUInt64(int lower, int upper) => PackTwo<int, ulong>(lower, upper);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateUInt64(uint lower, uint upper) => PackTwo<uint, ulong>(lower, upper);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TResult PackTwo<TElement, TResult>(TElement lower, TElement upper)
        where TElement : unmanaged
        where TResult : unmanaged
    {
        Debug.Assert(sizeof(TElement) * 2 == sizeof(TResult));

        TResult result = default;
        TElement* elements = (TElement*)&result;

        if (BitConverter.IsLittleEndian)
        {
            elements[0] = lower;
            elements[1] = upper;
        }
        else
        {
            elements[1] = lower;
            elements[0] = upper;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TResult PackFour<TElement, TResult>(TElement lower0, TElement lower1, TElement upper0, TElement upper1)
        where TElement : unmanaged
        where TResult : unmanaged
    {
        Debug.Assert(sizeof(TElement) * 4 == sizeof(TResult));

        TResult result = default;
        TElement* elements = (TElement*)&result;

        if (BitConverter.IsLittleEndian)
        {
            elements[0] = lower0;
            elements[1] = lower1;
            elements[2] = upper0;
            elements[3] = upper1;
        }
        else
        {
            elements[3] = lower0;
            elements[2] = lower1;
            elements[1] = upper0;
            elements[0] = upper1;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TResult PackEight<TElement, TResult>(
        TElement lower0, TElement lower1, TElement lower2, TElement lower3,
        TElement upper0, TElement upper1, TElement upper2, TElement upper3
    )
        where TElement : unmanaged
        where TResult : unmanaged
    {
        Debug.Assert(sizeof(TElement) * 8 == sizeof(TResult));

        TResult result = default;
        TElement* elements = (TElement*)&result;

        if (BitConverter.IsLittleEndian)
        {
            elements[0] = lower0;
            elements[1] = lower1;
            elements[2] = lower2;
            elements[3] = lower3;
            elements[4] = upper0;
            elements[5] = upper1;
            elements[6] = upper2;
            elements[7] = upper3;
        }
        else
        {
            elements[7] = lower0;
            elements[6] = lower1;
            elements[5] = lower2;
            elements[4] = lower3;
            elements[3] = upper0;
            elements[2] = upper1;
            elements[1] = upper2;
            elements[0] = upper3;
        }

        return result;
    }
}
