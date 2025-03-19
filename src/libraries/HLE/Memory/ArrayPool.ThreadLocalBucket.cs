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

        private const int ThreadLocalArraysPerLength = 3;

        public bool TryRent(int bucketIndex, [MaybeNullWhen(false)] out T[] array)
        {
#pragma warning disable CA1508
            // if the constant's value changes, the code will break
            Debug.Assert(ThreadLocalArraysPerLength == 3);
#pragma warning restore CA1508

            ref T[]? bucket = ref Unsafe.Add(ref InlineArrayHelpers.GetReference<Pool, T[]?>(ref _pool), bucketIndex * ThreadLocalArraysPerLength);

            ref T[]? current = ref Unsafe.Add(ref bucket, 0);
            T[]? value = current;
            if (value is not null)
            {
                goto ArrayFound;
            }

            current = ref Unsafe.Add(ref bucket, 1);
            value = current;
            if (value is not null)
            {
                goto ArrayFound;
            }

            current = ref Unsafe.Add(ref bucket, 2);
            value = current;
            if (value is null)
            {
                array = null;
                return false;
            }

        ArrayFound:
            array = value;
            current = null; // remove reference from pool
            return true;
        }

        public bool TryReturn(int bucketIndex, T[] array)
        {
#pragma warning disable CA1508
            // if the constant's value changes, the code will break
            Debug.Assert(ThreadLocalArraysPerLength == 3);
#pragma warning restore CA1508

            ref T[]? bucket = ref Unsafe.Add(ref Unsafe.As<Pool, T[]?>(ref _pool), bucketIndex * ThreadLocalArraysPerLength);

            ref T[]? current = ref Unsafe.Add(ref bucket, 0);
            if (current is null)
            {
                goto ReturnArray;
            }

            current = ref Unsafe.Add(ref bucket, 1);
            if (current is null)
            {
                goto ReturnArray;
            }

            current = ref Unsafe.Add(ref bucket, 2);
            if (current is not null)
            {
                return false;
            }

        ReturnArray:
            current = array;
            return true;
        }
    }
}
