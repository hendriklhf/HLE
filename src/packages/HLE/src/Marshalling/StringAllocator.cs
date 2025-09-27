using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Memory;

namespace HLE.Marshalling;

public static class StringAllocator
{
    [Pure]
    public static unsafe ref RawStringData Alloc(Span<byte> buffer, ReadOnlySpan<char> chars)
    {
        bool isAligned = MemoryHelpers.IsAligned(ref MemoryMarshal.GetReference(buffer), (uint)sizeof(nuint));
        if (!isAligned)
        {
            ThrowBufferNotAligned();
        }

        nuint size = ObjectMarshal.GetRawStringSize(chars.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, (uint)buffer.Length);

        ref byte reference = ref MemoryMarshal.GetReference(buffer);
        Unsafe.As<byte, nuint>(ref reference) = 0;
        reference = ref Unsafe.Add(ref reference, sizeof(nuint));
        ref RawStringData rawString = ref Unsafe.As<byte, RawStringData>(ref reference);
        rawString.MethodTable = ObjectMarshal.GetMethodTable<string>();
        rawString.Length = chars.Length;
        SpanHelpers.Copy(chars, ref rawString.FirstChar);
        Unsafe.Add(ref rawString.FirstChar, chars.Length) = '\0';
        return ref rawString;

        [DoesNotReturn]
        static void ThrowBufferNotAligned()
            => throw new ArgumentException("The provided buffer is not properly pointer-aligned.", nameof(buffer));
    }
}
