using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HLE.Collections;
using HLE.Marshalling;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal struct Bucket(int arrayLength, int capacity) : IEquatable<Bucket>
    {
        internal readonly T[][] _stack = GC.AllocateArray<T[]>(capacity, true);
        internal readonly int _arrayLength = arrayLength;
        internal uint _count;
        internal readonly Lock _lock = new();
        internal long _lastAccessTick;

        [Pure]
        public T[] Rent()
        {
            if (TryRent(out T[]? array))
            {
                return array;
            }

            array = GC.AllocateUninitializedArray<T>(_arrayLength);
            _lastAccessTick = Environment.TickCount64;
            Log.Allocated(array);
            return array;
        }

        public bool TryRent([MaybeNullWhen(false)] out T[] array)
        {
            lock (_lock)
            {
                uint count = _count;
                if (count == 0)
                {
                    array = null;
                    return false;
                }

                ref T[] reference = ref ArrayMarshal.GetUnsafeElementAt(_stack, count - 1);
                _count--;
                array = reference;
                reference = null!; // remove the reference from the pool, so arrays can be collected even if not returned to the pool
                _lastAccessTick = Environment.TickCount64;
                Log.Rented(array);
                return true;
            }
        }

        public bool TryRentExact(int length, [MaybeNullWhen(false)] out T[] array)
        {
            lock (_lock)
            {
                _lastAccessTick = Environment.TickCount64;

                uint count = _count;
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
                    if (i != count - 1)
                    {
                        SpanHelpers.Memmove(ref currentRef, ref Unsafe.Add(ref currentRef, 1), count - i - 1);
                    }

                    _count--;
                    Log.Rented(array);
                    return true;
                }

                array = null;
                return false;
            }
        }

        public void Return(T[] array, bool clearArray)
        {
            lock (_lock)
            {
                uint count = _count;
                if (count == _stack.Length)
                {
                    Log.Dropped(array);
                    return;
                }

                _lastAccessTick = Environment.TickCount64;
                ClearArrayIfNeeded(array, clearArray);
                ArrayMarshal.GetUnsafeElementAt(_stack, count) = array;
                _count++;
                Log.Returned(array);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                ClearWithoutLock();
            }
        }

        public void ClearWithoutLock()
        {
            _stack.AsSpanUnsafe(0, (int)_count).Clear();
            _count = 0;
        }

        public readonly bool Equals(Bucket other) => ReferenceEquals(_stack, other._stack) && _arrayLength == other._arrayLength && _count == other._count;

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(_stack, _arrayLength, _count);

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
