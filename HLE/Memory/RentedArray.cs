using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

/// <summary>
/// Wraps an <see cref="System.Array"/> rented from a shared <see cref="ArrayPool{T}"/>
/// to allow declaration with a <see langword="using"/> statement and to remove the need of nesting in a <see langword="try"/>-<see langword="finally"/> block.
/// </summary>
/// <typeparam name="T">The type the rented array contains.</typeparam>
[DebuggerDisplay("Length = {_array.Length}")]
public readonly struct RentedArray<T> : IDisposable, IEnumerable<T>, ICopyable<T>, IEquatable<RentedArray<T>>, IEquatable<T[]>
{
    public ref T this[int index] => ref Span[index];

    public ref T this[Index index] => ref Span[index];

    public Span<T> this[Range range] => Span[range];

    public Span<T> Span => _array;

    public Memory<T> Memory => _array;

    public ArraySegment<T> ArraySegment => _array;

    public T[] Array => _array;

    public ref T Reference => ref MemoryMarshal.GetReference(Span);

    public unsafe T* Pointer => (T*)Unsafe.AsPointer(ref Reference);

    public int Length => _array.Length;

    private readonly T[] _array = System.Array.Empty<T>();

    public static RentedArray<T> Empty => new();

    public RentedArray()
    {
    }

    public RentedArray(int size)
    {
        _array = ArrayPool<T>.Shared.Rent(size);
    }

    public RentedArray(T[] array)
    {
        _array = array;
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyTo(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(destination), offset));
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination.Span));
    }

    public void CopyTo(Span<T> destination)
    {
        CopyTo(ref MemoryMarshal.GetReference(destination));
    }

    public unsafe void CopyTo(ref T destination)
    {
        CopyTo((T*)Unsafe.AsPointer(ref destination));
    }

    public unsafe void CopyTo(T* destination)
    {
        T* source = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(_array));
        Unsafe.CopyBlock(destination, source, (uint)(sizeof(T) * _array.Length));
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            RentedArray<T> rentedArray => Equals(rentedArray._array),
            T[] array => Equals(array),
            _ => false
        };
    }

    [Pure]
    public bool Equals(RentedArray<T> other)
    {
        return Equals(other._array);
    }

    [Pure]
    public bool Equals(T[]? array)
    {
        return ReferenceEquals(_array, array);
    }

    [Pure]
    public override int GetHashCode()
    {
        return _array.GetHashCode();
    }

    /// <inheritdoc/>
    [Pure]
    public override string ToString()
    {
        Type thisType = typeof(RentedArray<T>);
        Type genericType = typeof(T);
        return $"{thisType.Namespace}.{nameof(RentedArray<T>)}<{genericType.Namespace}.{genericType.Name}>[{_array.Length}]";
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        int length = _array.Length;
        for (int i = 0; i < length; i++)
        {
            yield return Span[i];
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (ReferenceEquals(_array, System.Array.Empty<T>()))
        {
            return;
        }

        ArrayPool<T>.Shared.Return(_array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    public static implicit operator T[](RentedArray<T> rentedArray)
    {
        return rentedArray._array;
    }

    public static implicit operator Span<T>(RentedArray<T> rentedArray)
    {
        return rentedArray._array;
    }

    public static implicit operator ReadOnlySpan<T>(RentedArray<T> rentedArray)
    {
        return rentedArray._array;
    }

    public static implicit operator Memory<T>(RentedArray<T> rentedArray)
    {
        return rentedArray._array;
    }

    public static implicit operator ReadOnlyMemory<T>(RentedArray<T> rentedArray)
    {
        return rentedArray._array;
    }

    public static implicit operator ArraySegment<T>(RentedArray<T> rentedArray)
    {
        return rentedArray._array;
    }

    public static implicit operator RentedArray<T>(T[] array)
    {
        return new(array);
    }

    public static bool operator ==(RentedArray<T> left, RentedArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RentedArray<T> left, RentedArray<T> right)
    {
        return !(left == right);
    }
}
