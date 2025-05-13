using System;
using System.Diagnostics;
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
#if NET9_0_OR_GREATER
        where TArray : struct, allows ref struct
        where TElement : allows ref struct
#else
        where TArray : struct
#endif
    {
        ValidateGenericArguments<TArray, TElement>();
        return ref Unsafe.As<TArray, TElement>(ref array);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TElement> AsSpan<TArray, TElement>(ref TArray array, int length) where TArray : struct
        => MemoryMarshal.CreateSpan(ref GetReference<TArray, TElement>(ref array), length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<TElement> AsSpan<TArray, TElement>(ref TArray array) where TArray : struct
        => MemoryMarshal.CreateSpan(ref GetReference<TArray, TElement>(ref array), sizeof(TArray) / sizeof(TElement));

#pragma warning disable
    [Conditional("DEBUG")]
    private static void ValidateGenericArguments<TArray, TElement>()
#if NET9_0_OR_GREATER
        where TArray : allows ref struct
        where TElement : allows ref struct
#endif
    {
        ReadOnlySpan<FieldInfo> fields = typeof(TArray).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _ = fields;
        Debug.Assert(fields.Length == 1);
        Debug.Assert(fields[0].FieldType == typeof(TElement));
    }
#pragma warning restore
}
