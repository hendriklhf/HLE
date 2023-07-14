using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

/// <summary>
/// A pool of objects from which you can rent objects and return objects to in order to reuse them.<br/>
/// Objects rented from the pool don't necessarily have to be returned to the pool, because references to them are not stored in the pool.
/// </summary>
/// <typeparam name="T">The type of pooled objects.</typeparam>
public sealed class ObjectPool<T> : IEquatable<ObjectPool<T>>
{
    private readonly ConcurrentStack<T> _rentableItems = new();
    private readonly Func<T> _itemFactory;
    private readonly Action<T>? _returnAction;

    private const int _defaultMaximumPoolCapacity = 64;

    public ObjectPool(Func<T> itemFactory, Action<T>? returnAction = null)
    {
        _itemFactory = itemFactory;
        _returnAction = returnAction;
    }

    [Pure]
    public T Rent()
    {
        if (!_rentableItems.TryPop(out T? item))
        {
            item = _itemFactory();
        }

        return item;
    }

    public void Return(T item)
    {
        if (_rentableItems.Count >= _defaultMaximumPoolCapacity)
        {
            return;
        }

        _returnAction?.Invoke(item);
        _rentableItems.Push(item);
    }

    public void Clear()
    {
        _rentableItems.Clear();
    }

    public bool Equals(ObjectPool<T>? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is ObjectPool<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(ObjectPool<T>? left, ObjectPool<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ObjectPool<T>? left, ObjectPool<T>? right)
    {
        return !(left == right);
    }
}
