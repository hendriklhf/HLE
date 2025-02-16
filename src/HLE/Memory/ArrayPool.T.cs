using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HLE.Marshalling;

namespace HLE.Memory;

/// <summary>
/// A pool of arrays from which you can rent arrays and return arrays to in order to reuse them.<br/>
/// Also accepts random arrays that were create anywhere else in the application in order to reuse them.
/// </summary>
/// <typeparam name="T">The type of items stored in the rented arrays.</typeparam>
public sealed partial class ArrayPool<T> : IDisposable, IEquatable<ArrayPool<T>>
{
    public static ArrayPool<T> Shared { get; } = new();

    internal readonly Bucket[] _buckets;
    private readonly Trimmer _trimmer;

    [ThreadStatic]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ThreadStatic")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "ThreadStatic")]
    [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
    private static ThreadLocalBucket t_threadLocalBucket;

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

        Trimmer trimmer = new(this);
        trimmer.StartTrimmingThread();
        _trimmer = trimmer;
    }

    ~ArrayPool() => DisposeCore();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DisposeCore();
    }

    private void DisposeCore() => _trimmer.StopTrimmingThread();

    [Pure]
    public T[] Rent(int minimumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);

        int length = minimumLength >= ArrayPool.MaximumPow2Length ? Array.MaxLength : (int)BitOperations.RoundUpToPowerOf2((uint)minimumLength);
        if (length > ArrayPool.MaximumArrayLength)
        {
            T[] allocatedArray = GC.AllocateUninitializedArray<T>(length);
            Log.Allocated(allocatedArray);
            return allocatedArray;
        }

        length = int.Max(length, ArrayPool.MinimumArrayLength);

        Debug.Assert(BitOperations.PopCount((uint)length) == 1);

        int bucketIndex = BitOperations.TrailingZeroCount(length) - ArrayPool.TrailingZeroCountBucketIndexOffset;
        return TryRentFromThreadLocalBucket(length, bucketIndex, out T[]? array) ? array : RentFromSharedBuckets(bucketIndex);
    }

    [Pure]
    public T[] RentExact(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (length > ArrayPool.MaximumArrayLength)
        {
            T[] allocatedArray = GC.AllocateUninitializedArray<T>(length);
            Log.Allocated(allocatedArray);
            return allocatedArray;
        }

        T[]? array;
        int roundedLength = (int)BitOperations.RoundUpToPowerOf2((uint)length);
        int bucketIndex = BitOperations.TrailingZeroCount(roundedLength) - ArrayPool.TrailingZeroCountBucketIndexOffset;
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

        Bucket bucket = ArrayMarshal.GetUnsafeElementAt(_buckets, bucketIndex);
        if (bucket.TryRentExact(length, out array))
        {
            return array;
        }

        array = GC.AllocateUninitializedArray<T>(length);
        Log.Allocated(array);
        return array;
    }

    private T[] RentFromSharedBuckets(int bucketIndex)
    {
        const int MaximumTryCount = 3;

        ref Bucket startingBucket = ref ArrayMarshal.GetUnsafeElementAt(_buckets, bucketIndex);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryRentFromThreadLocalBucket(int arrayLength, int bucketIndex, [MaybeNullWhen(false)] out T[] array)
    {
        Debug.Assert(BitOperations.PopCount((uint)arrayLength) == 1);

        ref ThreadLocalBucket threadLocalBucket = ref t_threadLocalBucket;
        if (!threadLocalBucket.IsInitialized(arrayLength))
        {
            threadLocalBucket.SetInitialized(arrayLength);
            array = GC.AllocateUninitializedArray<T>(arrayLength);
            Log.Allocated(array);
            return true;
        }

        ref T[]? arrayReference = ref Unsafe.Add(ref threadLocalBucket.GetPoolReference(), bucketIndex);
        if (arrayReference is not null)
        {
            array = arrayReference;
            arrayReference = null; // remove reference from the pool
            Log.Rented(array);
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

        if (!TryGetBucketIndex(array.Length, out int bucketIndex, out int pow2Length))
        {
            Log.Dropped(array);
            return;
        }

        if (TryReturnToThreadLocalBucket(array, pow2Length, bucketIndex, clearArray))
        {
            return;
        }

        ReturnToSharedBucket(array, bucketIndex, clearArray);
    }

    private void ReturnToSharedBucket(T[] array, int bucketIndex, bool clearArray)
    {
        ref Bucket bucket = ref ArrayMarshal.GetUnsafeElementAt(_buckets, bucketIndex);
        bucket.Return(array, clearArray);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReturnToThreadLocalBucket(T[] array, int pow2Length, int bucketIndex, bool clearArray)
    {
        Debug.Assert(array.Length != 0);

        ref ThreadLocalBucket threadLocalBucket = ref t_threadLocalBucket;
        if (!threadLocalBucket.IsInitialized(pow2Length))
        {
            ClearArrayIfNeeded(array, clearArray);
            threadLocalBucket.SetInitialized(pow2Length);
            Unsafe.Add(ref threadLocalBucket.GetPoolReference(), bucketIndex) = array;
            Log.Returned(array);
            return true;
        }

        ref T[]? arrayReference = ref Unsafe.Add(ref threadLocalBucket.GetPoolReference(), bucketIndex);
        if (arrayReference is not null)
        {
            return false;
        }

        ClearArrayIfNeeded(array, clearArray);
        arrayReference = array;
        Log.Returned(array);
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

        if (BitOperations.PopCount((uint)arrayLength) == 1)
        {
            pow2Length = arrayLength;
        }
        else
        {
            pow2Length = (int)BitOperations.RoundUpToPowerOf2((uint)arrayLength);
            if (pow2Length != arrayLength)
            {
                pow2Length >>= 1;
            }
        }

        bucketIndex = BitOperations.TrailingZeroCount(pow2Length) - ArrayPool.TrailingZeroCountBucketIndexOffset;
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
    public bool Equals([NotNullWhen(true)] ArrayPool<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ArrayPool<T>? left, ArrayPool<T>? right) => Equals(left, right);

    public static bool operator !=(ArrayPool<T>? left, ArrayPool<T>? right) => !(left == right);
}
