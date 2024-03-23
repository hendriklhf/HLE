using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using HLE.IL;

namespace HLE.Marshalling;

public static unsafe class ObjectMarshal
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
    /// Gets the amount of bytes allocated for the instance of the object.
    /// </summary>
    /// <param name="obj">The object whose instance size will be returned.</param>
    /// <returns>The amount of bytes allocated for the object.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetObjectSize(object obj)
        => BaseObjectSize + (nuint)s_getRawObjectSize(obj);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TRef GetMethodTableReference<TRef>(object obj)
        => ref UnsafeIL.AsRef<object, TRef>(obj);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref nuint GetMethodTableReference(object obj)
        => ref UnsafeIL.AsRef<object, nuint>(obj);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TPointer* GetMethodTablePointer<TPointer>(object obj)
        => *(TPointer**)&obj;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint* GetMethodTablePointer(object obj)
        => *(nuint**)&obj;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodTable* GetMethodTable(object obj)
        => **(MethodTable***)&obj;

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadObject<T>(ref nuint methodTablePointer) where T : class
        => UnsafeIL.RefAs<nuint, T>(ref methodTablePointer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TObject ReadObject<TObject, TRef>(ref TRef methodTableReference) where TObject : class
        => UnsafeIL.RefAs<TRef, TObject>(ref methodTableReference);

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
        => UnsafeIL.As<nuint, T>(methodTablePointer);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetField<T>(object obj, nuint byteOffset) => ref UnsafeIL.GetField<T>(obj, byteOffset + (uint)sizeof(nuint));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref RawStringData GetRawStringData(string str) => ref UnsafeIL.AsRef<string, RawStringData>(str);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString(ref RawStringData data) => ReadObject<string, RawStringData>(ref data);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString(RawStringData* data) => ReadObject<string>(data);

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
    public static ref RawArrayData GetRawArrayData<T>(T[] array) => ref UnsafeIL.AsRef<T[], RawArrayData>(array);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetArray<T>(ref RawArrayData data) => ReadObject<T[], RawArrayData>(ref data);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] GetArray<T>(RawArrayData* data) => ReadObject<T[]>(data);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawArraySize<T>(T[] array) => GetRawArraySize<T>(array.Length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint GetRawArraySize<T>(int arrayLength) =>
        BaseObjectSize +
        (nuint)sizeof(nuint) /* array length */ +
        (uint)arrayLength * (uint)sizeof(T); /* items */

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodTable* GetMethodTable<T>() => GetMethodTable(typeof(T));

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodTable* GetMethodTable(Type type) => (MethodTable*)type.TypeHandle.Value;
}
