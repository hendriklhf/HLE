using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public ref struct MemoryEnumerator<T>
{
    public T Current => Unsafe.Add(ref _reference, _current++);

    private readonly ref T _reference;
    private int _current;
    private readonly int _end;

    public MemoryEnumerator(ref T reference, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _reference = ref reference;
        _end = length;
    }

    public readonly bool MoveNext() => _current < _end;

    public void Reset() => _current = 0;

    [Pure]
    public readonly bool Equals(MemoryEnumerator<T> other) => Unsafe.AreSame(ref _reference, ref other._reference) && _current == other._current && _end == other._end;

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly bool Equals(object? obj) => false;

    [Pure]
    // ReSharper disable once ArrangeModifiersOrder
    public override readonly int GetHashCode() => _end.GetHashCode();

    public static bool operator ==(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => !(left == right);
}
