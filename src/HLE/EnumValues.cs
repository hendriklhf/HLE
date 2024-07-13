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
    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "exactly what i want")]
    private static readonly TEnum[] s_values = Enum.GetValues<TEnum>();

    public static int Count => s_values.Length;

    public static TEnum MaximumValue { get; } = s_values[^1];

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
        ref TEnum valuesReference = ref MemoryMarshal.GetReference(values);
        ref TUnderlyingType underlyingTypeReference = ref Unsafe.As<TEnum, TUnderlyingType>(ref valuesReference);
        return MemoryMarshal.CreateReadOnlySpan(ref underlyingTypeReference, Count);
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
                return AsSpan<byte>().Contains(Unsafe.As<TEnum, byte>(ref value));
            case sizeof(ushort):
                return AsSpan<ushort>().Contains(Unsafe.As<TEnum, ushort>(ref value));
            case sizeof(uint):
                return AsSpan<uint>().Contains(Unsafe.As<TEnum, uint>(ref value));
            case sizeof(ulong):
                return AsSpan<ulong>().Contains(Unsafe.As<TEnum, ulong>(ref value));
            default:
                ThrowHelper.ThrowUnreachableException();
                return false;
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDifferentInstanceSize(Type enumType, Type underlyingType)
        => throw new InvalidOperationException(
            $"{enumType} and {underlyingType} have different instance sizes, so {underlyingType} can't be an underlying type of {enumType}."
        );
}
