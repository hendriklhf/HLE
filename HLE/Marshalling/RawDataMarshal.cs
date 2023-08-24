using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static unsafe class RawDataMarshal
{
    private static readonly delegate*<object, nint> _getRawDataSize = (delegate*<object, nint>)typeof(RuntimeHelpers).GetMethod("GetRawObjectDataSize", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    // returns raw data size without method table pointer
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawDataSize<T>(T obj) where T : class
    {
        return (nuint)_getRawDataSize(obj);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> GetRawData<T>(T obj) where T : class
    {
        ref byte rawDataReference = ref GetRawDataReference(obj);
        nuint rawDataSize = GetRawDataSize(obj);
        return MemoryMarshal.CreateSpan(ref rawDataReference, (int)rawDataSize);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nuint GetMethodTablePointer<T>(T obj) where T : class
    {
        nuint pointer = *(nuint*)&obj;
        return ref Unsafe.AsRef<nuint>((void*)pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref byte GetRawDataReference<T>(T obj) where T : class
    {
        ref nuint methodTable = ref GetMethodTablePointer(obj);
        ref nuint rawDataPointer = ref Unsafe.Add(ref methodTable, 1);
        return ref Unsafe.As<nuint, byte>(ref rawDataPointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetObjectFromRawData<T>(ReadOnlySpan<byte> rawData) where T : class
    {
        return GetObjectFromRawData<T>(ref MemoryMarshal.GetReference(rawData));
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetObjectFromRawData<T>(ref byte rawDataReference) where T : class
    {
        T* pointer = (T*)Unsafe.AsPointer(ref rawDataReference);
        return GetObjectFromRawData<T>((nuint)pointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetObjectFromRawData<T>(void* rawDataPointer) where T : class
    {
        return GetObjectFromRawData<T>((nuint)rawDataPointer);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetObjectFromRawData<T>(nuint rawDataPointer) where T : class
    {
        rawDataPointer -= (nuint)sizeof(nuint);
        return *(T*)&rawDataPointer;
    }
}
