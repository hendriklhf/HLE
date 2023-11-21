using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal struct ThreadLocalBucket : IEquatable<ThreadLocalBucket>
    {
        public readonly bool IsInitialized => _pool is not null;

        public uint BucketInitializationStatuses { get; set; }

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
        public readonly ref T[]? Reference => ref MemoryMarshal.GetArrayDataReference(_pool);

        private readonly T[]?[] _pool = new T[BucketCapacities.Length][];

        public ThreadLocalBucket()
        {
        }

        public readonly bool Equals(ThreadLocalBucket other) => ReferenceEquals(_pool, other._pool);

        // ReSharper disable once ArrangeModifiersOrder
        public override readonly bool Equals(object? obj) => obj is ThreadLocalBucket other && Equals(other);

        // ReSharper disable once ArrangeModifiersOrder
        public override readonly int GetHashCode() => _pool.GetHashCode();

        public static bool operator ==(ThreadLocalBucket left, ThreadLocalBucket right) => left.Equals(right);

        public static bool operator !=(ThreadLocalBucket left, ThreadLocalBucket right) => !left.Equals(right);
    }
}
