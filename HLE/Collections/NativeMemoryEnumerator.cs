using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

public unsafe struct NativeMemoryEnumerator<T> : IEnumerator<T>, IEquatable<NativeMemoryEnumerator<T>>
    where T : unmanaged
{
    public T Current => _ptr[_current++];

    object IEnumerator.Current => Current;

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

    [Pure]
    public readonly bool Equals(NativeMemoryEnumerator<T> other) => _ptr == other._ptr && _current == other._current && _end == other._end;

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj) => obj is NativeMemoryEnumerator<T> other && Equals(other);

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => HashCode.Combine((nuint)_ptr, _end);

    public static bool operator ==(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right) => !(left == right);

    readonly void IDisposable.Dispose()
    {
    }
}
