using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public unsafe partial class NativeMemory<T>
{
    [Pure]
    public static T* Alloc() => (T*)NativeMemory.AlignedAlloc((uint)sizeof(T), (uint)sizeof(nuint));

    [Pure]
    public static T* AllocZeroed()
    {
        T* obj = Alloc();
        Unsafe.InitBlock(obj, 0, (uint)sizeof(T));
        return obj;
    }

    public static void Free(T* obj) => NativeMemory.AlignedFree(obj);
}
