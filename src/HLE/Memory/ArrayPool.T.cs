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
/// You can also return random arrays that were create anywhere else in the application to the pool in order to reuse them.
/// </summary>
/// <typeparam name="T">The type of items stored in the rented arrays.</typeparam>
public sealed partial class ArrayPool<T> : IEquatable<ArrayPool<T>>
{
    public static ArrayPool<T> Shared { get; } = new();

    internal readonly Bucket[] _buckets;

    [ThreadStatic]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ThreadStatic")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "ThreadStatic")]
    [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
    private static ThreadLocalBucket t_threadLocalBucket;

    [ThreadStatic]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ThreadStatic")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "ThreadStatic")]
    [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
    private static SmallArrayPool t_smallArrayPool;

    public ArrayPool()
    {
        int bucketCount = BitOperations.TrailingZeroCount(ArrayPool.MaximumArrayLength) - BitOperations.TrailingZeroCount(ArrayPool.MinimumArrayLength) + 1;
        Debug.Assert(ArrayPool.BucketCapacities.Length == bucketCount);

        Bucket[] buckets = GC.AllocateArray<Bucket>(bucketCount, true);
        int arrayLength = ArrayPool.MinimumArrayLength;
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new(arrayLength, ArrayPool.BucketCapacities[i]);
            arrayLength <<= 1;
        }

        _buckets = buckets;

        Debug.Assert(arrayLength >>> 1 == ArrayPool.MaximumArrayLength);
    }

    [Pure]
    public T[] Rent(int minimumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);

        if (minimumLength < ArrayPool.MinimumArrayLength)
        {
            ref SmallArrayPool smallArrayPool = ref t_smallArrayPool;
            return smallArrayPool.Rent(minimumLength);
        }

        int length = minimumLength >= 1 << 30 ? Array.MaxLength : (int)BitOperations.RoundUpToPowerOf2((uint)minimumLength);
        if (length > ArrayPool.MaximumArrayLength)
        {
            return GC.AllocateUninitializedArray<T>(length);
        }

        Debug.Assert(BitOperations.PopCount((uint)length) == 1);

        int bucketIndex = BitOperations.TrailingZeroCount(length) - ArrayPool.BucketIndexOffset;
        return TryRentFromThreadLocalBucket(length, bucketIndex, out T[]? array) ? array : RentFromSharedBuckets(bucketIndex);
    }

    [Pure]
    public T[] RentExact(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        switch (length)
        {
            case > ArrayPool.MaximumArrayLength:
                return GC.AllocateUninitializedArray<T>(length);
            case < ArrayPool.MinimumArrayLength:
                ref SmallArrayPool smallArrayPool = ref t_smallArrayPool;
                return smallArrayPool.Rent(length);
        }

        T[]? array;
        int roundedLength = (int)BitOperations.RoundUpToPowerOf2((uint)length);
        int bucketIndex = BitOperations.TrailingZeroCount(roundedLength) - ArrayPool.BucketIndexOffset;
        if (roundedLength == length)
        {
            if (TryRentFromThreadLocalBucket(length, bucketIndex, out array))
            {
                return array;
            }
        }
        else
        {
            bucketIndex--;
        }

        Bucket bucket = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        return bucket.TryRentExact(length, out array) ? array : GC.AllocateUninitializedArray<T>(length, true);
    }

    [Pure]
    public T[] RentFor(T item)
    {
        T[] array = RentExact(1);
        Debug.Assert(array.Length == 1);

        MemoryMarshal.GetArrayDataReference(array) = item;
        return array;
    }

    [Pure]
    public T[] RentFor(T item0, T item1)
    {
        T[] array = RentExact(2);
        Debug.Assert(array.Length == 2);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(array);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;

        return array;
    }

    [Pure]
    public T[] RentFor(T item0, T item1, T item2)
    {
        T[] array = RentExact(3);
        Debug.Assert(array.Length == 3);

        ref T reference = ref MemoryMarshal.GetArrayDataReference(array);
        Unsafe.Add(ref reference, 0) = item0;
        Unsafe.Add(ref reference, 1) = item1;
        Unsafe.Add(ref reference, 2) = item2;

        return array;
    }

    [Pure]
    public T[] RentFor(params ReadOnlySpan<T> items)
    {
        T[] array = RentExact(items.Length);
        Debug.Assert(array.Length == items.Length);

        SpanHelpers.Copy(items, array);
        return array;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private T[] RentFromSharedBuckets(int bucketIndex)
    {
        const int MaximumTryCount = 3;

        ref Bucket startingBucket = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        int bucketsLength = _buckets.Length;
        ref Bucket bucket = ref startingBucket;
        int tryCount = 0;
        do
        {
            if (bucket.TryRent(out T[]? array))
            {
                return array;
            }

            bucket = ref Unsafe.Add(ref bucket, 1);
            bucketIndex++;
            tryCount++;
        }
        while (tryCount < MaximumTryCount && bucketIndex < bucketsLength);

        return startingBucket.Rent();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private static bool TryRentFromThreadLocalBucket(int arrayLength, int bucketIndex, [MaybeNullWhen(false)] out T[] array)
    {
        Debug.Assert(BitOperations.PopCount((uint)arrayLength) == 1);

        ref ThreadLocalBucket threadLocalBucket = ref t_threadLocalBucket;
        if (!threadLocalBucket.IsInitialized(arrayLength))
        {
            threadLocalBucket.SetInitialized(arrayLength);
            array = GC.AllocateUninitializedArray<T>(arrayLength, true);
            return true;
        }

        ref T[]? arrayReference = ref Unsafe.Add(ref threadLocalBucket.GetPoolReference(), bucketIndex);
        if (arrayReference is not null)
        {
            array = arrayReference;
            arrayReference = null; // remove reference from the pool
            return true;
        }

        array = null;
        return false;
    }

    [Pure]
    public RentedArray<T> RentAsRentedArray(int minimumLength) => new(Rent(minimumLength), this);

    public void Return(T[]? array, bool clearArray = false)
    {
        if (array is null)
        {
            return;
        }

        if (array.Length < ArrayPool.MinimumArrayLength)
        {
            ref SmallArrayPool smallArrayPool = ref t_smallArrayPool;
            smallArrayPool.Return(array, clearArray);
            return;
        }

        if (!TryGetBucketIndex(array.Length, out int bucketIndex, out int pow2Length))
        {
            return;
        }

        if (TryReturnToThreadLocalBucket(array, pow2Length, bucketIndex, clearArray))
        {
            return;
        }

        ReturnToSharedBucket(array, bucketIndex, clearArray);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private void ReturnToSharedBucket(T[] array, int bucketIndex, bool clearArray)
    {
        ref Bucket bucket = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        bucket.Return(array, clearArray);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private static bool TryReturnToThreadLocalBucket(T[] array, int pow2Length, int bucketIndex, bool clearArray)
    {
        // TODO: check if array can still be returned even though it is not a pow2 length
        if (BitOperations.PopCount((uint)array.Length) != 1)
        {
            return false;
        }

        ref ThreadLocalBucket threadLocalBucket = ref t_threadLocalBucket;
        if (!threadLocalBucket.IsInitialized(pow2Length))
        {
            ClearArrayIfNeeded(array, clearArray);
            threadLocalBucket.SetInitialized(pow2Length);
            Unsafe.Add(ref threadLocalBucket.GetPoolReference(), bucketIndex) = array;
            return true;
        }

        ref T[]? arrayReference = ref Unsafe.Add(ref threadLocalBucket.GetPoolReference(), bucketIndex);
        if (arrayReference is not null)
        {
            return false;
        }

        ClearArrayIfNeeded(array, clearArray);
        arrayReference = array;
        threadLocalBucket.SetInitialized(pow2Length);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ClearArrayIfNeeded(T[] array, bool clearArray)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>() || clearArray)
        {
            Array.Clear(array);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetBucketIndex(int arrayLength, out int bucketIndex, out int pow2Length)
    {
        if (arrayLength is < ArrayPool.MinimumArrayLength or > ArrayPool.MaximumArrayLength)
        {
            bucketIndex = -1;
            pow2Length = -1;
            return false;
        }

        pow2Length = (int)BitOperations.RoundUpToPowerOf2((uint)arrayLength);
        if (pow2Length != arrayLength)
        {
            pow2Length >>= 1;
        }

        bucketIndex = BitOperations.TrailingZeroCount(pow2Length) - ArrayPool.BucketIndexOffset;
        return true;
    }

    public void Clear()
    {
        Bucket[] buckets = _buckets;
        ref Bucket bucketReference = ref MemoryMarshal.GetArrayDataReference(buckets);
        int lengths = buckets.Length;
        for (int i = 0; i < lengths; i++)
        {
            ref Bucket bucket = ref Unsafe.Add(ref bucketReference, i);
            bucket.Clear();
        }
    }

    [Pure]
    public bool Equals([NotNullWhen(true)] ArrayPool<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ArrayPool<T>? left, ArrayPool<T>? right) => Equals(left, right);

    public static bool operator !=(ArrayPool<T>? left, ArrayPool<T>? right) => !(left == right);
}
