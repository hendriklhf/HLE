using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace HLE.Collections;

// ReSharper disable once UseNameofExpressionForPartOfTheString
[DebuggerDisplay("Current: {Current}")]
public struct ReadOnlyArrayEnumerator<T> : IEnumerator<T>, IEquatable<ReadOnlyArrayEnumerator<T>>
{
    public readonly ref readonly T Current => ref _enumerator.Current;

    readonly T IEnumerator<T>.Current => Current;

    readonly object? IEnumerator.Current => Current;

    private ArrayEnumerator<T> _enumerator;

    public static ReadOnlyArrayEnumerator<T> Empty => default;

    public ReadOnlyArrayEnumerator(T[] array) => _enumerator = new(array);

    public ReadOnlyArrayEnumerator(T[] array, int start, int length) => _enumerator = new(array, start, length);

    public ReadOnlyArrayEnumerator(List<T> list) => _enumerator = new(list);

    public bool MoveNext() => _enumerator.MoveNext();

    [DoesNotReturn]
    readonly void IEnumerator.Reset() => throw new NotSupportedException();

    readonly void IDisposable.Dispose()
    {
    }

    [Pure]
    public readonly bool Equals(ReadOnlyArrayEnumerator<T> other) => _enumerator.Equals(other._enumerator);

    [Pure]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
        => obj is ReadOnlyArrayEnumerator<T> other && Equals(other);

    [Pure]
    public override readonly int GetHashCode() => _enumerator.GetHashCode();

    public static bool operator ==(ReadOnlyArrayEnumerator<T> left, ReadOnlyArrayEnumerator<T> right) => left.Equals(right);

    public static bool operator !=(ReadOnlyArrayEnumerator<T> left, ReadOnlyArrayEnumerator<T> right) => !(left == right);
}
