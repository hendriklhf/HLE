using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace HLE.Memory;

internal static unsafe class FrozenHeap
{
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
    private static readonly delegate*<nint, nint, nint> s_registerFrozenSegment = (delegate*<nint, nint, nint>)typeof(GC).GetMethod("_RegisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
    private static readonly delegate*<nint, void> s_unregisterFrozenSegment = (delegate*<nint, void>)typeof(GC).GetMethod("_UnregisterFrozenSegment", BindingFlags.NonPublic | BindingFlags.Static)!.MethodHandle.GetFunctionPointer();

    public static FrozenSegmentHandle RegisterSegment(nint address, nint size)
    {
        nint handle = s_registerFrozenSegment(address, size);
        return new(handle);
    }

    public static void UnregisterSegment(ref FrozenSegmentHandle handle)
    {
        s_unregisterFrozenSegment(handle.Value);
        handle.Dispose();
    }

    public static void UnregisterSegment(FrozenSegmentHandle* handle)
    {
        s_unregisterFrozenSegment(handle->Value);
        handle->Dispose();
    }
}
