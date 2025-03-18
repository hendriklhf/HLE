using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
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

        private uint _bucketInitializationStatuses;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T[]? GetPoolReference() => ref InlineArrayHelpers.GetReference<Pool, T[]?>(ref Unsafe.AsRef(ref _pool));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInitialized(int arrayLength)
        {
            Debug.Assert(BitOperations.PopCount((uint)arrayLength) == 1);
            _bucketInitializationStatuses |= (uint)arrayLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsInitialized(int arrayLength)
        {
            Debug.Assert(BitOperations.PopCount((uint)arrayLength) == 1);
            return (_bucketInitializationStatuses & arrayLength) != 0;
        }
    }
}
