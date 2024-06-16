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
        internal readonly T[][] _stack = GC.AllocateArray<T[]>(capacity, true);
        private readonly int _arrayLength = arrayLength;
        private int _count;

        [Pure]
        public T[] Rent() => TryRent(out T[]? array) ? array : GC.AllocateUninitializedArray<T>(_arrayLength, true);

        public bool TryRent([MaybeNullWhen(false)] out T[] array)
        {
            lock (_stack)
            {
                int count = _count;
                if (count == 0)
                {
                    array = null;
                    return false;
                }

                ref T[] reference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_stack), --count);
                _count = count;
                array = reference;
                reference = null!; // remove the reference from the pool, so arrays can be collected even if not returned to the pool
                return true;
            }
        }

        public bool TryRentExact(int length, [MaybeNullWhen(false)] out T[] array)
        {
            lock (_stack)
            {
                int count = _count;
                if (count == 0)
                {
                    array = null;
                    return false;
                }

                ref T[] reference = ref MemoryMarshal.GetArrayDataReference(_stack);
                for (uint i = 0; i < count; i++)
                {
                    ref T[] currentRef = ref Unsafe.Add(ref reference, i);
                    if (currentRef.Length != length)
                    {
                        continue;
                    }

                    array = currentRef;
                    SpanHelpers.Memmove(ref currentRef, ref Unsafe.Add(ref currentRef, 1), (uint)count - i - 1);
                    _count = count - 1;
                    return true;
                }

                array = null;
                return false;
            }
        }

        public void Return(T[] array, bool clearArray)
        {
            lock (_stack)
            {
                int count = _count;
                if (count == _stack.Length)
                {
                    return;
                }

                ClearArrayIfNeeded(array, clearArray);
                Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_stack), count++) = array;
                _count = count;
            }
        }

        public void Clear()
        {
            lock (_stack)
            {
                Array.Clear(_stack);
                _count = 0;
            }
        }

        public readonly bool Equals(Bucket other) => ReferenceEquals(_stack, other._stack) && _arrayLength == other._arrayLength && _count == other._count;

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(_stack, _arrayLength, _count);

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
