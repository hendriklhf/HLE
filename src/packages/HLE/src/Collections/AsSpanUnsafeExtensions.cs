using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public static class AsSpanUnsafeExtensions
{
    extension<T>(T[] array)
    {
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpanUnsafe(int start) => array.AsSpanUnsafe(start, array.Length - start);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpanUnsafe(Range range)
        {
            (int start, int length) = range.GetOffsetAndLength(array.Length);
            return array.AsSpanUnsafe(start, length);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpanUnsafe(int start, int length)
        {
            Debug.Assert(length >= 0);
            ref T reference = ref MemoryMarshal.GetArrayDataReference(array);
            return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref reference, start), length);
        }
    }
}
