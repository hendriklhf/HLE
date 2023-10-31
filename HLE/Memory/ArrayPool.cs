using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

/// <summary>
/// A pool of arrays from which you can rent arrays and return arrays to in order to reuse them.<br/>
/// Arrays rented from the pool don't necessarily have to be returned to the pool, because references to them are not stored in the pool.<br/>
/// You can also return random arrays that were create anywhere else in the application to the pool in order to reuse them.
/// </summary>
/// <typeparam name="T">The type of items stored in the rented arrays.</typeparam>
public sealed partial class ArrayPool<T> : IEquatable<ArrayPool<T>>
{
    public static ArrayPool<T> Shared { get; } = new();

    internal static bool IsCommonlyPooledType => GetIsCommonlyPooledType();

    /// <summary>
    /// Stores the maximum amount of arrays per length that the ArrayPool can hold.
    /// </summary>
    internal static ReadOnlySpan<int> BucketCapacities => new[]
    {
        // 16,32,64,128,256,512
        256, 256, 256, 256, 256, 256,
        // 1024,2048,4096,8192
        128, 128, 128, 128,
        // 16384,32768,65536,
        64, 64, 64,
        // 131072,262144,524288,1048576
        16, 16, 16, 16,
        // 2097152,4194304
        8, 8
    };

    private readonly Bucket[] _buckets;

    internal const int MinimumArrayLength = 0x10; // has to be pow of 2
    internal const int MaximumArrayLength = 0x400000; // has to be pow of 2
    internal const int IndexOffset = 4; // BitOperations.TrailingZeroCount(MinimumArrayLength)

    public ArrayPool()
    {
        int poolCount = BitOperations.TrailingZeroCount(MaximumArrayLength) - BitOperations.TrailingZeroCount(MinimumArrayLength) + 1;
        _buckets = new Bucket[poolCount];
        int arrayLength = MinimumArrayLength;
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = new(arrayLength, int.Max(BucketCapacities[i], Environment.ProcessorCount));
            arrayLength <<= 1;
        }

        Debug.Assert(BucketCapacities.Length == poolCount);
        Debug.Assert(arrayLength >> 1 == MaximumArrayLength);
    }

    [Pure]
    public T[] Rent(int minimumLength)
    {
        switch (minimumLength)
        {
            case 0:
                return [];
            case < 0:
                ThrowMinimumLengthIsNegative(minimumLength);
                break;
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

        int bucketIndex = BitOperations.TrailingZeroCount(length) - IndexOffset;
        ref Bucket bucket = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        ref Bucket initialBucket = ref bucket;
        int bucketsLength = _buckets.Length;
        int tryCount = 0;
        const int maximumTryCount = 3;
        while (bucketIndex < bucketsLength && tryCount < maximumTryCount)
        {
            if (bucket.TryRent(out T[]? array))
            {
                return array;
            }

            bucket = ref Unsafe.Add(ref bucket, 1);
            bucketIndex++;
            tryCount++;
        }

        return initialBucket.Rent();
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMinimumLengthIsNegative(int minimumLength)
        => throw new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "The minimum length is negative.");

    [Pure]
    public RentedArray<T> RentAsRentedArray(int minimumLength) => new(Rent(minimumLength), this);

    public void Return(T[]? array, ArrayReturnOptions returnOptions = ArrayReturnOptions.ClearOnlyIfManagedType)
    {
        if (!TryGetBucketIndex(array, out int bucketIndex))
        {
            return;
        }

        if (returnOptions != 0)
        {
            PerformReturnActions(array, returnOptions);
        }

        ref Bucket bucket = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        bucket.Return(array);
    }

    private static void PerformReturnActions(T[] array, ArrayReturnOptions options)
    {
        Debug.Assert(options != 0);

        if (options.HasFlag(ArrayReturnOptions.Clear) ||
            (RuntimeHelpers.IsReferenceOrContainsReferences<T>() && options.HasFlag(ArrayReturnOptions.ClearOnlyIfManagedType)))
        {
            Array.Clear(array);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetBucketIndex([NotNullWhen(true)] T[]? array, out int bucketIndex)
    {
        if (array is not { Length: >= MinimumArrayLength and <= MaximumArrayLength })
        {
            bucketIndex = -1;
            return false;
        }

        int length = array.Length;
        if (!BitOperations.IsPow2(length))
        {
            length = (int)(BitOperations.RoundUpToPowerOf2((uint)length) >> 1);
        }

        bucketIndex = BitOperations.TrailingZeroCount(length) - IndexOffset;
        return true;
    }

    public void Clear()
    {
        ref Bucket bucketReference = ref MemoryMarshal.GetArrayDataReference(_buckets);
        int lengths = _buckets.Length;
        for (int i = 0; i < lengths; i++)
        {
            ref Bucket bucket = ref Unsafe.Add(ref bucketReference, i);
            bucket.Clear();
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
