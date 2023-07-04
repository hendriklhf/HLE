using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public sealed class Pool<T> : IEquatable<Pool<T>>
{
    private readonly ConcurrentStack<T> _rentableItems = new();
    private readonly Func<T> _itemFactory;
    private readonly Action<T>? _returnAction;

    private const int _defaultMaximumPoolCapacity = 32;

    public Pool(Func<T> itemFactory, Action<T>? returnAction = null)
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

    public bool Equals(Pool<T>? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is Pool<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(Pool<T>? left, Pool<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Pool<T>? left, Pool<T>? right)
    {
        return !(left == right);
    }
}
