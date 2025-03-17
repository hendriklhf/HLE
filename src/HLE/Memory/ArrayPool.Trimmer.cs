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
        public static void StartThread(WeakReference<ArrayPool<T>> weakPool)
        {
            Thread thread = new(Trim)
            {
                Name = TypeFormatter.Default.Format(typeof(Trimmer)),
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };

            thread.Start(weakPool);
        }

        private static void Trim(object? obj)
        {
            Debug.Assert(obj is WeakReference<ArrayPool<T>>);
            WeakReference<ArrayPool<T>> weakPool = Unsafe.As<WeakReference<ArrayPool<T>>>(obj);

            while (true)
            {
                Thread.Sleep(ArrayPool.TrimmingInterval);

                if (!weakPool.TryGetTarget(out ArrayPool<T>? pool))
                {
                    break;
                }

                if (!pool._isTrimmerRunning)
                {
                    break;
                }

                TrimCore(pool);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TrimCore(ArrayPool<T> pool)
        {
            bool hasHighMemoryPressure = HasHighMemoryPressure(out long memoryToRelease);

            Span<Bucket> buckets = pool._buckets;
            for (int i = buckets.Length - 1; i >= 0; i--)
            {
                ref Bucket bucket = ref buckets[i];
                if (Volatile.Read(ref bucket._count) == 0)
                {
                    continue;
                }

                lock (bucket._lock)
                {
                    if (Volatile.Read(ref bucket._count) == 0)
                    {
                        continue;
                    }

                    TimeSpan timeSinceLastAccess = TimeSpan.FromMilliseconds(Environment.TickCount64 - Volatile.Read(ref bucket._lastAccessTick));
                    if (timeSinceLastAccess > ArrayPool.MaximumLastAccessTime)
                    {
                        long releasedMemory = (long)ObjectMarshal.GetRawArraySize<T>(bucket._arrayLength) * bucket._count;
                        bucket.ClearWithoutLock();
                        memoryToRelease -= releasedMemory;
                        continue;
                    }

                    if (hasHighMemoryPressure && memoryToRelease > 0)
                    {
                        TrimBucket(ref bucket, ref memoryToRelease);
                    }
                }
            }
        }

        private static void TrimBucket(ref Bucket bucket, ref long memoryToRelease)
        {
            Span<T[]?> stack = bucket._stack!;
            long arraySize = (long)ObjectMarshal.GetRawArraySize<T>(bucket._arrayLength);
            for (int j = (int)bucket._count - 1; j >= 0 && memoryToRelease > 0; j--)
            {
                ref T[]? array = ref stack[j];
                Debug.Assert(array is not null);
                array = null;
                bucket._count--;

                memoryToRelease -= arraySize;
            }
        }

        private static bool HasHighMemoryPressure(out long memoryToRelease)
        {
            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();
            double ratio = ArrayPool.IsCommonlyPooledType<T>() ? ArrayPool.CommonlyPooledTypeTrimThreshold : ArrayPool.TrimThreshold;
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
