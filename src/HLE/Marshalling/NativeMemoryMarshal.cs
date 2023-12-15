using System;
using HLE.Memory;

namespace HLE.Marshalling;

public static class NativeMemoryMarshal<T> where T : unmanaged, IEquatable<T>
{
    public static unsafe T* GetPointer(NativeMemory<T> nativeMemory) => nativeMemory.Pointer;

    public static ref T GetReference(NativeMemory<T> nativeMemory) => ref nativeMemory.Reference;
}
