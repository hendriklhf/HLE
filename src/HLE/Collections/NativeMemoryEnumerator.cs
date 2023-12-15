using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

public unsafe struct NativeMemoryEnumerator<T> : IEnumerator<T>, IEquatable<NativeMemoryEnumerator<T>>
    where T : unmanaged
{
    public readonly T Current => _memory[_current];

    readonly object IEnumerator.Current => Current;

    private readonly T* _memory;
    private int _current = -1;
    private readonly int _end;

    public NativeMemoryEnumerator(T* memory, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _memory = memory;
        _end = length - 1;
    }

    public bool MoveNext() => _current++ < _end;

    public void Reset() => _current = 0;

    [Pure]
    public readonly bool Equals(NativeMemoryEnumerator<T> other) => _memory == other._memory && _current == other._current && _end == other._end;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is NativeMemoryEnumerator<T> other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine((nuint)_memory, _end);

    public static bool operator ==(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right) => !(left == right);

    readonly void IDisposable.Dispose()
    {
    }
}
