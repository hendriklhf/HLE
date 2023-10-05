using System;
using System.Reflection;

namespace HLE.Memory;

internal static unsafe class FrozenHeap
{
    private static readonly delegate*<nint, nint, nint> _registerFrozenSegment = (delegate*<nint, nint, nint>)typeof(GC).GetMethod("_RegisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();
    private static readonly delegate*<nint, void> _unregisterFrozenSegment = (delegate*<nint, void>)typeof(GC).GetMethod("_UnregisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    public static nint RegisterSegment(nint address, nint size) => _registerFrozenSegment(address, size);

    public static void UnregisterSegment(nint handle) => _unregisterFrozenSegment(handle);
}
