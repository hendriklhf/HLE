using System;
using System.Collections.Generic;
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
    private volatile bool _isTrimmerRunning;

    [ThreadStatic]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ThreadStatic")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "ThreadStatic")]
    [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
    private static ThreadLocalBucket? t_threadLocalBucket;

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

        CreateTrimmer();
    }

    ~ArrayPool() => DisposeCore();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DisposeCore();
    }

    private void DisposeCore() => _isTrimmerRunning = false;

    [Conditional("RELEASE")]
    private void CreateTrimmer()
    {
        _isTrimmerRunning = true;
        WeakReference<ArrayPool<T>> weakPool = new(this);
        Trimmer.StartThread(weakPool);
    }

    [Pure]
    public T[] Rent(int minimumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);

        if (minimumLength > ArrayPool.MaximumArrayLength)
        {
            T[] allocatedArray = GC.AllocateUninitializedArray<T>(minimumLength);
            Log.Allocated(allocatedArray);
            return allocatedArray;
        }

        int length = (int)BitOperations.RoundUpToPowerOf2((uint)minimumLength | ArrayPool.MinimumArrayLength);

        Debug.Assert(length >= minimumLength);
        Debug.Assert(BitOperations.PopCount((uint)length) == 1);

        int bucketIndex = BitOperations.TrailingZeroCount(length) - ArrayPool.TrailingZeroCountBucketIndexOffset;
        return TryRentFromThreadLocalBucket(bucketIndex, out T[]? array) ? array : RentFromSharedBuckets(bucketIndex);
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
    private static bool TryRentFromThreadLocalBucket(int bucketIndex, [MaybeNullWhen(false)] out T[] array)
    {
        ThreadLocalBucket threadLocalBucket = (t_threadLocalBucket ??= new());
#if RELEASE
        return threadLocalBucket.TryRent(bucketIndex, out array);
#else
        if (!threadLocalBucket.TryRent(bucketIndex, out array))
        {
            return false;
        }

        Log.Rented(array);
        return true;
#endif
    }

    public void Return(T[]? array)
    {
        if (array is null)
        {
            return;
        }

        if (!TryGetBucketIndex(array.Length, out int bucketIndex))
        {
            Log.Dropped(array);
            return;
        }

        AssertArrayWithReferencesIsCleared(array);

        if (TryReturnToThreadLocalBucket(array, bucketIndex))
        {
            return;
        }

        ReturnToSharedBucket(array, bucketIndex);
    }

    [Conditional("DEBUG")]
    private static void AssertArrayWithReferencesIsCleared(T[] array)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            return;
        }

        for (int i = 0; i < array.Length; i++)
        {
            Debug.Assert(EqualityComparer<T>.Default.Equals(array[i], default));
        }
    }

    private void ReturnToSharedBucket(T[] array, int bucketIndex)
    {
        ref Bucket bucket = ref ArrayMarshal.GetUnsafeElementAt(_buckets, bucketIndex);
        bucket.Return(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReturnToThreadLocalBucket(T[] array, int bucketIndex)
    {
        Debug.Assert(array.Length != 0);

        ThreadLocalBucket threadLocalBucket = (t_threadLocalBucket ??= new());
#if RELEASE
        return threadLocalBucket.TryReturn(bucketIndex, array);
#else
        if (!threadLocalBucket.TryReturn(bucketIndex, array))
        {
            return false;
        }

        Log.Returned(array);
        return true;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetBucketIndex(int arrayLength, out int bucketIndex)
    {
        if (arrayLength is < ArrayPool.MinimumArrayLength or > ArrayPool.MaximumArrayLength)
        {
            bucketIndex = -1;
            return false;
        }

        int pow2Length;
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
