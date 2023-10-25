using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public readonly unsafe ref partial struct CopyWorker<T>
{
    private readonly ref T _source;
    private readonly nuint _length;

    public CopyWorker(List<T> source) : this(ref MemoryMarshal.GetReference(CollectionsMarshal.AsSpan(source)), source.Count)
    {
    }

    public CopyWorker(T[] source) : this(ref MemoryMarshal.GetArrayDataReference(source), source.Length)
    {
    }

    public CopyWorker(Span<T> source) : this(ref MemoryMarshal.GetReference(source), source.Length)
    {
    }

    public CopyWorker(ReadOnlySpan<T> source) : this(ref MemoryMarshal.GetReference(source), source.Length)
    {
    }

    public CopyWorker(T* source, int length) : this(ref Unsafe.AsRef<T>(source), length)
    {
    }

    public CopyWorker(T* source, uint length) : this(ref Unsafe.AsRef<T>(source), length)
    {
    }

    public CopyWorker(T* source, nuint length) : this(ref Unsafe.AsRef<T>(source), length)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CopyWorker(T* source, long length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)length, nuint.MaxValue);
        _source = ref Unsafe.AsRef<T>(source);
        _length = (nuint)length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CopyWorker(T* source, ulong length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, nuint.MaxValue);
        _source = ref Unsafe.AsRef<T>(source);
        _length = (nuint)length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CopyWorker(ref T source, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _source = ref source;
        _length = (nuint)length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CopyWorker(ref T source, uint length)
    {
        _source = ref source;
        _length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CopyWorker(ref T source, nuint length)
    {
        _source = ref source;
        _length = length;
    }

    public CopyWorker(ref T source, long length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)length, nuint.MaxValue);
        _source = ref source;
        _length = (nuint)length;
    }

    public CopyWorker(ref T source, ulong length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, nuint.MaxValue);
        _source = ref source;
        _length = (nuint)length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(List<T> destination, int offset = 0)
    {
        if (_length >= int.MaxValue)
        {
            ThrowItemsToCopyExceedsInt32MaxValue();
        }

        if (destination.Count < (int)_length + offset)
        {
            CollectionsMarshal.SetCount(destination, (int)_length + offset);
        }

        ref T destinationReference = ref Unsafe.Add(ref MemoryMarshal.GetReference(CollectionsMarshal.AsSpan(destination)), offset);
        CopyTo(ref destinationReference);
    }

    public void CopyTo(T[] destination, int offset = 0) => CopyTo(destination.AsSpan(offset));

    public void CopyTo(Memory<T> destination) => CopyTo(destination.Span);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> destination)
    {
        if ((nuint)destination.Length < _length)
        {
            ThrowDestinationTooShort();
        }

        CopyTo(ref MemoryMarshal.GetReference(destination));
    }

    public void CopyTo(ref T destination) => s_memmove(ref destination, ref _source, _length);

    public void CopyTo(T* destination) => CopyTo(ref Unsafe.AsRef<T>(destination));

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowDestinationTooShort() => throw new InvalidOperationException("The destination length is shorter than the source length.");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowItemsToCopyExceedsInt32MaxValue()
        => throw new InvalidOperationException("The amount of items needed to be copied is greater than the maximum value of a 32-bit signed integer, thus can't be copied to the destination.");
}
