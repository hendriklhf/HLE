using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HLE.Memory;

namespace HLE.Collections;

public sealed class Pool<T> : IDisposable, IEquatable<Pool<T>>
{
    public static Pool<T> Shared => _sharedInstance ?? throw new InvalidOperationException($"The shared instance has not been initialized yet. Use the {nameof(InitializeSharedInstance)} method to initialize the instance.");

    private readonly ConcurrentStack<T> _rentableItems = new();
    private readonly HashSet<T> _rentedItems;
    private readonly SemaphoreSlim _rentedItemsLock = new(1);
    private readonly Func<T> _itemFactory;

    private static Pool<T>? _sharedInstance;

    public Pool(Func<T> itemFactory, IEqualityComparer<T>? equalityComparer = null)
    {
        _itemFactory = itemFactory;
        _rentedItems = new(equalityComparer);
    }

    public static void InitializeSharedInstance(Func<T> itemFactory, IEqualityComparer<T>? equalityComparer = null)
    {
        _sharedInstance ??= new(itemFactory, equalityComparer);
    }

    public static void ReinitializeSharedInstance(Func<T> itemFactory, IEqualityComparer<T>? equalityComparer = null)
    {
        _sharedInstance?.Dispose();
        _sharedInstance = new(itemFactory, equalityComparer);
    }

    public void Dispose()
    {
        _rentedItemsLock.Dispose();
    }

    public T Rent()
    {
        if (!_rentableItems.TryPop(out T? item))
        {
            item = _itemFactory();
        }

        _rentedItemsLock.Wait();
        try
        {
            _rentedItems.Add(item);
        }
        finally
        {
            _rentedItemsLock.Release();
        }

        return item;
    }

    public void Return(T item)
    {
        bool hasBeenReturned;
        _rentedItemsLock.Wait();
        try
        {
            hasBeenReturned = _rentedItems.Remove(item);
        }
        finally
        {
            _rentedItemsLock.Release();
        }

        if (!hasBeenReturned)
        {
            return;
        }

        _rentableItems.Push(item);
    }

    public async ValueTask<T> RentAsync()
    {
        if (!_rentableItems.TryPop(out T? item))
        {
            item = _itemFactory();
        }

        await _rentedItemsLock.WaitAsync();
        try
        {
            _rentedItems.Add(item);
        }
        finally
        {
            _rentedItemsLock.Release();
        }

        return item;
    }

    public async ValueTask ReturnAsync(T item)
    {
        bool hasBeenReturned;
        await _rentedItemsLock.WaitAsync();
        try
        {
            hasBeenReturned = _rentedItems.Remove(item);
        }
        finally
        {
            _rentedItemsLock.Release();
        }

        if (!hasBeenReturned)
        {
            return;
        }

        _rentableItems.Push(item);
    }

    public void Clear()
    {
        _rentedItemsLock.Wait();
        try
        {
            _rentableItems.Clear();
            _rentedItems.Clear();
        }
        finally
        {
            _rentedItemsLock.Release();
        }
    }

    public async ValueTask ClearAsync()
    {
        await _rentedItemsLock.WaitAsync();
        try
        {
            _rentableItems.Clear();
            _rentedItems.Clear();
        }
        finally
        {
            _rentedItemsLock.Release();
        }
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
        return MemoryHelper.GetRawDataPointer(this).GetHashCode();
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
