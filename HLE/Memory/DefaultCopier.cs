using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.Memory;

internal readonly ref struct DefaultCopier<T>
{
    private readonly ReadOnlySpan<T> _source;

    public DefaultCopier(ReadOnlySpan<T> source)
    {
        _source = source;
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        if (destination.Count < _source.Length + offset)
        {
            CollectionsMarshal.SetCount(destination, _source.Length + offset);
        }

        Span<T> destinationSpan = CollectionsMarshal.AsSpan(destination);
        Debug.Assert(destinationSpan.Length >= _source.Length, "destinationSpan.Length >= _source.Length");
        CopyTo(destinationSpan.SliceUnsafe(offset));
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyTo(destination.AsSpan(offset));
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyTo(destination.Span);
    }

    public void CopyTo(Span<T> destination)
    {
        _source.CopyTo(destination);
    }

    public unsafe void CopyTo(ref T destination)
    {
        ref byte sourceAsByteReference = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(_source));
        ref byte destinationAsByteReference = ref Unsafe.As<T, byte>(ref destination);
        Unsafe.CopyBlock(ref destinationAsByteReference, ref sourceAsByteReference, (uint)(sizeof(T) * _source.Length));
    }

    public unsafe void CopyTo(T* destination)
    {
        CopyTo(ref Unsafe.AsRef<T>(destination));
    }
}
