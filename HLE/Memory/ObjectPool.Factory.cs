using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public sealed partial class ObjectPool<T>
{
    public interface IFactory
    {
        T Create();

        void Return(T obj);
    }

    public sealed class ArrayFactory<TElement> : IFactory
    {
        public int ArrayLength { get; }

        private readonly bool _skipClearing;

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

        internal ArrayFactory(int arrayLength, bool skipClearing) : this(arrayLength)
        {
            _skipClearing = skipClearing;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Create()
        {
            Debug.Assert(typeof(TElement[]) == typeof(T));
            TElement[] array = GC.AllocateUninitializedArray<TElement>(ArrayLength);
            return Unsafe.As<TElement[], T>(ref array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T obj)
        {
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<TElement>() || _skipClearing)
            {
                return;
            }

            Debug.Assert(typeof(T) == typeof(TElement[]));
            TElement[] array = Unsafe.As<T, TElement[]>(ref obj);
            Array.Clear(array);
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowGenericParameterIsNotArrayElementType()
        {
            throw new InvalidOperationException($"Generic parameter {typeof(TElement)} is not the element type of generic parameter {typeof(T)}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowGenericParameterIsNotArray()
        {
            throw new InvalidOperationException($"Generic parameter {typeof(T)} is not an array type.");
        }
    }

    public sealed class AnonymousFactory(Func<T> createFunction, Action<T>? returnAction = null) : IFactory
    {
        private readonly Func<T> _createFunction = createFunction;
        private readonly Action<T>? _returnAction = returnAction;

        public T Create() => _createFunction();

        public void Return(T obj) => _returnAction?.Invoke(obj);
    }
}
