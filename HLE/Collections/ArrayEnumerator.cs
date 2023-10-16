using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Collections;

public struct ArrayEnumerator<T> : IEnumerator<T>, IEquatable<ArrayEnumerator<T>>
{
    public T Current => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_array), _current++);

    object? IEnumerator.Current => Current;

    private readonly T[] _array;
    private readonly int _start;
    private int _current;
    private readonly int _end;

    public ArrayEnumerator(T[] array, int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative((uint)array.Length - (uint)start - (uint)length);

        _array = array;
        _start = start;
        _current = start;
        _end = _current + length;
    }

    public readonly bool MoveNext() => _current < _end;

    public void Reset() => _current = _start;

    [Pure]
    public readonly bool Equals(ArrayEnumerator<T> other) => _array == other._array && _current == other._current && _end == other._end;

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj) => obj is ArrayEnumerator<T> other && Equals(other);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => HashCode.Combine(_array, _start, _end);

    public static bool operator ==(ArrayEnumerator<T> left, ArrayEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(ArrayEnumerator<T> left, ArrayEnumerator<T> right) => !(left == right);

    readonly void IDisposable.Dispose()
    {
    }
}
