using System;
using System.Collections;
using System.Collections.Generic;

namespace HLE.Collections;

public unsafe struct NativeMemoryEnumerator<T> : IEnumerator<T>, IEquatable<NativeMemoryEnumerator<T>>
{
    public T Current => _ptr[_current++];

    object? IEnumerator.Current => Current;

    private readonly T* _ptr;
    private int _current;
    private readonly int _end;

    public NativeMemoryEnumerator(T* ptr, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _ptr = ptr;
        _end = length;
    }

    public readonly bool MoveNext() => _current < _end;

    public void Reset() => _current = 0;

    public readonly bool Equals(NativeMemoryEnumerator<T> other)
    {
        return _ptr == other._ptr && _current == other._current && _end == other._end;
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj)
    {
        return obj is NativeMemoryEnumerator<T> other && Equals(other);
    }

    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode()
    {
        return HashCode.Combine((nuint)_ptr, _end);
    }

    public static bool operator ==(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right)
    {
        return !(left == right);
    }

    readonly void IDisposable.Dispose()
    {
    }
}
