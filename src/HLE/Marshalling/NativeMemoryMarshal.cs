using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

public static unsafe class NativeMemoryMarshal
{
    [Pure]
    public static T* Alloc<T>() where T : struct, allows ref struct
    {
        T* ptr = (T*)NativeMemory.AlignedAlloc((uint)sizeof(T), (uint)sizeof(nuint));
        Unsafe.InitBlock(ptr, 0, (uint)sizeof(T));
        return ptr;
    }

    public static void Free<T>(T* ptr) where T : struct, allows ref struct
        => NativeMemory.AlignedFree(ptr);

    [Pure]
    public static T AllocObject<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>()
        where T : class
    {
        EnsureAllocatableType<T>();

        nuint allocationSize = ObjectMarshal.GetObjectSize<T>();
        nuint* memory = (nuint*)NativeMemory.AlignedAlloc(allocationSize, (uint)sizeof(nuint));
        *memory = 0;
        memory++;
        *memory = (nuint)ObjectMarshal.GetMethodTable<T>();
        Unsafe.InitBlock(memory + 1, 0, (uint)(allocationSize - ObjectMarshal.BaseObjectSize));
        return ObjectMarshal.ReadObject<T>(memory);
    }

    public static void FreeObject<T>(T obj)
        where T : class
    {
        nuint* memory = ObjectMarshal.GetMethodTablePointer<nuint>(obj);
        NativeMemory.AlignedFree(memory - 1);
    }

    private static void EnsureAllocatableType<T>() where T : allows ref struct
    {
        if (typeof(T).IsArray || typeof(T) == typeof(string) || typeof(T).IsInterface || typeof(T).IsAbstract)
        {
            Throw();
        }

        return;

        [DoesNotReturn]
        static void Throw() => throw new InvalidOperationException("Types with undefined sizes can't be allocated.");
    }
}
