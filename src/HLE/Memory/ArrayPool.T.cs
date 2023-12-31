using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PureAttribute = System.Diagnostics.Contracts.PureAttribute;

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
    private static ThreadLocalBucket s_threadLocalBucket;

    private const ArrayReturnOptions DefaultReturnOptions = ArrayReturnOptions.ClearOnlyIfManagedType | ArrayReturnOptions.DisposeElements;

    public ArrayPool()
    {
        int bucketCount = BitOperations.TrailingZeroCount(ArrayPool.MaximumArrayLength) - BitOperations.TrailingZeroCount(ArrayPool.MinimumArrayLength) + 1;
        Debug.Assert(ArrayPool.BucketCapacities.Length == bucketCount);

        _buckets = new Bucket[bucketCount];
        int arrayLength = ArrayPool.MinimumArrayLength;
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = new(arrayLength, ArrayPool.BucketCapacities[i]);
            arrayLength <<= 1;
        }

        Debug.Assert(arrayLength >> 1 == ArrayPool.MaximumArrayLength);
    }

    [Pure]
    public T[] Rent(int minimumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);

        int length = minimumLength >= 1 << 30 ? Array.MaxLength : (int)BitOperations.RoundUpToPowerOf2((uint)minimumLength);
        switch (length)
        {
            case > ArrayPool.MaximumArrayLength:
                return GC.AllocateUninitializedArray<T>(length);
            case < ArrayPool.MinimumArrayLength:
                length = ArrayPool.MinimumArrayLength;
                break;
        }

        int bucketIndex = BitOperations.TrailingZeroCount(length) - ArrayPool.IndexOffset;
        return TryRentFromThreadLocalBucket(length, bucketIndex, out T[]? array) ? array : RentFromSharedBuckets(bucketIndex);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private T[] RentFromSharedBuckets(int bucketIndex)
    {
        ref Bucket bucket = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        ref Bucket initialBucket = ref bucket;
        int bucketsLength = _buckets.Length;
        int tryCount = 0;
        const int MaximumTryCount = 3;
        while (bucketIndex < bucketsLength && tryCount < MaximumTryCount)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private static bool TryRentFromThreadLocalBucket(int arrayLength, int bucketIndex, [MaybeNullWhen(false)] out T[] array)
    {
        ref ThreadLocalBucket threadLocalBucket = ref s_threadLocalBucket;
        if (!threadLocalBucket.IsInitialized)
        {
            threadLocalBucket = new();
            threadLocalBucket.BucketInitializationStatuses |= (uint)arrayLength;
            array = GC.AllocateUninitializedArray<T>(arrayLength, true);
            return true;
        }

        if ((threadLocalBucket.BucketInitializationStatuses & arrayLength) == 0)
        {
            threadLocalBucket.BucketInitializationStatuses |= (uint)arrayLength;
            array = GC.AllocateUninitializedArray<T>(arrayLength, true);
            return true;
        }

        ref T[]? arrayReference = ref Unsafe.Add(ref threadLocalBucket.Reference, bucketIndex);
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
    [MustDisposeResource]
    public RentedArray<T> RentAsRentedArray(int minimumLength) => new(Rent(minimumLength), this);

    public void Return(T[]? array, ArrayReturnOptions returnOptions = DefaultReturnOptions)
    {
        if (!TryGetBucketIndex(array, out int bucketIndex, out int pow2Length))
        {
            return;
        }

        if (TryReturnToThreadLocalBucket(array, returnOptions, pow2Length, bucketIndex))
        {
            return;
        }

        ReturnToSharedBucket(array, returnOptions, bucketIndex);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // don't inline as slow path
    private void ReturnToSharedBucket(T[] array, ArrayReturnOptions returnOptions, int bucketIndex)
    {
        ref Bucket bucket = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        bucket.Return(array, returnOptions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // inline as fast path
    private static bool TryReturnToThreadLocalBucket(T[] array, ArrayReturnOptions returnOptions, int pow2Length, int bucketIndex)
    {
        ref ThreadLocalBucket threadLocalBucket = ref s_threadLocalBucket;
        if (!threadLocalBucket.IsInitialized)
        {
            threadLocalBucket = new();
            threadLocalBucket.BucketInitializationStatuses |= (uint)pow2Length;
            Unsafe.Add(ref threadLocalBucket.Reference, bucketIndex) = array;

            if (returnOptions != 0)
            {
                PerformReturnActions(array, returnOptions);
            }

            return true;
        }

        ref T[]? arrayReference = ref Unsafe.Add(ref threadLocalBucket.Reference, bucketIndex);
        if (arrayReference is not null)
        {
            return false;
        }

        arrayReference = array;
        threadLocalBucket.BucketInitializationStatuses |= (uint)pow2Length;

        if (returnOptions != 0)
        {
            PerformReturnActions(array, returnOptions);
        }

        return true;
    }

    private static void PerformReturnActions(T[] array, ArrayReturnOptions options)
    {
        Debug.Assert(options != 0);

        if (typeof(T).IsAssignableTo(typeof(IDisposable)) && (options & ArrayReturnOptions.DisposeElements) != 0)
        {
            for (int i = 0; i < array.Length; i++)
            {
                object? item = array[i];
                IDisposable? disposable = Unsafe.As<object?, IDisposable?>(ref item);
                disposable?.Dispose();
            }
        }

        if (((options & ArrayReturnOptions.Clear) != 0) ||
            (RuntimeHelpers.IsReferenceOrContainsReferences<T>() && (options & ArrayReturnOptions.ClearOnlyIfManagedType) != 0))
        {
            Array.Clear(array);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetBucketIndex([NotNullWhen(true)] T[]? array, out int bucketIndex, out int pow2Length)
    {
        if (array is not { Length: >= ArrayPool.MinimumArrayLength and <= ArrayPool.MaximumArrayLength })
        {
            bucketIndex = -1;
            pow2Length = -1;
            return false;
        }

        pow2Length = array.Length;
        if (!BitOperations.IsPow2(pow2Length))
        {
            pow2Length = (int)(BitOperations.RoundUpToPowerOf2((uint)pow2Length) >> 1);
        }

        bucketIndex = BitOperations.TrailingZeroCount(pow2Length) - ArrayPool.IndexOffset;
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
