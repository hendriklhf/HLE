using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public ref struct ValueStack<T>
{
    public int Count { get; private set; }

    public readonly int Capacity => _stack.Length;

    internal readonly Span<T> _stack = [];

    public ValueStack()
    {
    }

    public ValueStack(Span<T> stack, int count = 0)
    {
        _stack = stack;
        Count = count;
    }

    public void Push(T item)
    {
        if (Count == Capacity)
        {
            ThrowStackIsFull();
        }

        _stack[Count++] = item;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStackIsFull() => throw new InvalidOperationException("Stack is full.");

    public T Pop()
    {
        if (Count == 0)
        {
            ThrowStackIsEmpty();
        }

        int index = --Count;
        ref T itemReference = ref _stack[index];
        T item = itemReference;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            itemReference = default!;
        }

        return item;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowStackIsEmpty() => throw new InvalidOperationException("Stack is empty.");

    [Pure]
    public readonly T Peek()
    {
        if (Count == 0)
        {
            ThrowStackIsEmpty();
        }

        return _stack[Count - 1];
    }

    public bool TryPush(T item)
    {
        if (Count >= Capacity)
        {
            return false;
        }

        Push(item);
        return true;
    }

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        if (Count == 0)
        {
            item = default;
            return false;
        }

        item = Pop();
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
            _stack[..Count].Clear();
        }

        Count = 0;
    }

    [Pure]
    public readonly T[] ToArray() => _stack[..Count].ToArray();
}
