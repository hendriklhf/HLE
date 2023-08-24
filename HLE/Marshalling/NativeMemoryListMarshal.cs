using System;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Marshalling;

public static class NativeMemoryListMarshal<T> where T : unmanaged, IEquatable<T>
{
    public static NativeMemory<T> GetBuffer(NativeMemoryList<T> list)
    {
        return list._buffer;
    }

    public static unsafe T* GetPointer(NativeMemoryList<T> list)
    {
        return NativeMemoryMarshal<T>.GetPointer(list._buffer);
    }

    public static ref T GetManagedPointer(NativeMemoryList<T> list)
    {
        return ref NativeMemoryMarshal<T>.GetManagedPointer(list._buffer);
    }
}
