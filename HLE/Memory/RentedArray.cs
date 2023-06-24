using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Collections;

namespace HLE.Memory;

/// <summary>
/// Wraps an <see cref="System.Array"/> rented from the shared <see cref="ArrayPool{T}"/>
/// to allow declaration with a <see langword="using"/> statement and to remove the need of nesting in a <see langword="try"/>-<see langword="finally"/> block.
/// </summary>
/// <typeparam name="T">The type the rented array contains.</typeparam>
[DebuggerDisplay("Length = {Array.Length}")]
public readonly struct RentedArray<T> : IDisposable, ICollection<T>, ICopyable<T>, ICountable, IEquatable<RentedArray<T>>, IEquatable<T[]>, IRefIndexAccessible<T>
{
    public ref T this[int index] => ref Span[index];

    public ref T this[Index index] => ref Span[index];

    public Span<T> this[Range range] => Span[range];

    public Span<T> Span => Array;

    public Memory<T> Memory => Array;

    public ArraySegment<T> ArraySegment => Array;

    public T[] Array { get; } = System.Array.Empty<T>();

    public ref T Reference => ref MemoryMarshal.GetReference(Span);

    public unsafe T* Pointer => (T*)Unsafe.AsPointer(ref Reference);

    public int Length => Array.Length;

    int ICountable.Count => Length;

    int ICollection<T>.Count => Length;

    bool ICollection<T>.IsReadOnly => false;

    public static RentedArray<T> Empty => new();

    public RentedArray()
    {
    }

    public RentedArray(int minimumLength)
    {
        Array = ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public RentedArray(T[] array)
    {
        Array = array;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (ReferenceEquals(Array, System.Array.Empty<T>()))
        {
            return;
        }

        ArrayPool<T>.Shared.Return(Array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        DefaultCopyableCopier<T> copier = new(Span);
        copier.CopyTo(destination);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            RentedArray<T> rentedArray => Equals(rentedArray.Array),
            T[] array => Equals(array),
            _ => false
        };
    }

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException();
    }

    void ICollection<T>.Clear()
    {
        Span.Clear();
    }

    bool ICollection<T>.Contains(T item)
    {
        return Array.Contains(item);
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException();
    }

    [Pure]
    public bool Equals(RentedArray<T> other)
    {
        return Equals(other.Array);
    }

    [Pure]
    public bool Equals(T[]? array)
    {
        return ReferenceEquals(Array, array);
    }

    [Pure]
    public override int GetHashCode()
    {
        return Array.GetHashCode();
    }

    /// <inheritdoc/>
    [Pure]
    public override string ToString()
    {
        Type thisType = typeof(RentedArray<T>);
        Type genericType = typeof(T);
        return $"{thisType.Namespace}.{nameof(RentedArray<T>)}<{genericType.Namespace}.{genericType.Name}>[{Array.Length}]";
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        int length = Length;
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

    public static implicit operator T[](RentedArray<T> rentedArray)
    {
        return rentedArray.Array;
    }

    public static implicit operator Span<T>(RentedArray<T> rentedArray)
    {
        return rentedArray.Array;
    }

    public static implicit operator ReadOnlySpan<T>(RentedArray<T> rentedArray)
    {
        return rentedArray.Array;
    }

    public static implicit operator Memory<T>(RentedArray<T> rentedArray)
    {
        return rentedArray.Array;
    }

    public static implicit operator ReadOnlyMemory<T>(RentedArray<T> rentedArray)
    {
        return rentedArray.Array;
    }

    public static implicit operator ArraySegment<T>(RentedArray<T> rentedArray)
    {
        return rentedArray.Array;
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
