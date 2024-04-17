using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace HLE.Memory;

public ref struct MemoryEnumerator<T>
{
    public readonly T Current => Unsafe.Add(ref _memory, _current);

    private readonly ref T _memory;
    private int _current = -1;
    private readonly int _end;

    public MemoryEnumerator(ref T memory, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _memory = ref memory;
        _end = length - 1;
    }

    public bool MoveNext() => _current++ < _end;

    public void Reset() => _current = -1;

    [Pure]
    public readonly bool Equals(MemoryEnumerator<T> other)
        => Unsafe.AreSame(ref _memory, ref other._memory) && _current == other._current && _end == other._end;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly unsafe int GetHashCode() => HashCode.Combine((nuint)Unsafe.AsPointer(ref _memory), _current, _end);

    public static bool operator ==(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => !(left == right);
}
