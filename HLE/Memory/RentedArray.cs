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
/// Wraps an <see cref="Array"/> rented from the shared <see cref="ArrayPool{T}"/>
/// to allow declaration with a <see langword="using"/> statement and to remove the need of nesting in a <see langword="try"/>-<see langword="finally"/> block.
/// </summary>
/// <typeparam name="T">The type the rented array contains.</typeparam>
[DebuggerDisplay("Length = {_array.Length}")]
public readonly struct RentedArray<T> : IDisposable, ICollection<T>, ICopyable<T>, ICountable, IEquatable<RentedArray<T>>, IEquatable<T[]>, IIndexAccessible<T>, IReadOnlyCollection<T>, ISpanProvider<T>
{
    public ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)index, (uint)_array.Length);
            return ref Unsafe.Add(ref ManagedPointer, index);
        }
    }

    T IIndexAccessible<T>.this[int index] => this[index];

    public ref T this[Index index]
    {
        get
        {
            int actualIndex = index.GetOffset(_array.Length);
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)actualIndex, (uint)_array.Length);
            return ref Unsafe.Add(ref ManagedPointer, actualIndex);
        }
    }

    public Span<T> this[Range range] => _array.AsSpan(range);

    public ref T ManagedPointer => ref MemoryMarshal.GetArrayDataReference(_array);

    public unsafe T* Pointer => (T*)Unsafe.AsPointer(ref ManagedPointer);

    public int Length => _array.Length;

    int ICountable.Count => Length;

    int ICollection<T>.Count => Length;

    int IReadOnlyCollection<T>.Count => Length;

    bool ICollection<T>.IsReadOnly => false;

    public static RentedArray<T> Empty => new();

    internal readonly T[] _array = Array.Empty<T>();

    public RentedArray()
    {
    }

    public RentedArray(int minimumLength)
    {
        _array = ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public RentedArray(T[] array)
    {
        _array = array;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (ReferenceEquals(_array, Array.Empty<T>()))
        {
            return;
        }

        ArrayPool<T>.Shared.Return(_array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    // TODO: create overload with start, length, range parameters
    public Span<T> AsSpan() => _array;

    // TODO: create overload with start, length, range parameters
    public Memory<T> AsMemory() => _array;

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    public void CopyTo(List<T> destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        DefaultCopier<T> copier = new(AsSpan());
        copier.CopyTo(destination);
    }

    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    public unsafe void Clear()
    {
        ref byte start = ref Unsafe.As<T, byte>(ref ManagedPointer);
        Unsafe.InitBlock(ref start, 0, (uint)(Length * sizeof(T)));
    }

    bool ICollection<T>.Contains(T item)
    {
        return _array.Contains(item);
    }

    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

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
        if (typeof(T) == typeof(char))
        {
            ReadOnlySpan<char> charSpan = Unsafe.As<T[], char[]>(ref Unsafe.AsRef(in _array));
            return new(charSpan);
        }

        Type thisType = typeof(RentedArray<T>);
        Type genericType = typeof(T);
        return $"{thisType.Namespace}.{nameof(RentedArray<T>)}<{genericType.Namespace}.{genericType.Name}>[{_array.Length}]";
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return AsSpan()[i];
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
