using System;
using System.Collections;
using System.Collections.Generic;

namespace HLE.Collections;

public struct ArrayEnumerator<T> : IEnumerator<T>, IEquatable<ArrayEnumerator<T>>
{
    public T Current => _array[_current++];

    object? IEnumerator.Current => Current;

    private readonly T[] _array;
    private readonly int _start;
    private int _current;
    private readonly int _end;

    public ArrayEnumerator(T[] array, int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)start, (uint)array.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)length, (uint)array.Length);
        ArgumentOutOfRangeException.ThrowIfNegative(array.Length - start - length);

        _array = array;
        _start = start;
        _current = start;
        _end = _current + length;
    }

    public readonly bool MoveNext() => _current < _end;

    public void Reset() => _current = _start;

    public readonly bool Equals(ArrayEnumerator<T> other)
    {
        return _array == other._array && _current == other._current && _end == other._end;
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj is ArrayEnumerator<T> other && Equals(other);
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_array, _start, _end);
    }

    public static bool operator ==(ArrayEnumerator<T> left, ArrayEnumerator<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArrayEnumerator<T> left, ArrayEnumerator<T> right)
    {
        return !(left == right);
    }

    readonly void IDisposable.Dispose()
    {
    }
}
