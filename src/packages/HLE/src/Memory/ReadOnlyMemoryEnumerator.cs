using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Memory;

public ref struct ReadOnlyMemoryEnumerator<T> :
#if NET9_0_OR_GREATER
    IEquatable<ReadOnlyMemoryEnumerator<T>>,
#endif
    IEnumerator<T>
{
    public readonly ref readonly T Current => ref _enumerator.Current;

    readonly T IEnumerator<T>.Current => _enumerator.Current;

    readonly object? IEnumerator.Current => Current;

    private MemoryEnumerator<T> _enumerator;

    public static ReadOnlyMemoryEnumerator<T> Empty => default;

    public ReadOnlyMemoryEnumerator(ref T memory, int length)
        => _enumerator = new(ref memory, length);

    public ReadOnlyMemoryEnumerator(T[] items)
        => _enumerator = new(items);

    public ReadOnlyMemoryEnumerator(Span<T> items)
        => _enumerator = new(items);

    public ReadOnlyMemoryEnumerator(ReadOnlySpan<T> items)
        => _enumerator = new(items);

    public bool MoveNext() => _enumerator.MoveNext();

    [DoesNotReturn]
    void IEnumerator.Reset() => throw new NotSupportedException();

    readonly void IDisposable.Dispose()
    {
    }

    [Pure]
    public readonly bool Equals(scoped ReadOnlyMemoryEnumerator<T> other)
        => _enumerator.Equals(other._enumerator);

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => false;

    [Pure]
    public override readonly int GetHashCode() => _enumerator.GetHashCode();

    public static bool operator ==(ReadOnlyMemoryEnumerator<T> left, ReadOnlyMemoryEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(ReadOnlyMemoryEnumerator<T> left, ReadOnlyMemoryEnumerator<T> right) => !(left == right);
}
