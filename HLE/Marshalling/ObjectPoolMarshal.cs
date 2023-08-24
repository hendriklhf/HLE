using System;
using HLE.Collections;

namespace HLE.Marshalling;

public static class ObjectPoolMarshal<T>
{
    public static void SetFactory(ObjectPool<T> pool, Func<T> factory)
    {
        pool._itemFactory = factory;
    }

    public static void SetReturnAction(ObjectPool<T> pool, Action<T> returnAction)
    {
        pool._returnAction = returnAction;
    }
}
