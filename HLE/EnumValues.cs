using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE;

public static class EnumValues
{
    private static readonly Dictionary<Type, byte[]> _valuesCache = new();

    public static unsafe ReadOnlySpan<TEnum> GetValues<TEnum>() where TEnum : struct, Enum
    {
        ref byte[]? bytes = ref CollectionsMarshal.GetValueRefOrAddDefault(_valuesCache, typeof(TEnum), out bool exists);
        if (exists)
        {
            ref byte bytesReference = ref MemoryMarshal.GetArrayDataReference(bytes!);
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, TEnum>(ref bytesReference), bytes!.Length / sizeof(TEnum));
        }

        ReadOnlySpan<TEnum> values = Enum.GetValues<TEnum>();
        ref TEnum valuesReference = ref MemoryMarshal.GetReference(values);
        ref byte valueBytesReference = ref Unsafe.As<TEnum, byte>(ref valuesReference);

        int byteCount = values.Length * sizeof(TEnum);
        bytes = new byte[byteCount];

        ref byte cacheBytesReference = ref MemoryMarshal.GetArrayDataReference(bytes);
        CopyWorker<byte> copyWorker = new(ref valueBytesReference, byteCount);
        copyWorker.CopyTo(ref cacheBytesReference);

        return values;
    }

    public static unsafe ReadOnlySpan<TUnderlyingType> GetValuesAsUnderlyingType<TEnum, TUnderlyingType>()
        where TEnum : struct, Enum
        where TUnderlyingType : struct
    {
        if (sizeof(TEnum) != sizeof(TUnderlyingType))
        {
            ThrowDifferentInstanceSize(typeof(TEnum), typeof(TUnderlyingType));
        }

        ReadOnlySpan<TEnum> values = GetValues<TEnum>();
        ref TEnum valuesReference = ref MemoryMarshal.GetReference(values);
        ref TUnderlyingType underlyingTypeReference = ref Unsafe.As<TEnum, TUnderlyingType>(ref valuesReference);
        return MemoryMarshal.CreateReadOnlySpan(ref underlyingTypeReference, values.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetValueCount<TEnum>() where TEnum : struct, Enum
    {
        return GetValues<TEnum>().Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum GetMaxValue<TEnum>() where TEnum : struct, Enum
    {
        return GetValues<TEnum>()[^1];
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDifferentInstanceSize(Type enumType, Type underlyingType)
    {
        throw new InvalidOperationException($"{enumType} and {underlyingType} have different instance sizes.");
    }
}
