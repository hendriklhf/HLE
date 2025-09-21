using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Marshalling;

internal static class ArrayMarshal
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetUnsafeElementAt<T>(T[] array, int index)
    {
        Debug.Assert((uint)index < (uint)array.Length);
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetUnsafeElementAt<T>(T[] array, uint index)
    {
        Debug.Assert(index < (uint)array.Length);
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref T GetUnsafeElementBeyondLast<T>(T[] array)
    {
        Debug.Assert(array.Length > 0);
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), array.Length);
    }
}
