using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HLE.Marshalling;

public static unsafe class RawDataMarshal
{
    public static uint BaseObjectSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (uint)sizeof(nuint) * 2; // object header + method table
    }

    private static readonly delegate*<object, nint> s_getRawObjectSize = (delegate*<object, nint>)typeof(RuntimeHelpers)
        .GetMethod("GetRawObjectDataSize", BindingFlags.NonPublic | BindingFlags.Static)!
        .MethodHandle
        .GetFunctionPointer();

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawObjectSize(object obj)
        => (nuint)s_getRawObjectSize(obj);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nuint GetMethodTableReference<T>(T obj) where T : class
        => ref Unsafe.AsRef<nuint>(GetMethodTablePointer(obj));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint* GetMethodTablePointer<T>(T obj) where T : class
        => *(nuint**)&obj;

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
        => ref Unsafe.AsRef<RawStringData>(*(RawStringData**)&str);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawStringSize(int stringLength) =>
        BaseObjectSize +
        sizeof(int) /* string length */ +
        (uint)stringLength * sizeof(char) /* chars */ +
        sizeof(char) /* zero-char */;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RawArrayData GetRawArrayData<T>(T[] array)
        => ref Unsafe.AsRef<RawArrayData>(*(RawArrayData**)&array);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawArraySize<T>(int arrayLength) =>
        BaseObjectSize +
        (nuint)sizeof(nuint) /* array length */ +
        (uint)arrayLength * (uint)sizeof(T); /* items */
}
