using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal struct Bucket(int arrayLength, int capacity) : IEquatable<Bucket>
    {
        public readonly bool CanReturn => _count != _stack.Length;

        public readonly object SyncRoot => _stack;

        internal readonly T[][] _stack = new T[capacity][];
        private readonly int _arrayLength = arrayLength;
        private int _count;

        [Pure]
        public T[] Rent() => TryRent(out T[]? array) ? array : GC.AllocateUninitializedArray<T>(_arrayLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent([MaybeNullWhen(false)] out T[] array)
        {
            lock (_stack)
            {
                if (_count == 0)
                {
                    array = null;
                    return false;
                }

                ref T[] arrayReference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_stack), --_count);
                array = arrayReference;
                arrayReference = null!; // remove the reference from the pool, so arrays can be collected even if not returned to the pool
                return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T[] array, ArrayReturnOptions returnOptions)
        {
            lock (_stack)
            {
                if (_count == _stack.Length)
                {
                    return;
                }

                if (returnOptions != 0)
                {
                    PerformReturnActions(array, returnOptions);
                }

                Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_stack), _count++) = array;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (_stack)
            {
                Array.Clear(_stack);
                _count = 0;
            }
        }

        public readonly bool Equals(Bucket other) => ReferenceEquals(_stack, other._stack) && _arrayLength == other._arrayLength && _count == other._count;

        // ReSharper disable once ArrangeModifiersOrder
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        // ReSharper disable once ArrangeModifiersOrder
        public override readonly int GetHashCode() => HashCode.Combine(_stack, _arrayLength, _count);

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
