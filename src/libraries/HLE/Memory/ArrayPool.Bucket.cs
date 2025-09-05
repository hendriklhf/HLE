using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    [StructLayout(LayoutKind.Auto)]
    internal partial struct Bucket(int arrayLength) : IEquatable<Bucket>
    {
        public int ArrayLength { get; } = arrayLength;

        internal Pool _pool;
        internal volatile uint _positions;
        internal long _lastAccessTick;

        private const int MaxTryCount = 6;

        public bool TryRent([MaybeNullWhen(false)] out T[] array)
        {
            for (int i = 0; i < MaxTryCount; i++)
            {
                uint positions = _positions;
                if (positions == 0)
                {
                    array = null;
                    return false;
                }

                int index = BitOperations.TrailingZeroCount(positions);
                uint bitValue = 1U << index;
                if ((Interlocked.And(ref _positions, ~bitValue) & bitValue) == 0)
                {
                    continue;
                }

                ref T[]? reference = ref Unsafe.Add(ref InlineArrayHelpers.GetReference<Pool, T[]?>(ref _pool), index);
                T[]? value = Interlocked.Exchange(ref reference, null);
                if (value is null)
                {
                    continue;
                }

                array = value;
                Log.RentedShared(array);
                Interlocked.Exchange(ref _lastAccessTick, Environment.TickCount64);
                return true;
            }

            array = null;
            return false;
        }

        public bool TryReturn(T[] array)
        {
            for (int i = 0; i < MaxTryCount; i++)
            {
                uint positions = ~_positions;
                if (positions == 0)
                {
                    continue;
                }

                int index = BitOperations.TrailingZeroCount(positions);
                uint bitValue = 1U << index;
                if ((Interlocked.Or(ref _positions, bitValue) & bitValue) != 0)
                {
                    continue;
                }

                ref T[]? reference = ref Unsafe.Add(ref InlineArrayHelpers.GetReference<Pool, T[]?>(ref _pool), index);
                T[]? previous = Interlocked.CompareExchange(ref reference, array, null);
                if (previous is not null)
                {
                    continue;
                }

                Log.ReturnedShared(array);
                Interlocked.Exchange(ref _lastAccessTick, Environment.TickCount64);
                return true;
            }

            return false;
        }

        public void Clear()
            => SpanHelpers.Clear(ref InlineArrayHelpers.GetReference<Pool, T[]?>(ref _pool), Pool.Length);

        public readonly bool Equals(Bucket other) => ArrayLength == other.ArrayLength && _positions == other._positions && _lastAccessTick == other._lastAccessTick;

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(ArrayLength, _positions, _lastAccessTick);

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
