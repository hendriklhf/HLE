using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE;

[SuppressMessage("Design", "CA1024:Use properties where appropriate")]
public static class EnumValues<TEnum> where TEnum : struct, Enum
{
    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "exactly what i want")]
    private static readonly TEnum[] s_values = Enum.GetValues<TEnum>();
    private static readonly TEnum s_maximumValue = s_values[^1];

    [Pure]
    public static ReadOnlySpan<TEnum> GetValues() => s_values;

    [Pure]
    public static unsafe ReadOnlySpan<TUnderlyingType> GetValuesAs<TUnderlyingType>()
        where TUnderlyingType : unmanaged
    {
        if (sizeof(TEnum) != sizeof(TUnderlyingType))
        {
            ThrowDifferentInstanceSize(typeof(TEnum), typeof(TUnderlyingType));
        }

        ReadOnlySpan<TEnum> values = GetValues();
        ref TEnum valuesReference = ref MemoryMarshal.GetReference(values);
        ref TUnderlyingType underlyingTypeReference = ref Unsafe.As<TEnum, TUnderlyingType>(ref valuesReference);
        return MemoryMarshal.CreateReadOnlySpan(ref underlyingTypeReference, values.Length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetValueCount() => GetValues().Length;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum GetMaximumValue() => s_maximumValue;

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDifferentInstanceSize(Type enumType, Type underlyingType)
        => throw new InvalidOperationException($"{enumType} and {underlyingType} have different instance sizes, so {underlyingType} can't be an underlying type.");
}
