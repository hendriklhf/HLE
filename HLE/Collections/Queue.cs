using System;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

public ref struct Queue<T>
{
    public readonly int Count => _count;

    public readonly int Capacity => _queue.Length;

    private readonly Span<T> _queue = Span<T>.Empty;
    private int _count;
    private readonly int _lastIndex;
    private int _enqueueIndex;
    private int _dequeueIndex;

    public Queue()
    {
    }

    public Queue(Span<T> queue)
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
                throw new InvalidOperationException("Queue is full.");
            }

            // copies the Span to the front of the queue
            Span<T> elementsToCopy = _queue[_dequeueIndex..];
            elementsToCopy.CopyTo(_queue);
            _dequeueIndex = 0;
            _enqueueIndex = elementsToCopy.Length;
        }

        _queue[_enqueueIndex++] = item;
        _count++;
    }

    public T Dequeue()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        _count--;
        return _queue[_dequeueIndex++];
    }

    [Pure]
    public readonly T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        return _queue[_dequeueIndex];
    }

    public bool TryEnqueue(T item)
    {
        if (_count >= Capacity)
        {
            return false;
        }

        Enqueue(item);
        return true;
    }

    public bool TryDequeue(out T? item)
    {
        if (_count <= 0)
        {
            item = default;
            return false;
        }

        item = Dequeue();
        return true;
    }

    public readonly bool TryPeek(out T? item)
    {
        if (_count <= 0)
        {
            item = default;
            return false;
        }

        item = Peek();
        return true;
    }

    public void Clear()
    {
        _count = 0;
        _enqueueIndex = 0;
        _dequeueIndex = 0;
    }

    [Pure]
    public readonly T[] ToArray()
    {
        return _queue[_dequeueIndex.._enqueueIndex].ToArray();
    }

    public static implicit operator Queue<T>(Span<T> queue) => new(queue);

    public static implicit operator Queue<T>(T[] queue) => new(queue);
}
