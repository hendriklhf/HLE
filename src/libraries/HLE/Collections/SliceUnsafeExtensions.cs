using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public static class SliceUnsafeExtensions
{
    extension<T>(Span<T> span)
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> SliceUnsafe(int start, int length)
        {
            Debug.Assert(length >= 0);
            ref T reference = ref MemoryMarshal.GetReference(span);
            return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref reference, start), length);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> SliceUnsafe(int start) => span.SliceUnsafe(start, span.Length - start);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> SliceUnsafe(Range range)
        {
            (int start, int length) = range.GetOffsetAndLength(span.Length);
            return span.SliceUnsafe(start, length);
        }
    }

    extension<T>(ReadOnlySpan<T> span)
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> SliceUnsafe(int start, int length)
        {
            Debug.Assert(length >= 0);
            ref T reference = ref MemoryMarshal.GetReference(span);
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref reference, start), length);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> SliceUnsafe(int start)
            => span.SliceUnsafe(start, span.Length - start);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> SliceUnsafe(Range range)
        {
            (int start, int length) = range.GetOffsetAndLength(span.Length);
            return span.SliceUnsafe(start, length);
        }
    }
}
