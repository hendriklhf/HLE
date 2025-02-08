using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Memory;

public unsafe struct NativeMemoryEnumerator<T> : IEnumerator<T>, IBitwiseEquatable<NativeMemoryEnumerator<T>>
    where T : unmanaged
{
    public T* Current { get; private set; }

    readonly T IEnumerator<T>.Current => *Current;

    readonly object IEnumerator.Current => *Current;

    private readonly T* _end;

    public static NativeMemoryEnumerator<T> Empty => new(null, 0);

    public NativeMemoryEnumerator(T* memory, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Current = memory - 1;
        _end = memory + length;
    }

    public bool MoveNext() => ++Current != _end;

    void IEnumerator.Reset() => throw new NotSupportedException();

    readonly void IDisposable.Dispose()
    {
    }

    [Pure]
    public readonly bool Equals(NativeMemoryEnumerator<T> other)
        => Current == other.Current && _end == other._end;

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
        => obj is NativeMemoryEnumerator<T> other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => HashCode.Combine((nuint)Current, (nuint)_end);

    public static bool operator ==(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(NativeMemoryEnumerator<T> left, NativeMemoryEnumerator<T> right) => !(left == right);
}
