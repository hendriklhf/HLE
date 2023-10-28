using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace HLE.Memory;

public sealed partial class ArrayPool<T>
{
    private struct Bucket(int arrayLength, int capacity)
    {
        private readonly T[][] _stack = new T[capacity][];
        private readonly int _arrayLength = arrayLength;
        private int _count;

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Rent()
        {
            Monitor.Enter(_stack);
            try
            {
                if (_count == 0)
                {
                    return GC.AllocateUninitializedArray<T>(_arrayLength);
                }

                ref T[] arrayReference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_stack), --_count);
                T[] array = arrayReference;
                arrayReference = null!; // remove the reference from the pool, so arrays can be collected even if not returned to the pool
                return array;
            }
            finally
            {
                Monitor.Exit(_stack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent([MaybeNullWhen(false)] out T[] array)
        {
            Monitor.Enter(_stack);
            try
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
            finally
            {
                Monitor.Exit(_stack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T[] array)
        {
            Monitor.Enter(_stack);
            try
            {
                if (_count == _stack.Length)
                {
                    return;
                }

                Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_stack), _count++) = array;
            }
            finally
            {
                Monitor.Exit(_stack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Monitor.Enter(_stack);
            try
            {
                Array.Clear(_stack);
                _count = 0;
            }
            finally
            {
                Monitor.Exit(_stack);
            }
        }
    }
}
