using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using HLE.Marshalling;
using HLE.Text;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    private sealed partial class Trimmer(ArrayPool<T> pool)
    {
        private readonly ArrayPool<T> _pool = pool;
        private volatile bool _running;
        private readonly Thread _thread = new(Trim)
        {
            Name = TypeFormatter.Default.Format<Trimmer>(),
            Priority = ThreadPriority.BelowNormal,
            IsBackground = true
        };

        public void StartTrimmingThread()
        {
            _running = true;
            _thread.Start(new State(this, _pool));
        }

        public void StopTrimmingThread() => _running = false;

        private static void Trim(object? o)
        {
            Debug.Assert(o is State);
            State state = Unsafe.As<State>(o);
            Trimmer trimmer = state.Trimmer;
            ArrayPool<T> pool = state.Pool;

            while (trimmer._running)
            {
                Thread.Sleep(ArrayPool.TrimmingInterval);
                TrimCore(pool);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TrimCore(ArrayPool<T> pool)
        {
            if (!NeedsTrimming(out long memoryToFree))
            {
                return;
            }

            Span<Bucket> buckets = pool._buckets;
            for (int i = buckets.Length - 1; i >= 0 && memoryToFree > 0; i--)
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

                    TrimBucket(ref bucket, ref memoryToFree);
                }
            }
        }

        private static void TrimBucket(ref Bucket bucket, ref long memoryToFree)
        {
            Span<T[]?> stack = bucket._stack!;
            for (int j = (int)bucket._count - 1; j >= 0 && memoryToFree > 0; j--)
            {
                ref T[]? arrayRef = ref stack[j];
                T[]? array = arrayRef;

                arrayRef = null;
                bucket._count--;

                Debug.Assert(array is not null);
                long size = (long)ObjectMarshal.GetRawArraySize(array);
                memoryToFree -= size;
            }
        }

        private static bool NeedsTrimming(out long memoryToFree)
        {
            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();
            double ratio = ArrayPool.IsCommonlyPooledType<T>() ? ArrayPool.CommonlyPooledTypeTrimThreshold : ArrayPool.TrimThreshold;
            long threshold = (long)(memoryInfo.HighMemoryLoadThresholdBytes * ratio);
            if (memoryInfo.MemoryLoadBytes >= threshold)
            {
                memoryToFree = memoryInfo.MemoryLoadBytes - threshold;
                return true;
            }

            memoryToFree = 0;
            return false;
        }
    }
}
