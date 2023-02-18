using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

/// <summary>
/// Wraps an <see cref="Array"/> rented from a shared <see cref="ArrayPool{T}"/>
/// to allow declaration with a <see langword="using"/> statement and to remove the need of nesting in a try-finally block.
/// </summary>
/// <typeparam name="T">The type the rented array contains.</typeparam>
[DebuggerDisplay("Length = {_array.Length}")]
public readonly struct RentedArray<T> : IDisposable, IEnumerable<T>, ICopyable<T>
{
    public ref T this[int index] => ref Span[index];

    public ref T this[Index index] => ref Span[index];

    public Span<T> this[Range range] => Span[range];

    public Span<T> Span => _array;

    public Memory<T> Memory => _array;

    public T[] Array => _array;

    public int Length => _array.Length;

    private readonly T[] _array = System.Array.Empty<T>();

    public static RentedArray<T> Empty => new();

    public RentedArray()
    {
    }

    public RentedArray(T[] array)
    {
        _array = array;
    }

    public void CopyTo(T[] destination)
    {
        CopyTo((Span<T>)destination);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyTo(destination.Span);
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

    public override bool Equals(object? obj)
    {
        return obj is RentedArray<T> rentedArray && rentedArray == this;
    }

    public bool Equals(RentedArray<T> rentedArray)
    {
        return rentedArray == this;
    }

    public bool Equals(T[] array)
    {
        return ReferenceEquals(_array, array);
    }

    public override int GetHashCode()
    {
        return _array.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        Type thisType = typeof(RentedArray<T>);
        Type genericType = typeof(T);
        return $"{thisType.Namespace}.{nameof(RentedArray<T>)}<{genericType.Namespace}.{genericType.Name}>[{_array.Length}]";
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        return _array.AsEnumerable().GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(_array);
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

    public static implicit operator RentedArray<T>(T[] array)
    {
        return new(array);
    }

    public static bool operator ==(RentedArray<T> left, RentedArray<T> right)
    {
        return ReferenceEquals(left._array, right._array);
    }

    public static bool operator !=(RentedArray<T> left, RentedArray<T> right)
    {
        return !(left == right);
    }
}
