using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

internal readonly ref struct DefaultCopyableCopier<T>
{
    private readonly ReadOnlySpan<T> _source;

    public DefaultCopyableCopier(ReadOnlySpan<T> source)
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
        CopyTo(destinationSpan[offset..]);
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
        CopyTo((T*)Unsafe.AsPointer(ref destination));
    }

    public unsafe void CopyTo(T* destination)
    {
        T* source = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(_source));
        Unsafe.CopyBlock(destination, source, (uint)(sizeof(T) * _source.Length));
    }
}
