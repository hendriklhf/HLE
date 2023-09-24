using System;
using System.Runtime.CompilerServices;
using HLE.Memory;

namespace HLE.Marshalling;

public static class NativeMemoryMarshal<T> where T : unmanaged, IEquatable<T>
{
    public static unsafe T* GetPointer(NativeMemory<T> nativeMemory)
    {
        return nativeMemory.Pointer;
    }

    public static unsafe ref T GetReference(NativeMemory<T> nativeMemory)
    {
        return ref Unsafe.AsRef<T>(nativeMemory.Pointer);
    }
}
