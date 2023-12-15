using System;

namespace HLE.Memory;

public sealed partial class ObjectPool<T>
{
    public sealed class AnonymousFactory(Func<T> createFunction, Action<T>? returnAction = null) : IFactory
    {
        private readonly Func<T> _createFunction = createFunction;
        private readonly Action<T>? _returnAction = returnAction;

        public T Create() => _createFunction();

        public void Return(T obj) => _returnAction?.Invoke(obj);
    }
}
