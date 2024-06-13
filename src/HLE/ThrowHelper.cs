using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HLE;

internal static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowObjectDisposedException<T>() where T : IDisposable
        => throw new ObjectDisposedException(typeof(T).FullName);

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

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowCalledCollectionBuilderConstructor<TCollectionBuilder>()
        => throw new NotSupportedException($"{typeof(TCollectionBuilder)} should not be instantiated. It only has static method that are used for building collections.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowTaskCancelledException() => throw new TaskCanceledException();

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowOperatingSystemNotSupported()
        => throw new NotSupportedException("The current operating system is not yet supported.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNotSupportedException(string message) => throw new NotSupportedException(message);
}
