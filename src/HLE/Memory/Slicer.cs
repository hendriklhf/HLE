using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public readonly ref partial struct Slicer<T>
{
    /// <inheritdoc cref="GetStart(System.Span{T},int,int)"/>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetStart(ReadOnlySpan<T> span, int start, int length)
        => ref GetStart(ref MemoryMarshal.GetReference(span), span.Length, start, length);

    /// <summary>
    /// Gets the reference to the start of the slice and validates <paramref name="start"/> and <paramref name="length"/>.
    /// </summary>
    /// <param name="span">The <see cref="Span{T}"/> that will be sliced.</param>
    /// <param name="start">The index of the start.</param>
    /// <param name="length">The length of the sliced, beginning at <paramref name="start"/>.</param>
    /// <returns>The reference to the start of the slice.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetStart(Span<T> span, int start, int length)
        => ref GetStart(ref MemoryMarshal.GetReference(span), span.Length, start, length);

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetStart(ref T buffer, int bufferLength, int start, int length)
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
