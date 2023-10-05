using System;
using System.Diagnostics.CodeAnalysis;
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
    /// <summary>
    /// Gets the amount of items in the pool.
    /// </summary>
    public int Count => _rentableItems.Count;

    /// <summary>
    /// Gets or sets the maximum amount of items allowed in the pool.
    /// </summary>
    public int Capacity { get; set; } = _defaultCapacity;

    public IFactory Factory { get; } = factory;

    private readonly ConcurrentStack<T> _rentableItems = new();

    private const int _defaultCapacity = 64;

    [Pure]
    public T Rent()
    {
        if (!_rentableItems.TryPop(out T? obj))
        {
            obj = Factory.Create();
        }

        return obj;
    }

    public bool TryRent([MaybeNullWhen(false)] out T obj) => _rentableItems.TryPop(out obj);

    public void Return(T obj)
    {
        if (_rentableItems.Count >= Capacity)
        {
            return;
        }

        Factory.Return(obj);
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

    public bool Equals(ObjectPool<T>? other) => ReferenceEquals(this, other);

    [Pure]
    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    [Pure]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ObjectPool<T>? left, ObjectPool<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ObjectPool<T>? left, ObjectPool<T>? right)
    {
        return !(left == right);
    }
}
