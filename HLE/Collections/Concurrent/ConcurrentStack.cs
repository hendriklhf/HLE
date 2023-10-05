using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using HLE.Memory;

namespace HLE.Collections.Concurrent;

/// <summary>
/// A concurrent stack that doesn't allocate on pushing items onto the stack, in comparision to <see cref="System.Collections.Concurrent.ConcurrentStack{T}"/>.
/// </summary>
/// <typeparam name="T">The type of stored items.</typeparam>
public sealed class ConcurrentStack<T> : IEquatable<ConcurrentStack<T>>, IDisposable, IReadOnlyCollection<T>, ICopyable<T>, IReadOnlySpanProvider<T>, ICountable, IIndexAccessible<T>
{
    T IIndexAccessible<T>.this[int index] => GetReadOnlySpan()[index];

    public int Count { get; private set; }

    public int Capacity => _buffer.Length;

    internal T[] _buffer;
    private SemaphoreSlim? _bufferLock = new(1);

    private const int _defaultCapacity = 8;
    private const int _maximumCapacity = 1 << 30;

    public ConcurrentStack(int capacity)
    {
        if (capacity < _defaultCapacity)
        {
            capacity = _defaultCapacity;
        }

        _buffer = GC.AllocateUninitializedArray<T>(capacity);
    }

    public ConcurrentStack() : this(_defaultCapacity)
    {
    }

    public void Dispose()
    {
        _bufferLock?.Dispose();
        _bufferLock = null;
    }

    public void Push(T item)
    {
        ObjectDisposedException.ThrowIf(_bufferLock is null, typeof(ConcurrentStack<T>));

        _bufferLock.Wait();
        try
        {
            if (Count == _buffer.Length)
            {
                GrowBuffer();
            }

            _buffer[Count++] = item;
        }
        finally
        {
            _bufferLock.Release();
        }
    }

    public T Pop()
    {
        ObjectDisposedException.ThrowIf(_bufferLock is null, typeof(ConcurrentStack<T>));

        _bufferLock.Wait();
        try
        {
            if (Count == 0)
            {
                ThrowStackIsEmpty();
            }

            int index = --Count;
            ref T itemReference = ref _buffer[index];
            T item = itemReference;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                itemReference = default!;
            }

            return item;
        }
        finally
        {
            _bufferLock.Release();
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStackIsEmpty()
    {
        throw new InvalidOperationException("The stack is empty.");
    }

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        ObjectDisposedException.ThrowIf(_bufferLock is null, typeof(ConcurrentStack<T>));

        _bufferLock.Wait();
        try
        {
            if (Count == 0)
            {
                item = default;
                return false;
            }

            int index = --Count;
            ref T itemReference = ref _buffer[index];
            item = itemReference;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                itemReference = default!;
            }

            return true;
        }
        finally
        {
            _bufferLock.Release();
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_bufferLock is null, typeof(ConcurrentStack<T>));

        _bufferLock.Wait();
        try
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _buffer.AsSpan(0, Count).Clear();
            }

            Count = 0;
        }
        finally
        {
            _bufferLock.Release();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowBuffer()
    {
        if (_buffer.Length == _maximumCapacity)
        {
            ThrowMaximumStackCapacityReached();
        }

        int newBufferLength = (int)BitOperations.RoundUpToPowerOf2((uint)(_buffer.Length + 1));
        T[] newBuffer = GC.AllocateUninitializedArray<T>(newBufferLength);
        CopyWorker<T> copyWorker = new(_buffer.AsSpan(0, Count));
        copyWorker.CopyTo(newBuffer);
        _buffer = newBuffer;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowMaximumStackCapacityReached()
    {
        throw new InvalidOperationException("The maximum stack capacity has been reached.");
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(GetReadOnlySpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(GetReadOnlySpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(GetReadOnlySpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(GetReadOnlySpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(GetReadOnlySpan());
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(GetReadOnlySpan());
        copyWorker.CopyTo(destination);
    }

    public ReadOnlySpan<T> GetReadOnlySpan()
    {
        return _buffer.AsSpan(0, Count);
    }

    public ArrayEnumerator<T> GetEnumerator() => new(_buffer, 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals(ConcurrentStack<T>? other) => ReferenceEquals(this, other);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }

    public static bool operator ==(ConcurrentStack<T>? left, ConcurrentStack<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ConcurrentStack<T>? left, ConcurrentStack<T>? right)
    {
        return !(left == right);
    }
}
