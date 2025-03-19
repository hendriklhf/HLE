using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    internal sealed partial class ThreadLocalBucket
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
        [SuppressMessage("ReSharper", "UnassignedField.Local")]
        private Pool _pool;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private const int ThreadLocalArraysPerLength = 3;

        public bool TryRent(int bucketIndex, [MaybeNullWhen(false)] out T[] array)
        {
            ref T[]? start = ref Unsafe.Subtract(ref GetReference(bucketIndex), 1);
            ref T[]? current = ref Unsafe.Add(ref start, ThreadLocalArraysPerLength - 1);

            do
            {
                array = current;
                if (array is not null)
                {
                    current = null; // remove reference from pool
                    return true;
                }

                current = ref Unsafe.Subtract(ref current, 1);
            }
            while (!Unsafe.AreSame(ref current, ref start));

            array = null;
            return false;
        }

        public bool TryReturn(int bucketIndex, T[] array)
        {
            ref T[]? current = ref GetReference(bucketIndex);
            ref T[]? end = ref Unsafe.Add(ref current, ThreadLocalArraysPerLength);

            do
            {
                if (current is null)
                {
                    current = array;
                    return true;
                }

                current = ref Unsafe.Add(ref current, 1)!;
            }
            while (!Unsafe.AreSame(ref current, ref end));

            return false;
        }

        private ref T[]? GetReference(int bucketIndex)
        {
            ref T[]? pool = ref Unsafe.As<Pool, T[]?>(ref _pool);
            return ref Unsafe.Add(ref pool, bucketIndex * ThreadLocalArraysPerLength);
        }
    }
}
