using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

internal static class Slicer
{
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceReadOnly<T>(ref T buffer, int bufferLength, int start)
        => MemoryMarshal.CreateReadOnlySpan(ref GetStart(ref buffer, bufferLength, start), bufferLength - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceReadOnly<T>(ref T buffer, int bufferLength, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(bufferLength);
        return MemoryMarshal.CreateReadOnlySpan(ref GetStart(ref buffer, bufferLength, start, length), length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> SliceReadOnly<T>(ref T buffer, int bufferLength, int start, int length)
        => MemoryMarshal.CreateReadOnlySpan(ref GetStart(ref buffer, bufferLength, start, length), length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> Slice<T>(ref T buffer, int bufferLength, int start)
        => MemoryMarshal.CreateSpan(ref GetStart(ref buffer, bufferLength, start), bufferLength - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> Slice<T>(ref T buffer, int bufferLength, Range range)
    {
        (int start, int length) = range.GetOffsetAndLength(bufferLength);
        return MemoryMarshal.CreateSpan(ref GetStart(ref buffer, bufferLength, start, length), length);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> Slice<T>(ref T buffer, int bufferLength, int start, int length)
        => MemoryMarshal.CreateSpan(ref GetStart(ref buffer, bufferLength, start, length), length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T GetStart<T>(ref T buffer, int bufferLength, int start)
        => ref GetStart(ref buffer, bufferLength, start, bufferLength - start);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T GetStart<T>(ref T buffer, int bufferLength, int start, int length)
    {
        if (Environment.Is64BitProcess)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)(uint)start + (uint)length, (uint)bufferLength);
        }
        else
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)start, (uint)bufferLength);
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)length, (uint)(bufferLength - start));
        }

        return ref Unsafe.Add(ref buffer, start);
    }
}
