﻿using System;
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
[DebuggerDisplay("Length = {Array.Length}")]
public struct RentedArray<T> : IDisposable, ICollection<T>, ICopyable<T>, ICountable, IEquatable<RentedArray<T>>, IEquatable<T[]>, IIndexAccessible<T>, IReadOnlyCollection<T>, ISpanProvider<T>
{
    public readonly ref T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Array.Length);
            return ref Unsafe.Add(ref Reference, index);
        }
    }

    readonly T IIndexAccessible<T>.this[int index] => this[index];

    public readonly ref T this[Index index]
    {
        get
        {
            int actualIndex = index.GetOffset(Array.Length);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)actualIndex, (uint)Array.Length);
            return ref Unsafe.Add(ref Reference, actualIndex);
        }
    }

    public readonly Span<T> this[Range range] => AsSpan(range);

    public readonly ref T Reference => ref MemoryMarshal.GetArrayDataReference(Array);

    public readonly int Length => Array.Length;

    readonly int ICountable.Count => Length;

    readonly int ICollection<T>.Count => Length;

    readonly int IReadOnlyCollection<T>.Count => Length;

    readonly bool ICollection<T>.IsReadOnly => false;

    internal readonly T[] Array
    {
        get
        {
            ObjectDisposedException.ThrowIf(_array is null, typeof(RentedArray<T>));
            return _array;
        }
    }

    public static RentedArray<T> Empty => new();

    internal T[]? _array = System.Array.Empty<T>();
    internal readonly ArrayPool<T> _pool = ArrayPool<T>.Shared;

    public RentedArray()
    {
    }

    internal RentedArray(T[] array, ArrayPool<T> pool)
    {
        _array = array;
        _pool = pool;
    }

    public void Dispose()
    {
        _pool.Return(_array!);
        _array = null;
    }

    [Pure]
    public readonly Span<T> AsSpan() => Array;

    [Pure]
    public readonly Span<T> AsSpan(int start) => Array.AsSpan(start);

    [Pure]
    public readonly Span<T> AsSpan(int start, int length) => Array.AsSpan(start, length);

    [Pure]
    public readonly Span<T> AsSpan(Range range) => Array.AsSpan(range);

    [Pure]
    public readonly Memory<T> AsMemory() => Array;

    [Pure]
    public readonly Memory<T> AsMemory(int start) => Array.AsMemory(start);

    [Pure]
    public readonly Memory<T> AsMemory(int start, int length) => Array.AsMemory(start, length);

    [Pure]
    public readonly Memory<T> AsMemory(Range range) => Array.AsMemory(range);

    readonly Span<T> ISpanProvider<T>.GetSpan() => AsSpan();

    [Pure]
    public readonly T[] ToArray()
    {
        T[] result = GC.AllocateUninitializedArray<T>(Length);
        Array.CopyToUnsafe(result);
        return result;
    }

    [Pure]
    public readonly List<T> ToList()
    {
        List<T> result = new(Length);
        CopyWorker<T> copyWorker = new(Array);
        copyWorker.CopyTo(result);
        return result;
    }

    public readonly void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(Array);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(Array);
        copyWorker.CopyTo(destination, offset);
    }

    public readonly void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(Array);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(Array);
        copyWorker.CopyTo(destination);
    }

    public readonly void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(Array);
        copyWorker.CopyTo(ref destination);
    }

    public readonly unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(Array);
        copyWorker.CopyTo(destination);
    }

    readonly void ICollection<T>.Add(T item) => throw new NotSupportedException();

    public readonly void Clear() => System.Array.Clear(Array);

    readonly bool ICollection<T>.Contains(T item) => Array.Contains(item);

    readonly bool ICollection<T>.Remove(T item) => throw new NotSupportedException();

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj switch
        {
            RentedArray<T> rentedArray => Equals(rentedArray.Array),
            T[] array => Equals(array),
            _ => false
        };
    }

    [Pure]
    public readonly bool Equals(RentedArray<T> other) => Equals(other.Array);

    [Pure]
    public readonly bool Equals(T[]? array) => ReferenceEquals(Array, array);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => Array.GetHashCode();

    /// <inheritdoc/>
    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly string ToString()
    {
        if (typeof(T) == typeof(char))
        {
            T[] array = Array;
            ReadOnlySpan<char> charSpan = Unsafe.As<T[], char[]>(ref array);
            return new(charSpan);
        }

        Type thisType = typeof(RentedArray<T>);
        Type genericType = typeof(T);
        return $"{thisType.Namespace}.{nameof(RentedArray<T>)}<{genericType.Namespace}.{genericType.Name}>[{Array.Length}]";
    }

    /// <inheritdoc/>
    public readonly IEnumerator<T> GetEnumerator()
    {
        int length = Length;
        for (int i = 0; i < length; i++)
        {
            yield return Unsafe.Add(ref Reference, i);
        }
    }

    /// <inheritdoc/>
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static bool operator ==(RentedArray<T> left, RentedArray<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RentedArray<T> left, RentedArray<T> right)
    {
        return !(left == right);
    }
}
