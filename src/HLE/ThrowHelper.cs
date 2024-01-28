using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE;

internal static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowObjectDisposedException<T>() => throw new ObjectDisposedException(typeof(T).FullName);

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowObjectDisposedException(Type type) => throw new ObjectDisposedException(type.FullName);

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowUnreachableException() => throw new UnreachableException();

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static unsafe void ThrowInvalidEnumValue<TEnum>(TEnum value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where TEnum : struct, Enum
    {
        Debug.Assert(sizeof(TEnum) == sizeof(int));
        throw new InvalidEnumArgumentException(paramName, Unsafe.As<TEnum, int>(ref value), typeof(TEnum));
    }
}
