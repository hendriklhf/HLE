using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ObjectPool<T>
{
    public sealed class ArrayFactory<TElement> : IFactory
    {
        public int ArrayLength { get; }

        public ArrayFactory(int arrayLength)
        {
            if (!typeof(T).IsArray)
            {
                ThrowGenericParameterIsNotArray();
            }

            if (typeof(T).GetElementType() != typeof(TElement))
            {
                ThrowGenericParameterIsNotArrayElementType();
            }

            ArrayLength = arrayLength;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Create()
        {
            TElement[] array = GC.AllocateUninitializedArray<TElement>(ArrayLength);
            return Unsafe.As<TElement[], T>(ref array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T obj)
        {
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<TElement>())
            {
                return;
            }

            TElement[] array = Unsafe.As<T, TElement[]>(ref obj);
            Array.Clear(array);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowGenericParameterIsNotArrayElementType()
            => throw new InvalidOperationException($"Generic parameter {typeof(TElement)} is not the element type of generic parameter {typeof(T)}");

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowGenericParameterIsNotArray()
            => throw new InvalidOperationException($"Generic parameter {typeof(T)} is not an array type.");
    }
}
