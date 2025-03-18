using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace HLE;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowObjectDisposedException<T>() where T : IDisposable, allows ref struct
        => throw new ObjectDisposedException(typeof(T).FullName);

    [DoesNotReturn]
    public static void ThrowObjectDisposedException(Type type)
    {
        Debug.Assert(type.IsAssignableTo(typeof(IDisposable)));
        throw new ObjectDisposedException(type.FullName);
    }

    [DoesNotReturn]
    public static void ThrowUnreachableException() => throw new UnreachableException();

    [DoesNotReturn]
    public static void ThrowInvalidEnumValue<TEnum>(TEnum value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where TEnum : struct, Enum
    {
        Debug.Assert(typeof(TEnum).GetEnumUnderlyingType() == typeof(int));
        throw new InvalidEnumArgumentException(paramName, Unsafe.BitCast<TEnum, int>(value), typeof(TEnum));
    }

    [DoesNotReturn]
    public static void ThrowCalledCollectionBuilderConstructor<TCollectionBuilder>()
        => throw new NotSupportedException($"{typeof(TCollectionBuilder)} should not be instantiated. It only has static method that are used by the compiler for building collections.");

    [DoesNotReturn]
    public static void ThrowOperationCanceledException(CancellationToken token) => throw new OperationCanceledException(token);

    [DoesNotReturn]
    public static void ThrowOperatingSystemNotSupported()
        => throw new NotSupportedException("The current operating system is not supported.");

    [DoesNotReturn]
    public static void ThrowNotSupportedException(string? message = null) => throw new NotSupportedException(message);

    [DoesNotReturn]
    public static void ThrowInvalidOperationException(string? message = null) => throw new InvalidOperationException(message);
}
