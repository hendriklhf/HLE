using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE;

public static class EnumValues<TEnum> where TEnum : struct, Enum
{
    // ⚠️ this array is the runtime's internal value storage.
    // do not mutate it :) ⚠️
    private static readonly TEnum[] s_values = EnumValues.GetValues<TEnum>();

    public static int Count => s_values.Length;

    internal static ref TEnum Reference => ref MemoryMarshal.GetArrayDataReference(s_values);

    [Pure]
    public static ReadOnlySpan<TEnum> AsSpan() => s_values;

    [Pure]
    public static unsafe ReadOnlySpan<TUnderlyingType> AsSpan<TUnderlyingType>()
        where TUnderlyingType : unmanaged
    {
        if (sizeof(TEnum) != sizeof(TUnderlyingType))
        {
            ThrowDifferentInstanceSize(typeof(TEnum), typeof(TUnderlyingType));
        }

        ReadOnlySpan<TEnum> values = AsSpan();
        return Unsafe.As<ReadOnlySpan<TEnum>, ReadOnlySpan<TUnderlyingType>>(ref values);

        [DoesNotReturn]
        static void ThrowDifferentInstanceSize(Type enumType, Type underlyingType)
            => throw new InvalidOperationException(
                $"{enumType} and {underlyingType} have different instance sizes, so {underlyingType} can't be an underlying type of {enumType}."
            );
    }

    [Pure]
    public static ReadOnlyMemory<TEnum> AsMemory() => s_values;

    [Pure]
    public static ImmutableArray<TEnum> AsImmutableArray() => ImmutableCollectionsMarshal.AsImmutableArray(s_values);

    [Pure]
    public static IEnumerable<TEnum> AsEnumerable() => s_values;

    [Pure]
    public static unsafe bool IsDefined(TEnum value)
    {
        switch (sizeof(TEnum))
        {
            case sizeof(byte):
                return AsSpan<byte>().Contains(Unsafe.BitCast<TEnum, byte>(value));
            case sizeof(ushort):
                return AsSpan<ushort>().Contains(Unsafe.BitCast<TEnum, ushort>(value));
            case sizeof(uint):
                return AsSpan<uint>().Contains(Unsafe.BitCast<TEnum, uint>(value));
            case sizeof(ulong):
                return AsSpan<ulong>().Contains(Unsafe.BitCast<TEnum, ulong>(value));
            default:
                ThrowHelper.ThrowUnreachableException();
                return false;
        }
    }
}
