using System;
using HLE.Collections;

namespace HLE.Marshals;

public static class NativeMemoryListMarshal<T> where T : unmanaged, IEquatable<T>
{
    public static unsafe T* GetPointer(NativeMemoryList<T> list)
    {
        return NativeMemoryMarshal<T>.GetPointer(list._buffer);
    }

    public static ref T GetReference(NativeMemoryList<T> list)
    {
        return ref NativeMemoryMarshal<T>.GetReference(list._buffer);
    }
}
