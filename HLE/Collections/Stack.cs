using System;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

public ref struct Stack<T>
{
    public readonly int Count => _count;

    public readonly int Capacity => _stack.Length;

    private readonly Span<T> _stack = Span<T>.Empty;
    private int _count;

    public Stack()
    {
    }

    public Stack(Span<T> stack, int count = 0)
    {
        _stack = stack;
        _count = count;
    }

    public void Push(T item) => _stack[_count++] = item;

    public T Pop() => _stack[--_count];

    [Pure]
    public readonly T Peek() => _stack[_count - 1];

    public bool TryPush(T item)
    {
        if (_count >= Capacity)
        {
            return false;
        }

        Push(item);
        return true;
    }

    public bool TryPop(out T? item)
    {
        if (_count <= 0)
        {
            item = default;
            return false;
        }

        item = Pop();
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

    public void Clear() => _count = 0;

    public static implicit operator Stack<T>(Span<T> stack) => new(stack);

    public static implicit operator Stack<T>(T[] stack) => new(stack);
}
