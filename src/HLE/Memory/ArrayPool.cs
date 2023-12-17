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

    internal static bool IsCommonlyPooledType => IsCommonlyPooledTypeCore();

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
        // 2097152,4194304,8388608
        8, 8, 8
    };

    internal readonly Bucket[] _buckets;

    [ThreadStatic]
    private static ThreadLocalBucket s_threadLocalBucket;

    internal const int MinimumArrayLength = 0x10; // has to be pow of 2
    internal const int MaximumArrayLength = 0x800000; // has to be pow of 2
    // ReSharper disable once StaticMemberInGenericType
    internal static readonly int s_indexOffset = BitOperations.TrailingZeroCount(MinimumArrayLength);

    private const ArrayReturnOptions DefaultReturnOptions = ArrayReturnOptions.ClearOnlyIfManagedType | ArrayReturnOptions.DisposeElements;

    public ArrayPool()
    {
        int bucketCount = BitOperations.TrailingZeroCount(MaximumArrayLength) - BitOperations.TrailingZeroCount(MinimumArrayLength) + 1;
        Debug.Assert(BucketCapacities.Length == bucketCount);

        _buckets = new Bucket[bucketCount];
        int arrayLength = MinimumArrayLength;
        for (int i = 0; i < _buckets.Length; i++)
        {
            _buckets[i] = new(arrayLength, BucketCapacities[i]);
            arrayLength <<= 1;
        }

        Debug.Assert(arrayLength >> 1 == MaximumArrayLength);
    }

    [Pure]
    public T[] Rent(int minimumLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);

        int length = minimumLength >= 1 << 30 ? Array.MaxLength : (int)BitOperations.RoundUpToPowerOf2((uint)minimumLength);
        switch (length)
        {
            case > MaximumArrayLength:
                return GC.AllocateUninitializedArray<T>(length);
            case < MinimumArrayLength:
                length = MinimumArrayLength;
                break;
        }

        int bucketIndex = BitOperations.TrailingZeroCount(length) - s_indexOffset;
        return TryRentFromThreadLocalBucket(length, bucketIndex, out T[]? array) ? array : RentFromSharedBuckets(bucketIndex);
    }

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

    private static bool TryRentFromThreadLocalBucket(int arrayLength, int bucketIndex, [MaybeNullWhen(false)] out T[] array)
    {
        ref ThreadLocalBucket threadLocalBucket = ref s_threadLocalBucket;
        if (!threadLocalBucket.IsInitialized)
        {
            threadLocalBucket = new();
            threadLocalBucket.BucketInitializationStatuses |= (uint)arrayLength;
            {
                array = GC.AllocateUninitializedArray<T>(arrayLength, true);
                return true;
            }
        }

        if ((threadLocalBucket.BucketInitializationStatuses & arrayLength) == 0)
        {
            threadLocalBucket.BucketInitializationStatuses |= (uint)arrayLength;
            {
                array = GC.AllocateUninitializedArray<T>(arrayLength, true);
                return true;
            }
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

            return;
        }

        ref T[]? arrayReference = ref Unsafe.Add(ref threadLocalBucket.Reference, bucketIndex);
        if (arrayReference is null)
        {
            arrayReference = array;
            threadLocalBucket.BucketInitializationStatuses |= (uint)pow2Length;

            if (returnOptions != 0)
            {
                PerformReturnActions(array, returnOptions);
            }

            return;
        }

        ref Bucket bucket = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buckets), bucketIndex);
        bucket.Return(array, returnOptions);
    }

    private static void PerformReturnActions(T[] array, ArrayReturnOptions options)
    {
        Debug.Assert(options != 0);

        if (typeof(T).IsAssignableTo(typeof(IDisposable)) && options.HasFlag(ArrayReturnOptions.DisposeElements))
        {
            for (int i = 0; i < array.Length; i++)
            {
                object? item = array[i];
                IDisposable? disposable = Unsafe.As<object?, IDisposable?>(ref item);
                disposable?.Dispose();
            }
        }

        if (options.HasFlag(ArrayReturnOptions.Clear) ||
            (RuntimeHelpers.IsReferenceOrContainsReferences<T>() && options.HasFlag(ArrayReturnOptions.ClearOnlyIfManagedType)))
        {
            Array.Clear(array);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetBucketIndex([NotNullWhen(true)] T[]? array, out int bucketIndex, out int pow2Length)
    {
        if (array is not { Length: >= MinimumArrayLength and <= MaximumArrayLength })
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

        bucketIndex = BitOperations.TrailingZeroCount(pow2Length) - s_indexOffset;
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
    private static bool IsCommonlyPooledTypeCore() =>
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
        typeof(T) == typeof(Int128) ||
        typeof(T) == typeof(UInt128) ||
        typeof(T).IsEnum;

    [Pure]
    public bool Equals([NotNullWhen(true)] ArrayPool<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ArrayPool<T>? left, ArrayPool<T>? right) => Equals(left, right);

    public static bool operator !=(ArrayPool<T>? left, ArrayPool<T>? right) => !(left == right);
}
