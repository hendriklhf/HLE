using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using HLE.Marshalling;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    internal struct Bucket(int arrayLength, int capacity) : IEquatable<Bucket>
    {
        internal readonly T[]?[] _stack = GC.AllocateArray<T[]>(capacity, true);
        internal readonly int _arrayLength = arrayLength;
        internal long _lastAccessTick;

        [Pure]
        public T[] Rent()
        {
            if (TryRent(out T[]? array))
            {
                return array;
            }

            array = GC.AllocateUninitializedArray<T>(_arrayLength);
            Volatile.Write(ref _lastAccessTick, Environment.TickCount64);
            Log.Allocated(array);
            return array;
        }

        public bool TryRent([MaybeNullWhen(false)] out T[] array)
        {
            ref T[]? current = ref ArrayMarshal.GetUnsafeElementAt(_stack, _stack.Length);
            ref T[]? start = ref ArrayMarshal.GetUnsafeElementAt(_stack, -1);

            do
            {
                T[]? value = Interlocked.Exchange(ref current, null);
                if (value is not null)
                {
                    Log.Rented(value);
                    array = value;
                    Volatile.Write(ref _lastAccessTick, Environment.TickCount64);
                    return true;
                }

                current = ref Unsafe.Subtract(ref current, 1)!;
            }
            while (!Unsafe.AreSame(ref current, ref start));

            array = null;
            return false;
        }

        public void Return(T[] array)
        {
            ref T[]? current = ref ArrayMarshal.GetUnsafeElementAt(_stack, _stack.Length);
            ref T[]? start = ref ArrayMarshal.GetUnsafeElementAt(_stack, -1);

            do
            {
                T[]? prev = Interlocked.CompareExchange(ref current, array, null);
                if (prev is null)
                {
                    Log.Returned(array);
                    Volatile.Write(ref _lastAccessTick, Environment.TickCount64);
                    return;
                }

                current = ref Unsafe.Subtract(ref current, 1)!;
            }
            while (!Unsafe.AreSame(ref current, ref start));

            Log.Dropped(array);
        }

        public readonly void Clear() => SpanHelpers.Clear(_stack, _stack.Length);

        public readonly bool Equals(Bucket other) => ReferenceEquals(_stack, other._stack) && _arrayLength == other._arrayLength && _lastAccessTick == other._lastAccessTick;

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Bucket other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(_stack, _arrayLength, _lastAccessTick);

        public static bool operator ==(Bucket left, Bucket right) => left.Equals(right);

        public static bool operator !=(Bucket left, Bucket right) => !left.Equals(right);
    }
}
