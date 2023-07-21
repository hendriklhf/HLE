using System;
using System.Runtime.CompilerServices;
using HLE.Memory;

namespace HLE.Marshals;

public static class NativeMemoryMarshal<T> where T : unmanaged, IEquatable<T>
{
    public static unsafe T* GetPointer(NativeMemory<T> nativeMemory)
    {
        return nativeMemory._pointer;
    }

    public static unsafe ref T GetManagedPointer(NativeMemory<T> nativeMemory)
    {
        return ref Unsafe.AsRef<T>(nativeMemory._pointer);
    }
}
