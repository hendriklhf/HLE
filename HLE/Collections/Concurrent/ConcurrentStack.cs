using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    private readonly SemaphoreSlim _bufferLock = new(1);

    private const int _defaultCapacity = 4;

    public ConcurrentStack() : this(_defaultCapacity)
    {
    }

    public ConcurrentStack(int capacity)
    {
        _buffer = new T[capacity];
    }

    public void Push(T item)
    {
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
        _bufferLock.Wait();
        try
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The stack is empty.");
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

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
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
        if (_buffer.Length == int.MaxValue)
        {
            throw new InvalidOperationException("The maximum stack capacity has been reached.");
        }

        int newBufferLength = (int)BitOperations.RoundUpToPowerOf2((uint)(_buffer.Length + 1));
        T[] newBuffer = GC.AllocateUninitializedArray<T>(newBufferLength);
        _buffer.CopyToUnsafe(newBuffer);
        _buffer = newBuffer;
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

    public void Dispose()
    {
        _bufferLock.Dispose();
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _buffer[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(ConcurrentStack<T>? other)
    {
        return ReferenceEquals(this, other);
    }

    public override bool Equals(object? obj)
    {
        return obj is ConcurrentStack<T> other && Equals(other);
    }

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
