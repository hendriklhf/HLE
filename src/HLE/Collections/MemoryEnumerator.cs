using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Collections;

public ref struct MemoryEnumerator<T>
{
    public readonly T Current => Unsafe.Add(ref _reference, _current);

    private readonly ref T _reference;
    private int _current = -1;
    private readonly int _end;

    public MemoryEnumerator(ref T reference, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _reference = ref reference;
        _end = length - 1;
    }

    public bool MoveNext() => _current++ < _end;

    public void Reset() => _current = 0;

    [Pure]
    public readonly bool Equals(MemoryEnumerator<T> other)
        => Unsafe.AreSame(ref _reference, ref other._reference) && _current == other._current && _end == other._end;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => _end.GetHashCode();

    public static bool operator ==(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => !(left == right);
}
