using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HLE.Marshalling;

public static unsafe class RawDataMarshal
{
    private static readonly delegate*<object, nint> s_getRawDataSize = (delegate*<object, nint>)typeof(RuntimeHelpers)
        .GetMethod("GetRawObjectDataSize", BindingFlags.NonPublic | BindingFlags.Static)!
        .MethodHandle
        .GetFunctionPointer();

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawDataSize<T>(T obj) where T : class
        => (nuint)s_getRawDataSize(obj);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nuint GetMethodTableReference<T>(T obj) where T : class
        => ref Unsafe.AsRef<nuint>(GetMethodTablePointer(obj));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint* GetMethodTablePointer<T>(T obj) where T : class
        => (nuint*)*(nuint*)&obj;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(ref byte methodTableReference) where T : class
    {
        nuint* pointer = (nuint*)Unsafe.AsPointer(ref methodTableReference);
        return ReadObject<T>(pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T, TRef>(ref TRef methodTableReference) where T : class
    {
        nuint* pointer = (nuint*)Unsafe.AsPointer(ref methodTableReference);
        return ReadObject<T>(pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(void* methodTablePointer) where T : class
        => ReadObject<T>((nuint)methodTablePointer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(nuint methodTablePointer) where T : class
        => *(T*)&methodTablePointer;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RawStringData GetRawStringData(string str)
        => ref Unsafe.As<nuint, RawStringData>(ref Unsafe.AsRef<nuint>(GetMethodTablePointer(str)));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawStringSize(int stringLength) =>
        (nuint)sizeof(nuint) /* object header */ +
        (nuint)sizeof(nuint) /* method table pointer */ +
        sizeof(int) /* string length */ +
        (nuint)(stringLength * sizeof(char)) /* chars */ +
        sizeof(char) /* zero-char */;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RawArrayData<T> GetRawArrayData<T>(T[] array)
        => ref Unsafe.As<nuint, RawArrayData<T>>(ref Unsafe.AsRef<nuint>(GetMethodTablePointer(array)));
}
