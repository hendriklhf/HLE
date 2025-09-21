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
    public static void ThrowObjectDisposedException<T>()
#if NET9_0_OR_GREATER
        where T : IDisposable, allows ref struct
#else
        where T : IDisposable
#endif
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
    [SuppressMessage("Minor Code Smell", "S3717:Track use of \"NotImplementedException\"", Justification = "it is implemented")]
    [SuppressMessage("Roslynator", "RCS1079:Throwing of new NotImplementedException", Justification = "it is implemented")]
    public static void ThrowNotImplementedException() => throw new NotImplementedException();

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
    public static void ThrowTypeNotSupported<T>()
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        => throw new NotSupportedException($"The type \"{typeof(T)}\" is not supported.");

    [DoesNotReturn]
    public static void ThrowNotSupportedException(string? message = null) => throw new NotSupportedException(message);

    [DoesNotReturn]
    public static void ThrowInvalidOperationException(string? message = null) => throw new InvalidOperationException(message);
}
