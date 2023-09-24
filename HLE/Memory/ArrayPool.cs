using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

/// <summary>
/// A pool of arrays from which you can rent arrays and return arrays to in order to reuse them.<br/>
/// Arrays rented from the pool don't necessarily have to be returned to the pool, because references to them are not stored in the pool.<br/>
/// You can also return random arrays that were create anywhere else in the application to the pool in order to reuse them.
/// </summary>
/// <typeparam name="T">The type of items stored in the rented arrays.</typeparam>
public sealed class ArrayPool<T> : IEquatable<ArrayPool<T>>
{
    public static ArrayPool<T> Shared { get; } = new();

    private readonly ObjectPool<T[]>[] _pools;
    private readonly int _indexOffset = BitOperations.TrailingZeroCount(MinimumArrayLength);

    internal const int MinimumArrayLength = 0x10; // has to be pow of 2
    internal const int MaximumArrayLength = 0x100000; // has to be pow of 2

    public ArrayPool()
    {
        int poolCount = BitOperations.TrailingZeroCount(MaximumArrayLength) - BitOperations.TrailingZeroCount(MinimumArrayLength) + 1;
        _pools = new ObjectPool<T[]>[poolCount];
        for (int arrayLength = MinimumArrayLength; arrayLength <= MaximumArrayLength; arrayLength <<= 1)
        {
            int poolIndex = BitOperations.TrailingZeroCount(arrayLength) - _indexOffset;
            ObjectPool<T[]>.ArrayFactory<T> factory = new(arrayLength, true);
            _pools[poolIndex] = new(factory);
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
            case > MaximumArrayLength:
                return GC.AllocateUninitializedArray<T>(length);
            case < MinimumArrayLength:
                length = MinimumArrayLength;
                break;
        }

        int poolIndex = BitOperations.TrailingZeroCount(length) - _indexOffset;
        ObjectPool<T[]> pool = _pools[poolIndex]; // TODO: make it rent from a pool of larger arrays if the exact pool doesnt have any arrays available
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
        if (array is not { Length: >= MinimumArrayLength and <= MaximumArrayLength })
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

    [Pure]
    public bool Equals(ArrayPool<T>? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ArrayPool<T> other && Equals(other);
    }

    [Pure]
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
