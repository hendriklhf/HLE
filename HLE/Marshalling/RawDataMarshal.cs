using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HLE.Marshalling;

public static unsafe class RawDataMarshal
{
    private static readonly delegate*<object, nint> _getRawDataSize = (delegate*<object, nint>)typeof(RuntimeHelpers).GetMethod("GetRawObjectDataSize", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawDataSize<T>(T obj) where T : class
        => (nuint)_getRawDataSize(obj);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nuint GetMethodTablePointer<T>(T obj) where T : class
    {
        nuint* pointer = (nuint*)*(nuint*)&obj;
        return ref Unsafe.AsRef<nuint>(pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(ref byte reference) where T : class
    {
        nuint* pointer = (nuint*)Unsafe.AsPointer(ref reference);
        return ReadObject<T>(pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T, TRef>(ref TRef reference) where T : class
    {
        nuint* pointer = (nuint*)Unsafe.AsPointer(ref reference);
        return ReadObject<T>(pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(void* rawDataPointer) where T : class
        => ReadObject<T>((nuint)rawDataPointer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(nuint rawDataPointer) where T : class
        => *(T*)&rawDataPointer;
}
