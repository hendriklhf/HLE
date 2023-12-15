using System;
using System.Reflection;

namespace HLE.Memory;

internal static unsafe class FrozenHeap
{
    private static readonly delegate*<nint, nint, nint> s_registerFrozenSegment = (delegate*<nint, nint, nint>)typeof(GC).GetMethod("_RegisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();
    private static readonly delegate*<nint, void> s_unregisterFrozenSegment = (delegate*<nint, void>)typeof(GC).GetMethod("_UnregisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    public static nint RegisterSegment(nint address, nint size) => s_registerFrozenSegment(address, size);

    public static void UnregisterSegment(nint handle) => s_unregisterFrozenSegment(handle);
}
