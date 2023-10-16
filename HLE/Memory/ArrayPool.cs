using System;
using System.Diagnostics;
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

    internal static bool IsCommonlyPooledType => GetIsCommonlyPooledType();

    /// <summary>
    /// Stores the maximum amount of arrays per length that the ArrayPool can hold.
    /// </summary>
    internal static ReadOnlySpan<int> ObjectPoolCapacities => new[]
    {
        // 16,32,64,128,256,512
        128, 128, 128, 64, 64, 64,
        // 1024,2048,4096,8192
        32, 32, 32, 32,
        // 16384,32768,65536,
        16, 16, 16,
        // 131072,262144,524288,1048576
        8, 8, 8, 8,
        // 2097152,4194304
        4, 4
    };

    private readonly ObjectPool<T[]>[] _pools;

    internal const int MinimumArrayLength = 0x10; // has to be pow of 2
    internal const int MaximumArrayLength = 0x400000; // has to be pow of 2
    internal const int IndexOffset = 4; // BitOperations.TrailingZeroCount(MinimumArrayLength)

    public ArrayPool()
    {
        int poolCount = BitOperations.TrailingZeroCount(MaximumArrayLength) - BitOperations.TrailingZeroCount(MinimumArrayLength) + 1;
        _pools = new ObjectPool<T[]>[poolCount];
        int arrayLength = MinimumArrayLength;
        for (int i = 0; i < _pools.Length; i++)
        {
            ObjectPool<T[]>.ArrayFactory<T> factory = new(arrayLength, true);
            _pools[i] = new(factory)
            {
                Capacity = ObjectPoolCapacities[i]
            };
            arrayLength <<= 1;
        }

        Debug.Assert(ObjectPoolCapacities.Length == _pools.Length);
        Debug.Assert(arrayLength >> 1 == MaximumArrayLength);
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

        int poolIndex = BitOperations.TrailingZeroCount(length) - IndexOffset;
        ref ObjectPool<T[]> pool = ref _pools[poolIndex];
        ObjectPool<T[]> initialPool = pool;
        int poolsLength = _pools.Length;
        int tryCount = 0;
        const int maximumTryCount = 3;
        while (poolIndex < poolsLength && tryCount < maximumTryCount)
        {
            if (pool.TryRent(out T[]? array))
            {
                return array;
            }

            pool = ref Unsafe.Add(ref pool, 1);
            poolIndex++;
            tryCount++;
        }

        return initialPool.Rent();
    }

    [Pure]
    public RentedArray<T> CreateRentedArray(int minimumLength) => new(Rent(minimumLength), this);

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
    private static bool TryGetPoolIndex(T[] array, out int poolIndex)
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

        poolIndex = BitOperations.TrailingZeroCount(length) - IndexOffset;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetIsCommonlyPooledType() =>
        typeof(T) == typeof(byte) ||
        typeof(T) == typeof(sbyte) ||
        typeof(T) == typeof(short) ||
        typeof(T) == typeof(ushort) ||
        typeof(T) == typeof(int) ||
        typeof(T) == typeof(uint) ||
        typeof(T) == typeof(long) ||
        typeof(T) == typeof(ulong) ||
        typeof(T) == typeof(nint) ||
        typeof(T) == typeof(nuint) ||
        typeof(T) == typeof(bool) ||
        typeof(T) == typeof(string) ||
        typeof(T) == typeof(char) ||
        typeof(T).IsEnum;

    [Pure]
    public bool Equals(ArrayPool<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ArrayPool<T>? left, ArrayPool<T>? right) => Equals(left, right);

    public static bool operator !=(ArrayPool<T>? left, ArrayPool<T>? right) => !(left == right);
}
