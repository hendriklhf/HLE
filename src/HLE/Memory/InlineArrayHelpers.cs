using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public static class InlineArrayHelpers
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TElement GetReference<TArray, TElement>(ref TArray array) => ref Unsafe.As<TArray, TElement>(ref array);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TElement> AsSpan<TArray, TElement>(ref TArray array, int length)
        => MemoryMarshal.CreateSpan(ref GetReference<TArray, TElement>(ref array), length);
}
