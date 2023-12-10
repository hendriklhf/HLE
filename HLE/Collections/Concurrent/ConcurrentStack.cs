using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using HLE.Memory;

namespace HLE.Collections.Concurrent;

/// <summary>
/// A concurrent stack that doesn't allocate on pushing items onto the stack, in comparision to <see cref="System.Collections.Concurrent.ConcurrentStack{T}"/>.
/// </summary>
/// <typeparam name="T">The type of stored items.</typeparam>
public sealed class ConcurrentStack<T> : IEquatable<ConcurrentStack<T>>, IReadOnlyCollection<T>, ICopyable<T>, IReadOnlySpanProvider<T>, ICountable,
    IIndexAccessible<T>
{
    T IIndexAccessible<T>.this[int index] => AsSpan()[index];

    public int Count { get; private set; }

    public int Capacity => _buffer.Length;

    internal T[] _buffer;
    private readonly object _syncRoot = new();

    private const int DefaultCapacity = 8;

    public ConcurrentStack(int capacity)
    {
        if (capacity < DefaultCapacity)
        {
            capacity = DefaultCapacity;
        }

        _buffer = GC.AllocateUninitializedArray<T>(capacity);
    }

    public ConcurrentStack() : this(DefaultCapacity)
    {
    }

    public ReadOnlySpan<T> AsSpan() => _buffer.AsSpan(..Count);

    public void Push(T item)
    {
        Monitor.Enter(_syncRoot);
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
            Monitor.Exit(_syncRoot);
        }
    }

    public T Pop()
    {
        Monitor.Enter(_syncRoot);
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
            Monitor.Exit(_syncRoot);
        }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStackIsEmpty() => throw new InvalidOperationException("The stack is empty.");

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        Monitor.Enter(_syncRoot);
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
            Monitor.Exit(_syncRoot);
        }
    }

    public void Clear()
    {
        Monitor.Enter(_syncRoot);
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
            Monitor.Exit(_syncRoot);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowBuffer()
    {
        int newBufferLength = BufferHelpers.GrowArray(_buffer.Length, 1);
        T[] newBuffer = GC.AllocateUninitializedArray<T>(newBufferLength);
        CopyWorker<T> copyWorker = new(_buffer.AsSpan(0, Count));
        copyWorker.CopyTo(newBuffer);
        _buffer = newBuffer;
    }

    public void CopyTo(List<T> destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(T[] destination, int offset = 0)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination, offset);
    }

    public void CopyTo(Memory<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(Span<T> destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    public void CopyTo(ref T destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(ref destination);
    }

    public unsafe void CopyTo(T* destination)
    {
        CopyWorker<T> copyWorker = new(AsSpan());
        copyWorker.CopyTo(destination);
    }

    ReadOnlySpan<T> IReadOnlySpanProvider<T>.GetReadOnlySpan() => AsSpan();

    public ArrayEnumerator<T> GetEnumerator() => new(_buffer, 0, Count);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [Pure]
    public bool Equals([NotNullWhen(true)] ConcurrentStack<T>? other) => ReferenceEquals(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) => ReferenceEquals(this, obj);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public static bool operator ==(ConcurrentStack<T>? left, ConcurrentStack<T>? right) => Equals(left, right);

    public static bool operator !=(ConcurrentStack<T>? left, ConcurrentStack<T>? right) => !(left == right);
}
