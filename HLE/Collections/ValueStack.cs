using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

public ref struct ValueStack<T>
{
    public readonly int Count => _count;

    public readonly int Capacity => _stack.Length;

    private readonly Span<T> _stack = Span<T>.Empty;
    private int _count;

    public ValueStack()
    {
    }

    public ValueStack(Span<T> stack, int count = 0)
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

    public bool TryPop([MaybeNullWhen(false)] out T item)
    {
        if (_count <= 0)
        {
            item = default;
            return false;
        }

        item = Pop();
        return true;
    }

    public readonly bool TryPeek([MaybeNullWhen(false)] out T item)
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

    [Pure]
    public readonly T[] ToArray()
    {
        return _stack[.._count].ToArray();
    }

    public static implicit operator ValueStack<T>(Span<T> stack) => new(stack);

    public static implicit operator ValueStack<T>(T[] stack) => new(stack);
}
