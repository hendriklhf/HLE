using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    private static class Trimmer
    {
        // The trimmer uses a weak reference to allow the pool to be collected
        // while it's not in use, i.e. while the thread is sleeping.
        // When the pool is collected, it won't be retrievable
        // from the weak reference, or the pool's finalizer has set
        // "_isTrimmerRunning" to false, which will end the thread.

        public static void StartThread(WeakReference<ArrayPool<T>> weakPool)
        {
            Thread thread = new(TrimmerThreadStart)
            {
                Name = TypeFormatter.Default.Format(typeof(Trimmer)),
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };

            thread.Start(weakPool);
        }

        private static void TrimmerThreadStart(object? obj)
        {
            Debug.Assert(obj is WeakReference<ArrayPool<T>>);
            WeakReference<ArrayPool<T>> weakPool = Unsafe.As<WeakReference<ArrayPool<T>>>(obj);

            do
            {
                Thread.Sleep(ArrayPoolSettings.TrimmingInterval);
            }
            while (Trim(weakPool));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Trim(WeakReference<ArrayPool<T>> weakPool)
        {
            if (!weakPool.TryGetTarget(out ArrayPool<T>? pool) || !pool._isTrimmerRunning)
            {
                return false;
            }

            TrimPool(pool, out bool allBucketsCleared);
            if (!allBucketsCleared)
            {
                return true;
            }

            // If all buckets have been cleared, it means that
            // the pool hasn't been used for a while, so we can
            // stop the thread for now and let it be restarted,
            // if needed again

            pool._isTrimmerRunning = false;
            return false;
        }

        private static void TrimPool(ArrayPool<T> pool, out bool allBucketsCleared)
        {
            allBucketsCleared = true;

            bool hasHighMemoryPressure = HasHighMemoryPressure(out long memoryToRelease);

            Span<Bucket> buckets = pool._buckets;
            for (int i = buckets.Length - 1; i >= 0; i--)
            {
                ref Bucket bucket = ref buckets[i];

                TimeSpan timeSinceLastAccess = TimeSpan.FromMilliseconds(Environment.TickCount64 - Interlocked.Read(ref bucket._lastAccessTick));
                if (timeSinceLastAccess > ArrayPoolSettings.MaximumLastAccessTime)
                {
                    ClearBucket(ref bucket, ref memoryToRelease);
                    continue;
                }

                allBucketsCleared = false;
                if (hasHighMemoryPressure && memoryToRelease > 0)
                {
                    TrimBucket(ref bucket, ref memoryToRelease);
                }
            }
        }

        private static void ClearBucket(ref Bucket bucket, ref long memoryToRelease)
        {
            ref T[]? current = ref InlineArrayHelpers.GetReference<Bucket.Pool, T[]?>(ref bucket._pool);
            ref T[]? end = ref Unsafe.Add(ref current, Bucket.Pool.Length);

            long releasedMemory = 0;

            int i = 0;

            do
            {
                T[]? array = Interlocked.Exchange(ref current, null);
                Interlocked.And(ref bucket._positions, ~(1U << i));
                if (array is not null)
                {
                    long arraySize = (long)ObjectMarshal.GetRawArraySize(array);
                    releasedMemory += arraySize;
                }

                current = ref Unsafe.Add(ref current, 1);
                i++;
            }
            while (!Unsafe.AreSame(ref current, ref end));

            memoryToRelease -= releasedMemory;
        }

        private static void TrimBucket(ref Bucket bucket, ref long memoryToRelease)
        {
            ref T[]? current = ref InlineArrayHelpers.GetReference<Bucket.Pool, T[]?>(ref bucket._pool);
            ref T[]? end = ref Unsafe.Add(ref current, Bucket.Pool.Length);

            int i = 0;

            do
            {
                T[]? array = Interlocked.Exchange(ref current, null);
                Interlocked.And(ref bucket._positions, ~(1U << i));
                if (array is not null)
                {
                    long arraySize = (long)ObjectMarshal.GetRawArraySize(array);
                    memoryToRelease -= arraySize;
                }

                current = ref Unsafe.Add(ref current, 1);
                i++;
            }
            while (!Unsafe.AreSame(ref current, ref end) && memoryToRelease > 0);
        }

        private static bool HasHighMemoryPressure(out long memoryToRelease)
        {
            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();
            double ratio = ArrayPoolSettings.IsCommonlyPooledType<T>() ? ArrayPoolSettings.CommonlyPooledTypeTrimThreshold : ArrayPoolSettings.TrimThreshold;
            long threshold = (long)(memoryInfo.HighMemoryLoadThresholdBytes * ratio);
            if (memoryInfo.MemoryLoadBytes >= threshold)
            {
                memoryToRelease = memoryInfo.MemoryLoadBytes - threshold;
                return true;
            }

            memoryToRelease = 0;
            return false;
        }
    }
}
