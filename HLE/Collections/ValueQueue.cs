using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public ref struct ValueQueue<T>
{
    public int Count { get; private set; }

    public readonly int Capacity => _queue.Length;

    internal readonly Span<T> _queue = [];
    private readonly int _lastIndex;
    private int _enqueueIndex;
    private int _dequeueIndex;

    public ValueQueue()
    {
    }

    public ValueQueue(Span<T> queue)
    {
        _queue = queue;
        _lastIndex = queue.Length - 1;
    }

    public void Enqueue(T item)
    {
        if (_enqueueIndex > _lastIndex)
        {
            if (_dequeueIndex == 0)
            {
                ThrowQueueIsFull();
            }

            // copies the Span to the front of the queue
            Span<T> elementsToCopy = _queue[_dequeueIndex..];
            elementsToCopy.CopyTo(_queue);
            _dequeueIndex = 0;
            _enqueueIndex = elementsToCopy.Length;
        }

        _queue[_enqueueIndex++] = item;
        Count++;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowQueueIsFull() => throw new InvalidOperationException("Queue is full.");

    public T Dequeue()
    {
        if (Count == 0)
        {
            ThrowQueueIsEmpty();
        }

        Count--;
        return _queue[_dequeueIndex++];
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowQueueIsEmpty() => throw new InvalidOperationException("Queue is empty.");

    [Pure]
    public readonly T Peek()
    {
        if (Count == 0)
        {
            ThrowQueueIsEmpty();
        }

        return _queue[_dequeueIndex];
    }

    public bool TryEnqueue(T item)
    {
        if (Count >= Capacity)
        {
            return false;
        }

        Enqueue(item);
        return true;
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T item)
    {
        if (Count == 0)
        {
            item = default;
            return false;
        }

        item = Dequeue();
        return true;
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T item)
    {
        if (Count == 0)
        {
            item = default;
            return false;
        }

        item = Peek();
        return true;
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _queue.Clear();
        }

        Count = 0;
        _enqueueIndex = 0;
        _dequeueIndex = 0;
    }

    [Pure]
    public readonly T[] ToArray() => _queue[_dequeueIndex.._enqueueIndex].ToArray();
}
