using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    private partial struct SmallArrayPool
    {
        private SmallPool _pool;

        public T[] Rent(int length)
        {
            Debug.Assert(length is >= 0 and < ArrayPool.MinimumArrayLength);

            if (length == 0)
            {
                return [];
            }

            ref SmallPool.SmallBucket bucket = ref Unsafe.Add(ref InlineArrayHelpers.GetReference<SmallPool, SmallPool.SmallBucket>(ref _pool), length - 1);
            ref T[]? arrays = ref InlineArrayHelpers.GetReference<SmallPool.SmallBucket, T[]?>(ref bucket);

            if (arrays is null)
            {
                T[] allocatedArray = GC.AllocateUninitializedArray<T>(length);
                Log.Allocated(allocatedArray);
                return allocatedArray;
            }

            for (int i = 1; i < SmallPool.SmallBucket.Length; i++)
            {
                ref T[]? current = ref Unsafe.Add(ref arrays, i)!;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (current is not null)
                {
                    continue;
                }

                ref T[]? arrayRef = ref Unsafe.Add(ref current, -1);
                T[]? correctArray = arrayRef;
                arrayRef = null!; // remove reference from pool

                Debug.Assert(correctArray is not null);
                Log.Rented(correctArray);
                return correctArray;
            }

            T[] array = GC.AllocateUninitializedArray<T>(length);
            Log.Allocated(array);
            return array;
        }

        public void Return(T[] array, bool clearArray)
        {
            int arrayLength = array.Length;
            Debug.Assert(arrayLength is >= 0 and < ArrayPool.MinimumArrayLength);

            if (arrayLength == 0)
            {
                return;
            }

            ref SmallPool.SmallBucket bucket = ref Unsafe.Add(ref InlineArrayHelpers.GetReference<SmallPool, SmallPool.SmallBucket>(ref _pool), arrayLength - 1);
            ref T[]? arrays = ref InlineArrayHelpers.GetReference<SmallPool.SmallBucket, T[]?>(ref bucket);

            for (int i = 0; i < SmallPool.SmallBucket.Length; i++)
            {
                ref T[]? current = ref Unsafe.Add(ref arrays, i);
                if (current is not null)
                {
                    continue;
                }

                ClearArrayIfNeeded(array, clearArray);
                current = array;
                Log.Returned(array);
                return;
            }

            Log.Dropped(array);
        }
    }
}
