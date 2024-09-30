using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HLE.Memory;

public ref struct MemoryEnumerator<T> : IEnumerator<T>, IEquatable<MemoryEnumerator<T>>
{
    public readonly T Current => Unsafe.Add(ref _memory, _current);

    readonly object? IEnumerator.Current => Current;

    private readonly ref T _memory;
    private int _current;
    private readonly int _length;

    public static MemoryEnumerator<T> Empty => default;

    public MemoryEnumerator(ref T memory, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _memory = ref memory;
        _length = length;
        _current = -1;
    }

    public MemoryEnumerator(Span<T> items)
    {
        _memory = ref MemoryMarshal.GetReference(items);
        _length = items.Length;
        _current = -1;
    }

    public MemoryEnumerator(ReadOnlySpan<T> items)
    {
        _memory = ref MemoryMarshal.GetReference(items);
        _length = items.Length;
        _current = -1;
    }

    public bool MoveNext() => ++_current < _length;

    public void Reset() => _current = -1;

    readonly void IDisposable.Dispose()
    {
    }

    [Pure]
    public readonly bool Equals(scoped MemoryEnumerator<T> other)
        => Unsafe.AreSame(ref _memory, ref other._memory) &&
           _current == other._current && _length == other._length;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly unsafe int GetHashCode() => HashCode.Combine((nuint)Unsafe.AsPointer(ref _memory), _current, _length);

    public static bool operator ==(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(MemoryEnumerator<T> left, MemoryEnumerator<T> right) => !(left == right);
}
