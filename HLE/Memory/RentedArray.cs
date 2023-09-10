using System;
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
/// Wraps an <see cref="Array"/> rented from an <see cref="ArrayPool{T}"/>
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
            return ref Unsafe.Add(ref Reference, index);
        }
    }

    T IIndexAccessible<T>.this[int index] => this[index];

    public ref T this[Index index]
    {
        get
        {
            int actualIndex = index.GetOffset(_array.Length);
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)actualIndex, (uint)_array.Length);
            return ref Unsafe.Add(ref Reference, actualIndex);
        }
    }

    public Span<T> this[Range range] => _array.AsSpan(range);

    public ref T Reference => ref MemoryMarshal.GetArrayDataReference(_array);

    public int Length => _array.Length;

    int ICountable.Count => Length;

    int ICollection<T>.Count => Length;

    int IReadOnlyCollection<T>.Count => Length;

    bool ICollection<T>.IsReadOnly => false;

    public static RentedArray<T> Empty => new();

    internal readonly T[] _array = Array.Empty<T>();
    private readonly ArrayPool<T> _pool = ArrayPool<T>.Shared;

    public RentedArray()
    {
    }

    internal RentedArray(T[] array, ArrayPool<T> pool)
    {
        _array = array;
        _pool = pool;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _pool.Return(_array);
    }

    // TODO: create overload with start, length, range parameters
    public Span<T> AsSpan() => _array;

    // TODO: create overload with start, length, range parameters
    public Memory<T> AsMemory() => _array;

    Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    public void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    void ICollection<T>.Add(T item) => throw new NotSupportedException();

    public void Clear()
    {
        Array.Clear(_array);
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
            yield return Unsafe.Add(ref Reference, i);
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
