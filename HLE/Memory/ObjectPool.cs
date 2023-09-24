using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using HLE.Collections;
using HLE.Collections.Concurrent;

namespace HLE.Memory;

/// <summary>
/// A pool of objects from which you can rent objects and return objects to in order to reuse them.<br/>
/// Objects rented from the pool don't necessarily have to be returned to the pool, because references to them are not stored in the pool.
/// </summary>
/// <typeparam name="T">The type of pooled objects.</typeparam>
public sealed partial class ObjectPool<T>(ObjectPool<T>.IFactory factory)
    : IEquatable<ObjectPool<T>>, IDisposable, ICountable
{
    public int Count => _rentableItems.Count;

    public int Capacity { get; set; } = 64;

    private readonly ConcurrentStack<T> _rentableItems = new();
    internal readonly IFactory _factory = factory;

    [Pure]
    public T Rent()
    {
        if (!_rentableItems.TryPop(out T? obj))
        {
            obj = _factory.Create();
        }

        return obj;
    }

    public void Return(T obj)
    {
        if (_rentableItems.Count >= Capacity)
        {
            return;
        }

        _factory.Return(obj);
        _rentableItems.Push(obj);
    }

    public void Clear()
    {
        _rentableItems.Clear();
    }

    public void Dispose()
    {
        _rentableItems.Dispose();
    }

    public bool Equals(ObjectPool<T>? other)
    {
        return ReferenceEquals(this, other);
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ObjectPool<T> other && Equals(other);
    }

    [Pure]
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
