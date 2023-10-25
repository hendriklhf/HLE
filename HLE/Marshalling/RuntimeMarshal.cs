using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HLE.Marshalling;

public static unsafe class RuntimeMarshal<T>
{
    private static readonly delegate*<bool> s_isBitwiseEquatable = (delegate*<bool>)typeof(RuntimeHelpers).GetMethod("IsBitwiseEquatable", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(typeof(T)).MethodHandle.GetFunctionPointer();

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBitwiseEquatable() => s_isBitwiseEquatable();
}
