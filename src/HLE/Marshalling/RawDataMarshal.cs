using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HLE.Marshalling;

public static unsafe class RawDataMarshal
{
    public static uint BaseObjectSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (uint)(sizeof(nuint) + sizeof(nuint)); // object header + method table
    }

    private static readonly delegate*<object, nint> s_getRawObjectSize = (delegate*<object, nint>)typeof(RuntimeHelpers)
        .GetMethod("GetRawObjectDataSize", BindingFlags.NonPublic | BindingFlags.Static)!
        .MethodHandle
        .GetFunctionPointer();

    /// <summary>
    /// Gets the amount of bytes allocated for every instance of the object.
    /// </summary>
    /// <param name="obj">The object whose instance size will be returned.</param>
    /// <returns>The amount of bytes allocated for the object.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawObjectSize(object obj)
        => BaseObjectSize + (nuint)s_getRawObjectSize(obj);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TRef GetMethodTableReference<TObject, TRef>(TObject obj) where TObject : class
        => ref Unsafe.AsRef<TRef>(GetMethodTablePointer(obj));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nuint GetMethodTableReference<T>(T obj) where T : class
        => ref Unsafe.AsRef<nuint>(GetMethodTablePointer(obj));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TPointer* GetMethodTablePointer<TObject, TPointer>(TObject obj) where TObject : class
        => *(TPointer**)&obj;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint* GetMethodTablePointer<T>(T obj) where T : class
        => *(nuint**)&obj;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodTable* GetMethodTable<T>(T obj) where T : class
        => **(MethodTable***)&obj;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(ref nuint methodTablePointer) where T : class
    {
        nuint ptr = (nuint)Unsafe.AsPointer(ref methodTablePointer);
        return ReadObject<T>(ptr);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TObject ReadObject<TObject, TRef>(ref TRef methodTableReference) where TObject : class
    {
        nuint ptr = (nuint)Unsafe.AsPointer(ref methodTableReference);
        return ReadObject<TObject>(ptr);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(void* methodTablePointer) where T : class
        => ReadObject<T>((nuint)methodTablePointer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(MethodTable** methodTablePointer) where T : class
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
    public static nuint GetRawStringSize(string str) => GetRawStringSize(str.Length);

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
    public static nuint GetRawArraySize<T>(T[] array) => GetRawArraySize<T>(array.Length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawArraySize<T>(int arrayLength) =>
        BaseObjectSize +
        (nuint)sizeof(nuint) /* array length */ +
        (uint)arrayLength * (uint)sizeof(T); /* items */
}
