using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Marshalling;

namespace HLE.Memory;

/// <summary>
/// A pool of arrays from which you can rent arrays and return arrays to reuse them.<br/>
/// Also accepts random arrays created anywhere else in the application.
/// </summary>
/// <typeparam name="T">The type of items stored in the pooled arrays.</typeparam>
public sealed partial class ArrayPool<T> : IDisposable, IEquatable<ArrayPool<T>>
{
    public static ArrayPool<T> Shared { get; } = new();

    internal readonly Bucket[] _buckets;
#if NET9_0_OR_GREATER
    private volatile bool _isTrimmerRunning;
#else
    private volatile uint _isTrimmerRunning;
#endif
    private bool _disposed;

    [ThreadStatic]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "ThreadStatic")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "ThreadStatic")]
    [SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types")]
    private static ThreadLocalPool t_threadLocalPool;

    public ArrayPool()
    {
        int bucketCount = BitOperations.TrailingZeroCount(ArrayPoolSettings.MaximumArrayLength) - BitOperations.TrailingZeroCount(ArrayPoolSettings.MinimumArrayLength) + 1;

        Bucket[] buckets = GC.AllocateArray<Bucket>(bucketCount, true);
        int arrayLength = ArrayPoolSettings.MinimumArrayLength;
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new(arrayLength);
            arrayLength <<= 1;
        }

        _buckets = buckets;

        Debug.Assert(arrayLength >>> 1 == ArrayPoolSettings.MaximumArrayLength);
    }

    ~ArrayPool() => DisposeCore();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DisposeCore();
    }

    private void DisposeCore()
    {
        _disposed = true;
#if NET9_0_OR_GREATER
        _isTrimmerRunning = false;
#else
        _isTrimmerRunning = 0;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartTrimmerIfNotStarted()
    {
#if NET9_0_OR_GREATER
        if (_isTrimmerRunning)
#else
        if (_isTrimmerRunning != 0)
#endif
        {
            return;
        }

        Start();

        return;

        [MethodImpl(MethodImplOptions.NoInlining)]
        void Start()
        {
            if (_disposed)
            {
                ThrowHelper.ThrowObjectDisposedException<ArrayPool<T>>();
            }

#if NET9_0_OR_GREATER
            if (!Interlocked.Exchange(ref _isTrimmerRunning, true))
#else
            if (Interlocked.Exchange(ref _isTrimmerRunning, 1) == 0)
#endif
            {
                WeakReference<ArrayPool<T>> weakPool = new(this);
                Trimmer.StartTask(weakPool);
            }
        }
    }

    [Pure]
    public T[] Rent(int minimumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);

        if (minimumLength > ArrayPoolSettings.MaximumArrayLength)
        {
            T[] allocatedArray = GC.AllocateUninitializedArray<T>(minimumLength);
            Log.Allocated(allocatedArray);
            return allocatedArray;
        }

        int length;
#pragma warning disable IDE0045, S3240
        if (minimumLength >= ArrayPoolSettings.MinimumArrayLength)
#pragma warning restore IDE0045, S3240
        {
            length = BitOperations.PopCount((uint)minimumLength) != 1
                ? (int)BitOperations.RoundUpToPowerOf2((uint)minimumLength)
                : minimumLength;
        }
        else
        {
            length = ArrayPoolSettings.MinimumArrayLength;
        }

        Debug.Assert(length >= minimumLength);
        Debug.Assert(length >= ArrayPoolSettings.MinimumArrayLength);
        Debug.Assert(BitOperations.PopCount((uint)length) == 1);

        int bucketIndex = BitOperations.TrailingZeroCount(length) - ArrayPoolSettings.TrailingZeroCountBucketIndexOffset;
        return TryRentFromThreadLocalBucket(bucketIndex, out T[]? array) ? array : RentFromSharedBuckets(bucketIndex);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
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

        T[] result = GC.AllocateUninitializedArray<T>(startingBucket.ArrayLength);
        Log.Allocated(result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryRentFromThreadLocalBucket(int bucketIndex, [MaybeNullWhen(false)] out T[] array)
    {
        ref ThreadLocalPool threadLocalBucket = ref t_threadLocalPool;
        if (!threadLocalBucket.TryRent((uint)bucketIndex, out array))
        {
            return false;
        }

        Log.RentedThreadLocal(array);
        return true;
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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReturnToSharedBucket(T[] array, int bucketIndex)
    {
        const int MaximumTryCount = 3;

        ref Bucket bucket = ref ArrayMarshal.GetUnsafeElementAt(_buckets, bucketIndex);
        int tryCount = 0;
        do
        {
            if (bucket.TryReturn(array))
            {
                StartTrimmerIfNotStarted();
                return;
            }

            bucket = ref Unsafe.Subtract(ref bucket, 1);
            bucketIndex--;
            tryCount++;
        }
        while (tryCount < MaximumTryCount && bucketIndex >= 0);

        Log.Dropped(array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryReturnToThreadLocalBucket(T[] array, int bucketIndex)
    {
        Debug.Assert(array.Length != 0);

        ref ThreadLocalPool threadLocalBucket = ref t_threadLocalPool;
        if (!threadLocalBucket.TryReturn((uint)bucketIndex, array))
        {
            return false;
        }

        Log.ReturnedThreadLocal(array);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetBucketIndex(int arrayLength, out int bucketIndex)
    {
        if (arrayLength is < ArrayPoolSettings.MinimumArrayLength or > ArrayPoolSettings.MaximumArrayLength)
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

        bucketIndex = BitOperations.TrailingZeroCount(pow2Length) - ArrayPoolSettings.TrailingZeroCountBucketIndexOffset;
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
