using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

/// <summary>
/// A pool of arrays from which you can rent arrays and return arrays to in order to reuse them.<br/>
/// Arrays rented from the pool don't necessarily have to be returned to the pool, because references to them are not stored in the pool.
/// </summary>
/// <typeparam name="T">The type of items stored in the rented arrays.</typeparam>
public sealed class ArrayPool<T> : IEquatable<ArrayPool<T>>
{
    public static ArrayPool<T> Shared { get; } = new();

    private readonly ObjectPool<T[]>[] _pools;
    private readonly int _indexOffset = BitOperations.TrailingZeroCount(_minimumArrayLength);

    private const int _minimumArrayLength = 0x10; // has to be pow of 2
    private const int _maximumArrayLength = 0x100000; // has to be pow of 2

    public ArrayPool()
    {
        int poolCount = BitOperations.TrailingZeroCount(_maximumArrayLength) - BitOperations.TrailingZeroCount(_minimumArrayLength) + 1;
        _pools = new ObjectPool<T[]>[poolCount];
        for (int i = _minimumArrayLength; i <= _maximumArrayLength; i <<= 1)
        {
            int poolIndex = BitOperations.TrailingZeroCount(i) - _indexOffset;
            int arrayLength = i;
            _pools[poolIndex] = new(() => GC.AllocateUninitializedArray<T>(arrayLength));
        }
    }

    [Pure]
    public T[] Rent(int minimumLength)
    {
        if (minimumLength <= 0)
        {
            return Array.Empty<T>();
        }

        int length = (int)BitOperations.RoundUpToPowerOf2((uint)minimumLength);
        switch (length)
        {
            case > _maximumArrayLength:
                return GC.AllocateUninitializedArray<T>(length);
            case < _minimumArrayLength:
                length = _minimumArrayLength;
                break;
        }

        int poolIndex = BitOperations.TrailingZeroCount(length) - _indexOffset;
        ObjectPool<T[]> pool = _pools[poolIndex];
        return pool.Rent();
    }

    [Pure]
    public RentedArray<T> CreateRentedArray(int minimumLength)
    {
        return new(Rent(minimumLength), this);
    }

    public void Return(T[] array)
    {
        if (!TryGetPoolIndex(array, out int poolIndex))
        {
            return;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(array);
        }

        ObjectPool<T[]> pool = _pools[poolIndex];
        pool.Return(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetPoolIndex(T[] array, out int poolIndex)
    {
        if (array is not { Length: >= _minimumArrayLength and <= _maximumArrayLength })
        {
            poolIndex = -1;
            return false;
        }

        int length = array.Length;
        if (!BitOperations.IsPow2(length))
        {
            length = (int)(BitOperations.RoundUpToPowerOf2((uint)length) >> 1);
        }

        poolIndex = BitOperations.TrailingZeroCount(length) - _indexOffset;
        return true;
    }

    public void Clear()
    {
        foreach (ObjectPool<T[]> pool in _pools)
        {
            pool.Clear();
        }
    }

    public bool Equals(ArrayPool<T>? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is ArrayPool<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(ArrayPool<T>? left, ArrayPool<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ArrayPool<T>? left, ArrayPool<T>? right)
    {
        return !(left == right);
    }
}
