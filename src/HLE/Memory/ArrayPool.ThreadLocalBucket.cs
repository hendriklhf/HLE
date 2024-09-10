using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal partial struct ThreadLocalBucket : IEquatable<ThreadLocalBucket>
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
        [SuppressMessage("ReSharper", "UnassignedField.Local")]
        private Pool _pool;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        private uint _bucketInitializationStatuses;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T[]? GetPoolReference() => ref Unsafe.As<Pool, T[]?>(ref Unsafe.AsRef(ref _pool));

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

        public readonly bool Equals(ThreadLocalBucket other) => _bucketInitializationStatuses == other._bucketInitializationStatuses; // TODO: not correct

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is ThreadLocalBucket other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(_bucketInitializationStatuses, _pool); // TODO: not correct

        public static bool operator ==(ThreadLocalBucket left, ThreadLocalBucket right) => left.Equals(right);

        public static bool operator !=(ThreadLocalBucket left, ThreadLocalBucket right) => !left.Equals(right);
    }
}
