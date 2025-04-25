using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    internal partial struct ThreadLocalBucket
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
        [SuppressMessage("ReSharper", "UnassignedField.Local")]
        private Pool _pool;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private uint _count;

        private const int ThreadLocalArraysPerLength = 4;

        public bool TryRent(int bucketIndex, [MaybeNullWhen(false)] out T[] array)
        {
            if (_count == 0)
            {
                array = null;
                return false;
            }

            ref T[]? bucket = ref Unsafe.Add(ref InlineArrayHelpers.GetReference<Pool, T[]?>(ref _pool), bucketIndex * ThreadLocalArraysPerLength);
            ref T[]? current = ref Unsafe.Add(ref bucket, --_count);
            array = current;
            Debug.Assert(array is not null);
            current = null;
            return true;
        }

        public bool TryReturn(int bucketIndex, T[] array)
        {
            if (_count == ThreadLocalArraysPerLength)
            {
                return false;
            }

            ref T[]? bucket = ref Unsafe.Add(ref Unsafe.As<Pool, T[]?>(ref _pool), bucketIndex * ThreadLocalArraysPerLength);
            ref T[]? current = ref Unsafe.Add(ref bucket, _count++);
            Debug.Assert(current is null);
            current = array;
            return true;
        }
    }
}
