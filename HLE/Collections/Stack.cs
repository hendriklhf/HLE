using System;

namespace HLE.Collections;

public ref struct Stack<T>
{
    public int Count { get; private set; }

    public readonly int Capacity => _stack.Length;

    private readonly Span<T> _stack = Span<T>.Empty;

    public Stack(Span<T> stack)
    {
        _stack = stack;
    }

    public void Push(T item) => _stack[Count++] = item;

    public T Pop() => _stack[--Count];

    public readonly T Peek() => _stack[Count - 1];

    public void Clear() => Count = 0;

    public static implicit operator Stack<T>(Span<T> stack) => new(stack);
}
