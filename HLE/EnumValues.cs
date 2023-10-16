using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE;

public static class EnumValues<TEnum> where TEnum : struct, Enum
{
    [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "exactly what i want")]
    private static byte[]? _bytes;

    [Pure]
    public static unsafe ReadOnlySpan<TEnum> GetValues()
    {
        byte[]? bytes = _bytes;
        if (bytes is null)
        {
            return GetAndCacheValues();
        }

        ref byte bytesReference = ref MemoryMarshal.GetArrayDataReference(bytes);
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, TEnum>(ref bytesReference), bytes.Length / sizeof(TEnum));
    }

    [Pure]
    public static unsafe ReadOnlySpan<TUnderlyingType> GetValuesAsUnderlyingType<TUnderlyingType>()
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
    public static TEnum GetMaxValue() => GetValues()[^1];

    private static unsafe ReadOnlySpan<TEnum> GetAndCacheValues()
    {
        ReadOnlySpan<TEnum> values = Enum.GetValues<TEnum>();
        ref TEnum valuesReference = ref MemoryMarshal.GetReference(values);
        ref byte valueBytesReference = ref Unsafe.As<TEnum, byte>(ref valuesReference);

        int byteCount = values.Length * sizeof(TEnum);
        byte[] bytes = GC.AllocateUninitializedArray<byte>(byteCount, true);

        ref byte cacheBytesReference = ref MemoryMarshal.GetArrayDataReference(bytes);
        CopyWorker<byte> copyWorker = new(ref valueBytesReference, byteCount);
        copyWorker.CopyTo(ref cacheBytesReference);

        _bytes = bytes;
        return values;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDifferentInstanceSize(Type enumType, Type underlyingType)
        => throw new InvalidOperationException($"{enumType} and {underlyingType} have different instance sizes, so {underlyingType} can't be the underlying type.");
}
