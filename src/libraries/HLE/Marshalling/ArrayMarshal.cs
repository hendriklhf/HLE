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
        Debug.Assert(index >= 0);
        return ref GetUnsafeElementAt(array, (uint)index);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetUnsafeElementAt<T>(T[] array, uint index)
        => ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
}
