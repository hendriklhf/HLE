using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    internal unsafe partial struct ThreadLocalPool
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
        [SuppressMessage("ReSharper", "UnassignedField.Local")]
        private Pool _pool;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private const int ThreadLocalArraysPerLength = 4;

        public bool TryRent(uint bucketIndex, [MaybeNullWhen(false)] out T[] array)
        {
            Debug.Assert(bucketIndex < Pool.Length);

            ref Pool.Bucket bucket = ref Unsafe.Add(ref Unsafe.As<Pool, Pool.Bucket>(ref _pool), bucketIndex);
            if (bucket.Count == 0)
            {
                array = null;
                return false;
            }

            ref T[]? current = ref Unsafe.Add(ref Unsafe.As<Pool.Bucket.ArrayBuffer, T[]?>(ref bucket.Arrays), --bucket.Count);
            array = current;
            Debug.Assert(array is not null);
            current = null;
            return true;
        }

        public bool TryReturn(uint bucketIndex, T[] array)
        {
            Debug.Assert(bucketIndex < Pool.Length);

            ref Pool.Bucket bucket = ref Unsafe.Add(ref Unsafe.As<Pool, Pool.Bucket>(ref _pool), bucketIndex);
            if (bucket.Count == ThreadLocalArraysPerLength)
            {
                return false;
            }

            ref T[]? current = ref Unsafe.Add(ref Unsafe.As<Pool.Bucket.ArrayBuffer, T[]?>(ref bucket.Arrays), bucket.Count++);
            Debug.Assert(current is null);
            current = array;
            return true;
        }
    }
}
