using System;
using HLE.Collections;
using HLE.Memory;

namespace HLE.Marshalling;

public static class NativeMemoryListMarshal<T> where T : unmanaged, IEquatable<T>
{
    public static NativeMemory<T> GetBuffer(NativeMemoryList<T> list) => list._buffer;

    public static unsafe T* GetPointer(NativeMemoryList<T> list) => list._buffer.Pointer;

    public static ref T GetReference(NativeMemoryList<T> list) => ref list._buffer.Reference;
}
