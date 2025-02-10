using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HLE;

internal static unsafe class EnumValues
{
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
    private static readonly delegate*<Type, Array> s_getValues =
        (delegate*<Type, Array>)typeof(Enum)
            .GetMethod("GetValuesAsUnderlyingTypeNoCopy", BindingFlags.NonPublic | BindingFlags.Static)!
            .MethodHandle
            .GetFunctionPointer();

    public static TEnum[] GetValues<TEnum>() where TEnum : struct, Enum
        => Unsafe.As<TEnum[]>(s_getValues(typeof(TEnum)));
}
