using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static class InlineArrayHelpers
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TElement GetReference<TArray, TElement>(ref TArray array)
        where TArray : struct, allows ref struct
        where TElement : allows ref struct
    {
        ValidateGenericArguments<TArray, TElement>();
        return ref Unsafe.As<TArray, TElement>(ref array);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TElement> AsSpan<TArray, TElement>(ref TArray array, int length) where TArray : struct
        => MemoryMarshal.CreateSpan(ref GetReference<TArray, TElement>(ref array), length);

    [Conditional("DEBUG")]
    [SuppressMessage("Trimming", "IL2090:\'this\' argument does not satisfy \'DynamicallyAccessedMembersAttribute\' in call to target method. The generic parameter of the source method or type does not have matching annotations.")]
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields")]
    private static void ValidateGenericArguments<TArray, TElement>()
        where TArray : allows ref struct
        where TElement : allows ref struct
    {
        ReadOnlySpan<FieldInfo> fields = typeof(TArray).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _ = fields;
        Debug.Assert(fields.Length == 1);
        Debug.Assert(fields[0].FieldType == typeof(TElement));
    }
}
